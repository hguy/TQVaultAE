namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Stores the state of the session in the user settings.
/// </summary>
public interface ISettingsPersistenceService
{
	/// <summary>
	/// Records the last vault and character (when their auto load is on), the zoom and the custom map state,
	/// clears the language and game paths that are auto detected, then saves the settings if anything changed.
	/// </summary>
	/// <param name="vaultName">The vault shown in the main panel, or null.</param>
	/// <param name="characterName">The selected character, an empty string when none is selected, or null.</param>
	/// <param name="zoom">The current UI zoom.</param>
	/// <param name="configChanged">Whether the settings were already changed in this session.</param>
	/// <returns>Whether the settings are changed, so the caller keeps it for the next call.</returns>
	bool SaveConfiguration(string? vaultName, string? characterName, float zoom, bool configChanged);
}
