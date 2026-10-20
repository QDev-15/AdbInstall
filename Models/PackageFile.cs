namespace AdbInstaller.Models;

public enum PackageKind
{
	Apk,
	Aab,
}

/// <summary>One file added to the install list.</summary>
public sealed class PackageFile
{
	public required string FullPath { get; init; }
	public string FileName => Path.GetFileName(FullPath);
	public PackageKind Kind { get; init; }

	public static PackageFile? TryCreate(string path)
	{
		string ext = Path.GetExtension(path).ToLowerInvariant();
		PackageKind? kind = ext switch
		{
			".apk" => PackageKind.Apk,
			".aab" => PackageKind.Aab,
			_ => null,
		};
		return kind == null ? null : new PackageFile { FullPath = path, Kind = kind.Value };
	}

	public override string ToString() => $"{FileName}  [{Kind.ToString().ToUpperInvariant()}]";
}
