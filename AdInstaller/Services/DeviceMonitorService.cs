using System.Globalization;
using System.Text.RegularExpressions;
using AdbInstaller.Models;

namespace AdbInstaller.Services;

/// <summary>Read-only device diagnostics over "adb shell" -- everything here is a plain shell read command, no
/// "shell" user write/control action (no force-stop, no disable) by design for this first phase: the goal is
/// to see which process is eating CPU/RAM/battery while the phone is actively being used, not to manage apps.</summary>
public sealed class DeviceMonitorService(string adbPath)
{
    /// <summary>"-o PID,%CPU,RES,ARGS": toybox top's column-select flag, so the output has a fixed, parseable
    /// shape regardless of OEM/Android version instead of relying on free-text column alignment. "-b -n 1":
    /// one non-interactive snapshot (not a live-updating screen) -- cheap enough to re-run every 1-2s from a
    /// timer for a "real-time" feel without noticeably loading an already-struggling phone.</summary>
    public async Task<List<ProcessInfo>> ListTopProcessesAsync(string serial, CancellationToken ct = default)
    {
        ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"-s {serial} shell top -b -n 1 -o PID,%CPU,RES,ARGS", ct: ct);
        var processes = new List<ProcessInfo>();
        foreach (string line in result.Output.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0) continue;
            string[] parts = trimmed.Split((char[]?)null, 4, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) continue;
            if (!int.TryParse(parts[0], out int pid)) continue; // skips the header row and any other non-data line
            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double cpu)) continue;
            // ARGS is the full command line (e.g. "wpa_supplicant -O/data/vendor/... -dd ..."); only the first
            // token -- the package name for an app process, the binary name for a native daemon -- is kept as
            // the display Name, so the grid reads like a process list instead of a wall of command-line flags.
            string name = parts[3].Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries)[0];
            processes.Add(new ProcessInfo { Pid = pid, CpuPercent = cpu, ResidentKb = ParseResidentKb(parts[2]), Name = name });
        }
        return processes.OrderByDescending(p => p.CpuPercent).ToList();
    }

    /// <summary>toybox prints RES in KB while small, then switches to an "M" suffix once a process crosses
    /// 1 GB resident -- normalizing both to KB here keeps <see cref="ProcessInfo.ResidentMb"/> simple.</summary>
    private static long ParseResidentKb(string token)
    {
        token = token.Trim();
        if (token.EndsWith('M') && double.TryParse(token[..^1], NumberStyles.Any, CultureInfo.InvariantCulture, out double mb))
            return (long)(mb * 1024);
        if (token.EndsWith('K') && long.TryParse(token[..^1], out long kb))
            return kb;
        return long.TryParse(token, out long raw) ? raw : 0;
    }

    public async Task<BatteryInfo?> GetBatteryInfoAsync(string serial, CancellationToken ct = default)
    {
        ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"-s {serial} shell dumpsys battery", ct: ct);
        Dictionary<string, string> values = ParseKeyValueLines(result.Output);
        if (!values.TryGetValue("level", out string? levelStr) || !int.TryParse(levelStr, out int level)) return null;

        int temperatureTenths = values.TryGetValue("temperature", out string? t) && int.TryParse(t, out int temp) ? temp : 0;
        // BatteryManager.BATTERY_STATUS_CHARGING = 2, BATTERY_STATUS_FULL = 5 -- both read as "charging" here.
        bool charging = values.GetValueOrDefault("status") is "2" or "5";
        return new BatteryInfo { LevelPercent = level, TemperatureCelsius = temperatureTenths / 10.0, IsCharging = charging };
    }

    public async Task<MemoryInfo?> GetMemoryInfoAsync(string serial, CancellationToken ct = default)
    {
        ProcessResult result = await ProcessRunner.RunAsync(adbPath, $"-s {serial} shell cat /proc/meminfo", ct: ct);
        var values = new Dictionary<string, long>();
        foreach (string line in result.Output.Split('\n'))
        {
            Match match = Regex.Match(line, @"^(\w+):\s+(\d+)\s*kB");
            if (match.Success) values[match.Groups[1].Value] = long.Parse(match.Groups[2].Value);
        }
        if (!values.TryGetValue("MemTotal", out long total)) return null;
        long available = values.TryGetValue("MemAvailable", out long a) ? a : values.GetValueOrDefault("MemFree");
        return new MemoryInfo { TotalKb = total, AvailableKb = available };
    }

    private static Dictionary<string, string> ParseKeyValueLines(string output)
    {
        var values = new Dictionary<string, string>();
        foreach (string line in output.Split('\n'))
        {
            int colon = line.IndexOf(':');
            if (colon <= 0) continue;
            values[line[..colon].Trim()] = line[(colon + 1)..].Trim();
        }
        return values;
    }
}
