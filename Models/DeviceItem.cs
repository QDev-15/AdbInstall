namespace AdbInstaller.Models;

/// <summary>One line of "adb devices -l" output: a device adb currently knows about, whether or not it is
/// actually reachable right now (a USB device can show "unauthorized" or "offline"; a Wi-Fi device that was
/// connected earlier and dropped off the network still shows up, state "offline").</summary>
public sealed class DeviceItem
{
	/// <summary>USB device: a hardware serial (e.g. "RF8M81MKLTA"). Wi-Fi device: "ip:port" -- that shape is
	/// how <see cref="IsWifi"/> is decided, adb does not label this explicitly.</summary>
	public required string Serial { get; init; }

	/// <summary>"device" (ready to use), "offline", "unauthorized", "no permissions"... -- whatever adb printed.</summary>
	public required string State { get; init; }

	/// <summary>From the "model:" field adb prints alongside -l, when present; falls back to the serial.</summary>
	public string DisplayName { get; init; } = "";

	public bool IsWifi => Serial.Contains(':');

	public bool IsConnected => State == "device";

	public override string ToString() => $"{DisplayName} ({Serial})";
}
