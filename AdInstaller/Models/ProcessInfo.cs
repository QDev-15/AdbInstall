namespace AdbInstaller.Models;

/// <summary>One row of "adb shell top -b -n 1 -o PID,%CPU,RES,ARGS" -- a process snapshot at the moment the
/// command ran (not a continuously live value; the caller re-runs this on a timer for a "real-time" feel).</summary>
public sealed class ProcessInfo
{
    public required int Pid { get; init; }
    public required double CpuPercent { get; init; }
    public required long ResidentKb { get; init; }
    public required string Name { get; init; }

    public double ResidentMb => ResidentKb / 1024.0;
}
