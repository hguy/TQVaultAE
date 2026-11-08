using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Runs the save of the session: modified characters, vaults and stashes, then the configuration, then the git backup.
/// </summary>
public class SaveOrchestrator(
	IPlayerService playerService
	, IVaultService vaultService
	, IStashService stashService
	, ISettingsPersistenceService settingsPersistenceService
	, IGameFileService gameFileService
	, UserSettings userSettings
) : ISaveOrchestrator
{
	private readonly IPlayerService _playerService = playerService;
	private readonly IVaultService _vaultService = vaultService;
	private readonly IStashService _stashService = stashService;
	private readonly ISettingsPersistenceService _settingsPersistenceService = settingsPersistenceService;
	private readonly IGameFileService _gameFileService = gameFileService;
	private readonly UserSettings _userSettings = userSettings;

	public SaveResult Save(SaveRequest request, Func<SaveError, SaveDecision> onError)
	{
		var errors = new List<SaveError>();
		bool playersSaved = false;
		int vaultsSaved = 0;
		int stashesSaved = 0;

		if (request.Files)
		{
			PlayerCollection? playerOnError = null;
			playersSaved = RunStep(SaveStep.Players, () => this._playerService.SaveAllModifiedPlayers(ref playerOnError) ? 1 : 0, () => playerOnError?.PlayerName, onError, errors) > 0;

			PlayerCollection? vaultOnError = null;
			vaultsSaved = RunStep(SaveStep.Vaults, () => this._vaultService.SaveAllModifiedVaults(ref vaultOnError), () => vaultOnError?.PlayerName, onError, errors);

			Stash? stashOnError = null;
			stashesSaved = RunStep(SaveStep.Stashes, () => this._stashService.SaveAllModifiedStashes(ref stashOnError), () => stashOnError?.PlayerName, onError, errors);
		}

		bool? configChanged = null;
		if (request.Configuration is { } configuration)
			configChanged = this._settingsPersistenceService.SaveConfiguration(configuration.VaultName, configuration.CharacterName, configuration.Zoom, configuration.ConfigChanged);

		// After the files, so the commit holds them.
		bool gitPushed = request.GitBackup
			&& this._userSettings.GitBackupEnabled
			&& this._gameFileService.GitAddCommitTagAndPush();

		return new SaveResult
		{
			PlayersSaved = playersSaved,
			VaultsSaved = vaultsSaved,
			StashesSaved = stashesSaved,
			Errors = errors,
			ConfigChanged = configChanged,
			GitPushed = gitPushed,
		};
	}

	/// <summary>
	/// Runs a save step until it succeeds, is ignored or is aborted.
	/// </summary>
	/// <param name="step">The step.</param>
	/// <param name="save">The save; returns how many files were saved.</param>
	/// <param name="nameInError">The name of the file being saved when the error happened.</param>
	/// <param name="onError">Decides after a file error.</param>
	/// <param name="errors">Receives each file error.</param>
	/// <returns>How many files the last run saved, 0 when the step was ignored.</returns>
	private static int RunStep(SaveStep step, Func<int> save, Func<string?> nameInError, Func<SaveError, SaveDecision> onError, List<SaveError> errors)
	{
		while (true)
		{
			try
			{
				return save();
			}
			catch (IOException exception)
			{
				var error = new SaveError(step, nameInError(), exception);
				errors.Add(error);
				switch (onError(error))
				{
					case SaveDecision.Abort:
						throw;
					case SaveDecision.Retry:
						continue;
					default:
						return 0;
				}
			}
		}
	}
}
