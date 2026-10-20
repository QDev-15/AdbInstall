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

	/// <summary>build-apks (device-specific split APKs, auto-signed with a throwaway debug key for local
	/// testing -- Play's own upload signature is irrelevant here, the device just needs SOME valid signature)
	/// then install-apks (pushes that set to the one device named by <paramref name="serial"/>).
	/// <paramref name="onProgress"/> reports (percent 0-100, current-stage label): build-apks/install-apks
	/// are each one opaque external command with no internal progress of their own, so -- unlike the jar
	/// download above -- these are step milestones (jump on start/finish), not continuous byte progress.</summary>
	public async Task InstallAabAsync(string aabPath, string serial, Action<string>? onLine, Action<int, string>? onProgress = null, CancellationToken ct = default)
	{
		string jar = IsCached ? JarPath : await EnsureDownloadedAsync(onLine, pct => onProgress?.Invoke(pct * 15 / 100, "Đang tải bundletool.jar"), ct);
		onProgress?.Invoke(15, "Đang chuẩn bị bundletool");
		string apksPath = Path.Combine(Path.GetTempPath(), $"adbinstaller_{Guid.NewGuid():N}.apks");
		try
		{
			onProgress?.Invoke(15, "Đang tạo bộ APK cho thiết bị (build-apks)");
			onLine?.Invoke("Đang tạo bộ APK từ AAB cho thiết bị này (build-apks)...");
			ProcessResult build = await ProcessRunner.RunAsync("java",
				$"-jar \"{jar}\" build-apks --bundle=\"{aabPath}\" --output=\"{apksPath}\" --connected-device --device-id={serial} --adb=\"{adbPath}\" --overwrite",
				onLine, ct);
			if (build.ExitCode != 0) throw new InvalidOperationException("build-apks thất bại:\n" + build.Output);

			onProgress?.Invoke(60, "Đang cài đặt lên thiết bị (install-apks)");
			onLine?.Invoke("Đang cài bộ APK lên thiết bị (install-apks)...");
			ProcessResult install = await ProcessRunner.RunAsync("java",
				$"-jar \"{jar}\" install-apks --apks=\"{apksPath}\" --device-id={serial} --adb=\"{adbPath}\"",
				onLine, ct);
			if (install.ExitCode != 0) throw new InvalidOperationException("install-apks thất bại:\n" + install.Output);
			onProgress?.Invoke(100, "Đã cài đặt xong");
		}
		finally
		{
			try { if (File.Exists(apksPath)) File.Delete(apksPath); } catch { /* best-effort cleanup */ }
		}
	}
}
