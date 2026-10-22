using System.IO.Compression;
using System.Text.Json;

namespace AdbInstaller.Services;

/// <summary>
/// adb cannot install an .aab directly -- an Android App Bundle is Play's own upload format, not something a
/// device can run. The standard way to get one onto a real device is Google's own bundletool: it builds a
/// single universal APK from the bundle (build-apks --mode=universal), which is then installed with a plain
/// "adb install -r" like any other .apk. Needs a JRE on PATH (not bundled here -- out of scope for a small dev
/// tool) and bundletool.jar itself, downloaded once from its GitHub releases into %LocalAppData%\AdbInstaller\
/// if not already there.
/// </summary>
public sealed class BundletoolService(string adbPath)
{
	private static readonly string CacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdbInstaller");
	private static readonly string JarPath = Path.Combine(CacheDir, "bundletool.jar");
	private static readonly string KeystorePath = Path.Combine(CacheDir, "debug.keystore");
	private const string KeystorePassword = "android";
	private const string KeyAlias = "androiddebugkey";

	public static bool IsCached => File.Exists(JarPath);

	/// <summary>bundletool only auto-signs with a debug key if ~/.android/debug.keystore (the one Android
	/// Studio normally generates on first run) already exists on this machine -- otherwise it silently builds
	/// UNSIGNED APKs (just a warning on its own stderr), which then fail to install with a cryptic
	/// "INSTALL_PARSE_FAILED_NO_CERTIFICATES ... Attempt to get length of null array". Generating and caching
	/// our own debug keystore here makes a sideload work regardless of whether Android Studio has ever run on
	/// this machine. Same well-known parameters Android tooling itself uses for a debug key -- this is for
	/// local test installs only, never Play's own upload signature.</summary>
	private static async Task<string> EnsureDebugKeystoreAsync(CancellationToken ct)
	{
		if (File.Exists(KeystorePath)) return KeystorePath;
		Directory.CreateDirectory(CacheDir);
		string tmp = KeystorePath + ".tmp";
		try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* best-effort */ }

		string? java = FindJava();
		if (java == null) throw new InvalidOperationException("Không tìm thấy Java để chạy keytool.");
		string keytool = Path.Combine(Path.GetDirectoryName(java)!, "keytool.exe");
		if (!File.Exists(keytool))
			throw new InvalidOperationException("Tìm thấy Java nhưng thiếu keytool (cần bản JDK đầy đủ, không chỉ JRE): " + keytool);

		ProcessResult result = await ProcessRunner.RunAsync(keytool,
			$"-genkeypair -keystore \"{tmp}\" -alias {KeyAlias} -storepass {KeystorePassword} -keypass {KeystorePassword} " +
			$"-keyalg RSA -keysize 2048 -validity 10000 -dname \"CN=Android Debug,O=Android,C=US\"", null, ct);
		if (result.ExitCode != 0 || !File.Exists(tmp))
			throw new InvalidOperationException("Không tạo được keystore debug để ký APK (cần keytool, đi kèm JDK):\n" + result.Output);
		File.Move(tmp, KeystorePath, overwrite: true);
		return KeystorePath;
	}

	public static Task<bool> IsJavaAvailableAsync() => Task.FromResult(FindJava() != null);

	/// <summary>Looks for java.exe the same way <see cref="AdbService.FindAdb"/> looks for adb.exe: a plain
	/// "java" on PATH is the common case everywhere else, but a machine set up only for Android development often
	/// has a JDK bundled with Android Studio / the SDK command-line tools and never added to PATH or JAVA_HOME --
	/// e.g. "Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe". Returns null if none of them have it.</summary>
	private static string? FindJava()
	{
		string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
		if (!string.IsNullOrEmpty(javaHome))
		{
			string candidate = Path.Combine(javaHome, "bin", "java.exe");
			if (File.Exists(candidate)) return candidate;
		}

		try
		{
			ProcessResult result = ProcessRunner.RunAsync("where", "java.exe").GetAwaiter().GetResult();
			string? first = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
			if (result.ExitCode == 0 && first != null && File.Exists(first)) return first;
		}
		catch { /* "where" itself missing or erroring: fall through to the bundled-JDK search below */ }

		List<string> roots = [];
		foreach (string pf in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) })
		{
			if (string.IsNullOrEmpty(pf)) continue;
			roots.Add(Path.Combine(pf, "Android", "openjdk")); // Android Studio's own bundled JDK (newer Studio versions)
			roots.Add(Path.Combine(pf, "Android", "jdk")); // older Studio / SDK Manager "Android SDK" JDK component
			roots.Add(Path.Combine(pf, "Android", "Android Studio", "jbr")); // Android Studio's JetBrains Runtime
		}
		roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Android Studio", "jbr"));

		foreach (string root in roots)
		{
			if (!Directory.Exists(root)) continue;
			try
			{
				// Multiple JDK versions can be installed side by side (e.g. a leftover jdk-8 next to jdk-21);
				// OrderByDescending on the full path puts the newest version first often enough for this --
				// a small dev tool's "good enough" heuristic, not exact version parsing.
				string? found = Directory.EnumerateFiles(root, "java.exe", SearchOption.AllDirectories).OrderByDescending(p => p).FirstOrDefault();
				if (found != null) return found;
			}
			catch { /* unreadable directory etc.: skip this root */ }
		}

		return null;
	}

	/// <summary>Downloads the latest bundletool-all-*.jar from GitHub releases (one-time; cached afterward).
	/// Reports progress lines the same way an install does, so the caller can show them next to the
	/// progress bar instead of the UI just looking stuck during the download. <paramref name="onProgress"/>
	/// (0-100) is real, byte-counted progress -- unlike build-apks/install-apks below, a file download is the
	/// one phase here where we actually know a total size to measure against.</summary>
	public static async Task<string> EnsureDownloadedAsync(Action<string>? onLine, Action<int>? onProgress = null, CancellationToken ct = default)
	{
		if (IsCached) return JarPath;
		Directory.CreateDirectory(CacheDir);

		using var http = new HttpClient();
		http.DefaultRequestHeaders.UserAgent.ParseAdd("AdbInstaller");
		onLine?.Invoke("Đang tìm bản bundletool mới nhất...");
		string releaseJson = await http.GetStringAsync("https://api.github.com/repos/google/bundletool/releases/latest", ct);
		using JsonDocument doc = JsonDocument.Parse(releaseJson);
		string? downloadUrl = doc.RootElement.GetProperty("assets").EnumerateArray()
			.Select(a => a.GetProperty("browser_download_url").GetString())
			.FirstOrDefault(u => u != null && u.Contains("bundletool-all") && u.EndsWith(".jar"));
		if (downloadUrl == null) throw new InvalidOperationException("Không tìm thấy file bundletool-all-*.jar trong bản phát hành mới nhất trên GitHub.");

		onLine?.Invoke("Đang tải bundletool.jar...");
		string tmp = JarPath + ".tmp";
		using (HttpResponseMessage response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
		{
			response.EnsureSuccessStatusCode();
			long? total = response.Content.Headers.ContentLength;
			await using Stream s = await response.Content.ReadAsStreamAsync(ct);
			await using FileStream f = File.Create(tmp);
			byte[] buffer = new byte[81920];
			long readSoFar = 0;
			int read;
			while ((read = await s.ReadAsync(buffer, ct)) > 0)
			{
				await f.WriteAsync(buffer.AsMemory(0, read), ct);
				readSoFar += read;
				if (total is > 0) onProgress?.Invoke((int)(readSoFar * 100 / total.Value));
			}
		}
		File.Move(tmp, JarPath, overwrite: true);
		onLine?.Invoke("Đã tải xong bundletool.jar.");
		return JarPath;
	}

	/// <summary>build-apks --mode=universal (one single APK with every density/ABI/language, instead of
	/// per-device split APKs) then a plain "adb install -r" on the extracted universal.apk -- the exact same,
	/// already-proven path used for .apk files elsewhere in this app. Device-specific SPLIT installs (the
	/// previous approach, via bundletool's own "install-apks") hit a real Android/bundletool bug on some
	/// devices: a config split like base-xxhdpi.apk fails with "INSTALL_PARSE_FAILED_NO_CERTIFICATES ...
	/// Attempt to get length of null array" even though the bundle itself is fine. A universal APK has no
	/// splits to go wrong, at the cost of a somewhat larger download (everything bundled into one file).
	/// <paramref name="onProgress"/> reports (percent 0-100, current-stage label): build-apks/adb install are
	/// each one opaque external command with no internal progress of their own, so -- unlike the jar download
	/// above -- these are step milestones (jump on start/finish), not continuous byte progress.</summary>
	public async Task InstallAabAsync(string aabPath, string serial, Action<string>? onLine, Action<int, string>? onProgress = null, CancellationToken ct = default)
	{
		string? java = FindJava();
		if (java == null)
			throw new InvalidOperationException("Cần cài Java (JRE/JDK) để cài file .aab qua bundletool -- không tìm thấy lệnh \"java\" (đã tìm cả PATH, JAVA_HOME và các JDK đi kèm Android Studio/SDK).");

		string jar = IsCached ? JarPath : await EnsureDownloadedAsync(onLine, pct => onProgress?.Invoke(pct * 15 / 100, "Đang tải bundletool.jar"), ct);
		onProgress?.Invoke(15, "Đang chuẩn bị bundletool");
		string apksPath = Path.Combine(Path.GetTempPath(), $"adbinstaller_{Guid.NewGuid():N}.apks");
		string extractDir = Path.Combine(Path.GetTempPath(), $"adbinstaller_{Guid.NewGuid():N}");
		try
		{
			string keystore = await EnsureDebugKeystoreAsync(ct);

			onProgress?.Invoke(15, "Đang tạo APK universal từ AAB (build-apks)");
			onLine?.Invoke("Đang tạo APK universal từ AAB (build-apks --mode=universal)...");
			ProcessResult build = await ProcessRunner.RunAsync(java,
				$"-jar \"{jar}\" build-apks --bundle=\"{aabPath}\" --output=\"{apksPath}\" --mode=universal --overwrite " +
				$"--ks=\"{keystore}\" --ks-pass=pass:{KeystorePassword} --ks-key-alias={KeyAlias} --key-pass=pass:{KeystorePassword}",
				onLine, ct);
			if (build.ExitCode != 0) throw new InvalidOperationException("build-apks thất bại:\n" + build.Output);

			onProgress?.Invoke(55, "Đang giải nén APK universal");
			ZipFile.ExtractToDirectory(apksPath, extractDir);
			string universalApk = Path.Combine(extractDir, "universal.apk");
			if (!File.Exists(universalApk)) throw new InvalidOperationException("Không tìm thấy universal.apk trong gói build-apks.");

			onProgress?.Invoke(70, "Đang cài đặt lên thiết bị (adb install)");
			onLine?.Invoke("Đang cài APK universal lên thiết bị (adb install)...");
			ProcessResult install = await ProcessRunner.RunAsync(adbPath, $"-s {serial} install -r \"{universalApk}\"", onLine, ct);
			if (install.ExitCode != 0 || !install.Output.Contains("Success", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("adb install thất bại:\n" + install.Output);
			onProgress?.Invoke(100, "Đã cài đặt xong");
		}
		finally
		{
			try { if (File.Exists(apksPath)) File.Delete(apksPath); } catch { /* best-effort cleanup */ }
			try { if (Directory.Exists(extractDir)) Directory.Delete(extractDir, recursive: true); } catch { /* best-effort cleanup */ }
		}
	}
}
