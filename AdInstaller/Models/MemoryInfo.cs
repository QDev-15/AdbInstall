namespace AdbInstaller.Models;

/// <summary>From "adb shell cat /proc/meminfo" -- the raw kernel interface, not "dumpsys meminfo" (whose
/// per-app breakdown format varies a lot more across Android versions/OEMs); this only needs the stable
/// MemTotal/MemAvailable fields for an overall RAM summary.</summary>
public sealed class MemoryInfo
{
    public required long TotalKb { get; init; }
    public required long AvailableKb { get; init; }

    public long UsedKb => TotalKb - AvailableKb;
    public double TotalMb => TotalKb / 1024.0;
    public double UsedMb => UsedKb / 1024.0;
    public double AvailableMb => AvailableKb / 1024.0;
    public double UsedPercent => TotalKb == 0 ? 0 : UsedKb * 100.0 / TotalKb;
}
