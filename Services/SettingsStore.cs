using System.Text.Json;

namespace AdbInstaller.Services;

public sealed class AppSettings
{
	public string? AdbPath { get; set; }
}

/// <summary>Tiny JSON file in %AppData% -- just remembers adb.exe's path once the user has pointed at it
/// manually (auto-detection, see <see cref="AdbService.FindAdb"/>, does not always find it), so they are not
/// asked again on every launch.</summary>
public static class SettingsStore
{
	private static readonly string FilePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdbInstaller", "settings.json");

	public static AppSettings Load()
	{
		try
		{
			if (File.Exists(FilePath))
				return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
		}
		catch { /* corrupt/unreadable: start fresh rather than crash the app over a settings file */ }
		return new AppSettings();
	}

	public static void Save(AppSettings settings)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
		File.WriteAllText(FilePath, JsonSerializer.Serialize(settings));
	}
}
