namespace AdbInstaller.Models;

/// <summary>From "adb shell dumpsys battery" -- the plain key: value block it prints, not the much heavier
/// "dumpsys batterystats" (per-app drain history), which this app deliberately does not poll.</summary>
public sealed class BatteryInfo
{
    public required int LevelPercent { get; init; }
    public required double TemperatureCelsius { get; init; }
    public required bool IsCharging { get; init; }
}
