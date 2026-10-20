using System.Text.Json;

namespace AdbInstaller.Services;

/// <summary>
/// adb cannot install an .aab directly -- an Android App Bundle is Play's own upload format, not something a
/// device can run. The standard way to test one on a real device is Google's own bundletool: it builds a
/// device-specific "APK set" from the bundle (build-apks), then pushes that set to the device (install-apks).
/// Needs a JRE on PATH (not bundled here -- out of scope for a small dev tool) and bundletool.jar itself,
/// downloaded once from its GitHub releases into %LocalAppData%\AdbInstaller\ if not already there.
/// </summary>
public sealed class BundletoolService(string adbPath)
{
	private static readonly string CacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdbInstaller");
	private static readonly string JarPath = Path.Combine(CacheDir, "bundletool.jar");

	public static bool IsCached => File.Exists(JarPath);

	public static async Task<bool> IsJavaAvailableAsync()
	{
		try
		{
			ProcessResult result = await ProcessRunner.RunAsync("java", "-version");
			return result.ExitCode == 0;
		}
		catch { return false; }
	}

	/// <summary>Downloads the latest bundletool-all-*.jar from GitHub releases (one-time; cached afterward).
	/// Reports progress lines the same way an install does, so the caller can show them next to the
	/// progress bar instead of the UI just looking stuck during the download.</summary>
	public static async Task<string> EnsureDownloadedAsync(Action<string>? onLine, CancellationToken ct = default)
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
		await using (Stream s = await http.GetStreamAsync(downloadUrl, ct))
		await using (FileStream f = File.Create(tmp))
			await s.CopyToAsync(f, ct);
		File.Move(tmp, JarPath, overwrite: true);
		onLine?.Invoke("Đã tải xong bundletool.jar.");
		return JarPath;
	}

	/// <summary>build-apks (device-specific split APKs, auto-signed with a throwaway debug key for local
	/// testing -- Play's own upload signature is irrelevant here, the device just needs SOME valid signature)
	/// then install-apks (pushes that set to the one device named by <paramref name="serial"/>).</summary>
	public async Task InstallAabAsync(string aabPath, string serial, Action<string>? onLine, CancellationToken ct = default)
	{
		string jar = await EnsureDownloadedAsync(onLine, ct);
		string apksPath = Path.Combine(Path.GetTempPath(), $"adbinstaller_{Guid.NewGuid():N}.apks");
		try
		{
			onLine?.Invoke("Đang tạo bộ APK từ AAB cho thiết bị này (build-apks)...");
			ProcessResult build = await ProcessRunner.RunAsync("java",
				$"-jar \"{jar}\" build-apks --bundle=\"{aabPath}\" --output=\"{apksPath}\" --connected-device --device-id={serial} --adb=\"{adbPath}\" --overwrite",
				onLine, ct);
			if (build.ExitCode != 0) throw new InvalidOperationException("build-apks thất bại:\n" + build.Output);

			onLine?.Invoke("Đang cài bộ APK lên thiết bị (install-apks)...");
			ProcessResult install = await ProcessRunner.RunAsync("java",
				$"-jar \"{jar}\" install-apks --apks=\"{apksPath}\" --device-id={serial} --adb=\"{adbPath}\"",
				onLine, ct);
			if (install.ExitCode != 0) throw new InvalidOperationException("install-apks thất bại:\n" + install.Output);
		}
		finally
		{
			try { if (File.Exists(apksPath)) File.Delete(apksPath); } catch { /* best-effort cleanup */ }
		}
	}
}
