using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Startup decisions made from the user settings: game paths, UI culture and map name.
/// </summary>
public class StartupService(
	ILogger<StartupService> log
	, UserSettings userSettings
	, IGamePathService gamePathService
	, IFileIO fileIO
	, IDirectoryIO directoryIO
) : IStartupService
{
	private readonly ILogger<StartupService> _log = log;
	private readonly UserSettings _userSettings = userSettings;
	private readonly IGamePathService _gamePathService = gamePathService;
	private readonly IFileIO _fileIO = fileIO;
	private readonly IDirectoryIO _directoryIO = directoryIO;

	public string? ResolveUICultureName()
	{
		string? settingsCulture = null;
		if (!string.IsNullOrEmpty(this._userSettings.UILanguage))
			settingsCulture = this._userSettings.UILanguage;
		else if (!this._userSettings.AutoDetectLanguage)
			settingsCulture = this._userSettings.TQLanguage;

		return FindNeutralCultureName(settingsCulture);
	}

	/// <summary>
	/// Finds the neutral culture whose English name is the given language, such as "French" for "fr".
	/// </summary>
	/// <returns>The culture name, or null when the language is empty or unknown.</returns>
	public static string? FindNeutralCultureName(string? englishName)
	{
		if (string.IsNullOrEmpty(englishName))
			return null;

		foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
		{
			if (ci.EnglishName.Equals(englishName, StringComparison.OrdinalIgnoreCase))
				return string.IsNullOrEmpty(ci.TextInfo.CultureName) ? null : ci.TextInfo.CultureName;
		}

		return null;
	}

	public StartupNotice SetupGamePaths()
	{
		if (this._userSettings.AutoDetectGamePath)
		{
			this._gamePathService.GamePathTQ = this._gamePathService.ResolveGamePath();
			this._gamePathService.GamePathTQIT = this._gamePathService.ResolveGamePath();
		}
		else
		{
			this._gamePathService.GamePathTQ = this._userSettings.TQPath;
			this._gamePathService.GamePathTQIT = this._userSettings.TQITPath;
		}

		// The Immortal Throne path wins when both are set.
		foreach (var gamePath in new[] { this._gamePathService.GamePathTQ, this._gamePathService.GamePathTQIT })
		{
			if (!string.IsNullOrWhiteSpace(gamePath))
				this._gamePathService.GameType = this.DetectGameType(gamePath);
		}

		this._log.LogInformation("Selected TQ path {GamePathTQ}", this._gamePathService.GamePathTQ);
		this._log.LogInformation("Selected TQIT path {GamePathTQIT}", this._gamePathService.GamePathTQIT);

		var notice = StartupNotice.None;
		if (string.IsNullOrEmpty(this._userSettings.VaultPath))
		{
			string folderPath = Path.Combine(this._gamePathService.SaveFolderTQ, "TQVaultData");

			// A shortcut used to give the vault path: the configuration UI replaces it.
			notice = !this._directoryIO.Exists(folderPath) && this._fileIO.Exists(Path.ChangeExtension(folderPath, ".lnk"))
				? StartupNotice.LegacyVaultShortcut
				: StartupNotice.DefaultVaultPath;
		}

		this._gamePathService.TQVaultSaveFolder = this._userSettings.VaultPath;
		return notice;
	}

	/// <summary>
	/// Gets the game type from the executable found in a game folder.
	/// </summary>
	public GameType DetectGameType(string gamePath)
	{
		if (this._fileIO.Exists(Path.Combine(gamePath, "TQ.exe")))
			return GameType.TQAE;

		if (this._fileIO.Exists(Path.Combine(gamePath, "TQIT.exe")))
			return GameType.TQIT;

		return GameType.TQ;
	}

	public void SetupMapName()
	{
		// The command line can override this in MainForm.LoadResources.
		this._gamePathService.MapName = this._userSettings.ModEnabled ? this._userSettings.CustomMap : "main";
	}
}
