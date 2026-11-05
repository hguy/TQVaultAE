//-----------------------------------------------------------------------
// <copyright file="GameLocator.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace TQVaultAE.DbCli.Core;

/// <summary>Located game install.</summary>
/// <param name="Root">Install folder.</param>
/// <param name="Database">database.arz path.</param>
/// <param name="Archives">Every .arc under the install (Resources, Resources\XPack*, Audio, Text, Toolset).</param>
public sealed record GameInstall(string Root, string Database, IReadOnlyList<string> Archives)
{
	/// <summary>Text archive for a language (e.g. "EN" → Text\Text_EN.arc), <see langword="null"/> when missing.</summary>
	public string? TextArchive(string language)
	{
		var p = System.IO.Path.Combine(this.Root, "Text", $"Text_{language.ToUpperInvariant()}.arc");
		return File.Exists(p) ? p : null;
	}
}

/// <summary>Finds the game install.</summary>
public static partial class GameLocator
{
	/// <summary>Environment variable that overrides detection.</summary>
	public const string EnvironmentVariable = "TQ_GAME_PATH";

	internal const string SteamFolderName = "Titan Quest Anniversary Edition";
	internal const string GogGameId = "1196955511";

	private static readonly string[] WellKnownFolders = [
		"""C:\Program Files (x86)\Steam\steamapps\common\Titan Quest Anniversary Edition""",
		"""C:\Program Files\Steam\steamapps\common\Titan Quest Anniversary Edition""",
		"""C:\GOG Games\Titan Quest - Anniversary Edition""",
		"""C:\Program Files (x86)\GOG Galaxy\Games\Titan Quest - Anniversary Edition""",
		"""D:\SteamLibrary\steamapps\common\Titan Quest Anniversary Edition""",
		"""E:\SteamLibrary\steamapps\common\Titan Quest Anniversary Edition""",
		"""D:\Games\Titan Quest Anniversary Edition""",
		"""C:\Games\Titan Quest Anniversary Edition""",
	];

	/// <summary>
	/// Order: <paramref name="explicitRoot"/>, env TQ_GAME_PATH, Steam (registry + libraryfolders.vdf, app 475150),
	/// GOG registry, well-known folders. Throws <see cref="DirectoryNotFoundException"/> when not found.
	/// </summary>
	public static GameInstall Locate(string? explicitRoot = null)
	{
		if (!string.IsNullOrWhiteSpace(explicitRoot)) {
			return IsGameRoot(explicitRoot)
				? FromRoot(explicitRoot)
				: throw new DirectoryNotFoundException($"'{explicitRoot}' is not a Titan Quest install (Database\\database.arz not found).");
		}

		var env = Environment.GetEnvironmentVariable(EnvironmentVariable);
		if (!string.IsNullOrWhiteSpace(env)) {
			return IsGameRoot(env)
				? FromRoot(env)
				: throw new DirectoryNotFoundException($"{EnvironmentVariable}='{env}' is not a Titan Quest install (Database\\database.arz not found).");
		}

		foreach (var candidate in Candidates())
			if (IsGameRoot(candidate))
				return FromRoot(candidate);

		throw new DirectoryNotFoundException($"Titan Quest Anniversary Edition not found. Pass the install folder explicitly or set {EnvironmentVariable}.");
	}

	/// <summary>Builds the install description for a known root (enumerates every .arc recursively).</summary>
	public static GameInstall FromRoot(string root)
	{
		var full = Path.GetFullPath(root.Trim().Trim('"'));
		var archives = Directory.EnumerateFiles(full, "*.arc", SearchOption.AllDirectories)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		return new GameInstall(full, DatabasePath(full), archives);
	}

	/// <summary>True when <paramref name="root"/> contains Database\database.arz.</summary>
	internal static bool IsGameRoot(string root)
	{
		try {
			return File.Exists(DatabasePath(root.Trim().Trim('"')));
		} catch (ArgumentException) {
			return false;
		}
	}

	internal static string DatabasePath(string root) => Path.Combine(root, "Database", "database.arz");

	/// <summary>Library roots listed in a Steam libraryfolders.vdf ("path" keys, unescaped).</summary>
	internal static IReadOnlyList<string> ParseLibraryFolders(string vdf)
	{
		var result = new List<string>();
		foreach (Match m in VdfPath().Matches(vdf))
			result.Add(m.Groups[1].Value.Replace("""\\""", """\"""));
		return result;
	}

	private static IEnumerable<string> Candidates()
	{
		if (OperatingSystem.IsWindows()) {
			foreach (var c in SteamCandidates())
				yield return c;
			foreach (var c in GogCandidates())
				yield return c;
		}
		foreach (var c in WellKnownFolders)
			yield return c;
	}

	[SupportedOSPlatform("windows")]
	private static IEnumerable<string> SteamCandidates()
	{
		var steam = ReadRegistry(Microsoft.Win32.Registry.CurrentUser, """Software\Valve\Steam""", "SteamPath")
			?? ReadRegistry(Microsoft.Win32.Registry.LocalMachine, """SOFTWARE\WOW6432Node\Valve\Steam""", "InstallPath")
			?? ReadRegistry(Microsoft.Win32.Registry.LocalMachine, """SOFTWARE\Valve\Steam""", "InstallPath");
		if (steam is null)
			yield break;

		steam = steam.Replace('/', '\\');
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

	[SupportedOSPlatform("windows")]
	private static IEnumerable<string> GogCandidates()
	{
		foreach (var key in new[] { $"""SOFTWARE\WOW6432Node\GOG.com\Games\{GogGameId}""", $"""SOFTWARE\GOG.com\Games\{GogGameId}""" }) {
			var path = ReadRegistry(Microsoft.Win32.Registry.LocalMachine, key, "PATH")
				?? ReadRegistry(Microsoft.Win32.Registry.LocalMachine, key, "path");
			if (path is not null)
				yield return path;
		}
	}

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
}
