using System.Text.RegularExpressions;
using AdbInstaller.Models;

namespace AdbInstaller.Services;

public sealed class AdbService(string adbPath)
{
	public string AdbPath => adbPath;

	/// <summary>Looks in the usual places a Windows machine with Android Studio or just platform-tools would
	/// have adb.exe: ANDROID_HOME/ANDROID_SDK_ROOT, the default Android Studio SDK install location, and
	/// finally PATH. Returns null if none of them have it -- the caller then asks the user to browse for it.</summary>
	public static string? FindAdb()
	{
		List<string> candidates = [];
		foreach (string? envVar in new[] { "ANDROID_HOME", "ANDROID_SDK_ROOT" })
		{
			string? root = Environment.GetEnvironmentVariable(envVar);
			if (!string.IsNullOrEmpty(root)) candidates.Add(Path.Combine(root, "platform-tools", "adb.exe"));
		}
		candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "platform-tools", "adb.exe"));

		foreach (string candidate in candidates)
			if (File.Exists(candidate)) return candidate;

		// PATH lookup: "where adb" (Windows' equivalent of "which"), first match wins.
		try
		{
			var result = ProcessRunner.RunAsync("where", "adb.exe").GetAwaiter().GetResult();
			string? first = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
			if (result.ExitCode == 0 && first != null && File.Exists(first)) return first;
		}
		catch { /* "where" itself missing or erroring: fall through to "not found" */ }

		return null;
	}

	/// <summary>Parses "adb devices -l": first token is the serial (a hardware id for USB, "ip:port" for Wi-Fi,
	/// see <see cref="DeviceItem.IsWifi"/>), second is the state, and a "model:xxx" token (not always present,
	/// e.g. while "unauthorized") gives a friendlier name than the raw serial.</summary>
	public async Task<List<DeviceItem>> ListDevicesAsync(CancellationToken ct = default)
	{
		ProcessResult result = await ProcessRunner.RunAsync(adbPath, "devices -l", ct: ct);
		var devices = new List<DeviceItem>();
		foreach (string line in result.Output.Split('\n'))
		{
			string trimmed = line.Trim();
			if (trimmed.Length == 0 || trimmed.StartsWith("List of devices")) continue;
			string[] parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2) continue;

			string serial = parts[0], state = parts[1];
			string model = parts.FirstOrDefault(p => p.StartsWith("model:"))?[6..].Replace('_', ' ') ?? serial;
			devices.Add(new DeviceItem { Serial = serial, State = state, DisplayName = model });
		}
		return devices;
	}

	/// <summary>Android 11+ wireless debugging, first-time pairing: the IP:port and 6-digit code come from the
	/// device's own "Pair device with pairing code" screen (a different port than the one used to actually
	/// connect afterward -- see <see cref="ConnectAsync"/>).</summary>
	public async Task<(bool Success, string Message)> PairAsync(string ip, string port, string code, CancellationToken ct = default)
	{
		ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"pair {ip}:{port} {code}", ct: ct);
		bool success = Regex.IsMatch(result.Output, "Successfully paired", RegexOptions.IgnoreCase);
		return (success, result.Output.Trim());
	}

	/// <summary>The IP:port shown on the device's main "Wireless debugging" screen -- only works once the
	/// device has been paired at least once (see <see cref="PairAsync"/>; a device that was paired earlier in
	/// an app-session elsewhere stays paired, only the connect port tends to change each time Wi-Fi debugging
	/// is toggled on).</summary>
	public async Task<(bool Success, string Message)> ConnectAsync(string ip, string port, CancellationToken ct = default)
	{
		ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"connect {ip}:{port}", ct: ct);
		bool success = Regex.IsMatch(result.Output, "connected to", RegexOptions.IgnoreCase) && !result.Output.Contains("failed", StringComparison.OrdinalIgnoreCase);
		return (success, result.Output.Trim());
	}

	/// <summary>Nudges one already-known device (USB or Wi-Fi, "adb reconnect &lt;serial&gt;" handles both)
	/// back to a working connection -- the per-row retry for a device currently showing "offline".</summary>
	public async Task<(bool Success, string Message)> ReconnectAsync(string serial, CancellationToken ct = default)
	{
		ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"reconnect {serial}", ct: ct);
		return (result.ExitCode == 0, result.Output.Trim());
	}

	/// <summary>"-r": reinstall, keeping the app's data -- the normal thing a dev wants when repeatedly pushing
	/// a build to the same test device, not a fresh install every time.</summary>
	public Task<ProcessResult> InstallApkAsync(string serial, string apkPath, Action<string>? onLine, CancellationToken ct = default) =>
		ProcessRunner.RunAsync(adbPath, $"-s {serial} install -r \"{apkPath}\"", onLine, ct);
}
