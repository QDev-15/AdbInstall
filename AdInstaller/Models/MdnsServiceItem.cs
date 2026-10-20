namespace AdbInstaller.Models;

/// <summary>One line of "adb mdns services": a device currently broadcasting Android 11+ wireless debugging on
/// the local network -- "_adb-tls-pairing._tcp" while its "Pair device with pairing code" screen is open (still
/// needs the 6-digit code shown there), "_adb-tls-connect._tcp" once already paired and its main "Wireless
/// debugging" screen is open (no code needed, a plain "adb connect" works).</summary>
public sealed class MdnsServiceItem
{
    public required string Name { get; init; }
    public required string Ip { get; init; }
    public required string Port { get; init; }
    public required bool IsPairing { get; init; }

    public override string ToString() =>
        IsPairing ? $"{Ip}:{Port} — chờ ghép nối (cần mã hiện trên máy)" : $"{Ip}:{Port} — đã ghép nối, kết nối trực tiếp";
}
