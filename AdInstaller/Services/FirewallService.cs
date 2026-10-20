using System.Diagnostics;

namespace AdbInstaller.Services;

/// <summary>One-click fix for the most common reason "Dò mạng Wi-Fi" finds nothing: when Windows classifies the
/// current network as "Public", Windows Firewall silently drops inbound mDNS (UDP 5353) multicast, so adb's own
/// mdns backend never sees the phone's broadcast even though the phone is sending it fine. Adds one narrow allow
/// rule for that single port instead of touching the network's Public/Private category (a bigger, security-
/// relevant choice the user should make for themselves, not something this app should flip silently).</summary>
public static class FirewallService
{
	private const string RuleName = "ADB wireless debugging discovery (mDNS 5353)";

	/// <summary>Read-only "netsh advfirewall firewall show rule" -- no elevation needed just to check.</summary>
	public static async Task<bool> IsMdnsRuleEnabledAsync(CancellationToken ct = default)
	{
		ProcessResult result = await ProcessRunner.RunAsync("netsh", $"advfirewall firewall show rule name=\"{RuleName}\"", ct: ct);
		return result.Output.Contains("Enabled:", StringComparison.OrdinalIgnoreCase)
			&& !result.Output.Contains("No rules match", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Runs "netsh advfirewall firewall add rule ..." elevated (one UAC prompt) to open inbound UDP
	/// 5353. Verb="runas" requires UseShellExecute=true, which rules out stdout redirection -- the exit code is
	/// all we get back. A cancelled UAC prompt surfaces as a Win32Exception (ERROR_CANCELLED), not a bad exit
	/// code.</summary>
	public static async Task<(bool Success, string Message)> EnableMdnsRuleAsync(CancellationToken ct = default)
	{
		string args = $"advfirewall firewall add rule name=\"{RuleName}\" dir=in action=allow protocol=UDP localport=5353 profile=any";
		var psi = new ProcessStartInfo("netsh", args)
		{
			UseShellExecute = true,
			Verb = "runas",
			WindowStyle = ProcessWindowStyle.Hidden,
		};
		try
		{
			using Process? process = Process.Start(psi);
			if (process == null) return (false, "Không khởi chạy được netsh.");
			await process.WaitForExitAsync(ct);
			return process.ExitCode == 0
				? (true, "Đã mở cổng mDNS (UDP 5353) trong tường lửa.")
				: (false, $"netsh báo lỗi (mã {process.ExitCode}).");
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return (false, "Bạn đã từ chối cấp quyền quản trị, chưa mở được tường lửa.");
		}
	}
}
