//-----------------------------------------------------------------------
// <copyright file="InstallLocations.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace TQVaultAE.GameFiles;

/// <summary>
/// Where Steam and GOG install Titan Quest Anniversary Edition, read from the registry and from
/// Steam's libraryfolders.vdf. Shared by TQVault and tqdb.
/// </summary>
public static partial class InstallLocations
{
	/// <summary>Steam app id of Titan Quest Anniversary Edition.</summary>
	public const string SteamAppId = "475150";

	/// <summary>Folder of the game under steamapps\common.</summary>
	public const string SteamFolderName = "Titan Quest Anniversary Edition";

	/// <summary>GOG game id of Titan Quest Anniversary Edition.</summary>
	public const string GogGameId = "1196955511";

	/// <summary>
	/// Folders where Steam may have installed the game: <c>&lt;library&gt;\steamapps\common\Titan Quest Anniversary Edition</c>
	/// for the Steam folder and each library it lists. Empty when Steam is not installed or not on Windows.
	/// The folders are not checked.
	/// </summary>
	public static IEnumerable<string> SteamCandidates()
	{
		if (!OperatingSystem.IsWindows())
			yield break;

		var steam = SteamRoot();
		if (steam is null)
			yield break;

		var libraries = new List<string> { steam };
		var vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
		string? content = null;
		try {
			if (File.Exists(vdf))
				content = File.ReadAllText(vdf);
		} catch (IOException) {
		} catch (UnauthorizedAccessException) {
		}
		if (content is not null)
			libraries.AddRange(ParseLibraryFolders(content));

		foreach (var lib in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
			yield return Path.Combine(lib, "steamapps", "common", SteamFolderName);
	}

	/// <summary>Install folders that GOG registered for the game (64 and 32-bit registry views). Empty when not on Windows.</summary>
	public static IEnumerable<string> GogCandidates()
	{
		if (!OperatingSystem.IsWindows())
			yield break;

		foreach (var key in new[] { $"""SOFTWARE\GOG.com\Games\{GogGameId}""", $"""SOFTWARE\WOW6432Node\GOG.com\Games\{GogGameId}""" }) {
			var path = ReadRegistry(Microsoft.Win32.Registry.LocalMachine, key, "PATH");
			if (path is not null)
				yield return path;
		}
	}

	/// <summary>
	/// Library roots listed in a Steam libraryfolders.vdf, with backslashes unescaped: the <c>"path"</c> keys of
	/// the current format, or the <c>"1"		"D:\\games\\Steam"</c> entries of the old one.
	/// </summary>
	public static IReadOnlyList<string> ParseLibraryFolders(string vdf)
	{
		var result = new List<string>();
		foreach (Match m in VdfPath().Matches(vdf))
			result.Add(Unescape(m.Groups[1].Value));

		if (result.Count == 0) {
			foreach (Match m in VdfOldPath().Matches(vdf))
				result.Add(Unescape(m.Groups[1].Value));
		}

		return result;
	}

	private static string Unescape(string value) => value.Replace("""\\""", """\""", StringComparison.Ordinal);

	/// <summary>The Steam install folder, with backslashes.</summary>
	[SupportedOSPlatform("windows")]
	private static string? SteamRoot()
		=> (ReadRegistry(Microsoft.Win32.Registry.CurrentUser, """Software\Valve\Steam""", "SteamPath")
			?? ReadRegistry(Microsoft.Win32.Registry.LocalMachine, """SOFTWARE\WOW6432Node\Valve\Steam""", "InstallPath")
			?? ReadRegistry(Microsoft.Win32.Registry.LocalMachine, """SOFTWARE\Valve\Steam""", "InstallPath"))
			?.Replace('/', '\\');

	[SupportedOSPlatform("windows")]
	private static string? ReadRegistry(Microsoft.Win32.RegistryKey hive, string subKey, string name)
	{
		try {
			using var key = hive.OpenSubKey(subKey);
			return key?.GetValue(name) is string s && s.Length > 0 ? s : null;
		} catch (System.Security.SecurityException) {
			return null;
		} catch (IOException) {
			return null;
		} catch (UnauthorizedAccessException) {
			return null;
		}
	}

	[GeneratedRegex("""
		"path"\s+"((?:[^"\\]|\\.)*)"
		""", RegexOptions.IgnoreCase)]
	private static partial Regex VdfPath();

	[GeneratedRegex("""
		^\s*"\d+"\s+"((?:[^"\\]|\\.)*)"\s*$
		""", RegexOptions.Multiline)]
	private static partial Regex VdfOldPath();
}
