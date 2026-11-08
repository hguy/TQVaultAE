using TQVaultAE.Application;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

/// <summary>
/// What the main form selects once the resources are loaded.
/// </summary>
/// <param name="CharacterName">The last character to select, or null to keep the selection.</param>
/// <param name="VaultName">The vault to load in the main panel.</param>
/// <param name="SkipTitle">Whether the splash screen closes at once.</param>
/// <param name="AutomaticPlayer">When the title is skipped, the start of the character name to select: the command line player, or empty.</param>
public sealed record StartupSelection(string? CharacterName, string VaultName, bool SkipTitle, string AutomaticPlayer)
{
	/// <summary>
	/// Applies the startup rules: the last character and vault when their auto load is on, the Main Vault when the last
	/// vault is not set or its file is missing, and skipping the title for the command line or the setting.
	/// </summary>
	/// <param name="settings">The user settings.</param>
	/// <param name="characterNames">The names of the characters in the list.</param>
	/// <param name="vaultExists">Tells whether the file of a vault exists.</param>
	/// <param name="args">The command line.</param>
	public static StartupSelection Resolve(UserSettings settings, IEnumerable<string> characterNames, Func<string, bool> vaultExists, CommandLineArgs args)
	{
		string? characterName = null;
		if (settings.LoadLastCharacter && characterNames.Contains(settings.LastCharacterName, StringComparer.Ordinal))
			characterName = settings.LastCharacterName;

		string vaultName = VaultService.MAINVAULT;
		if (settings.LoadLastVault)
		{
			vaultName = settings.LastVaultName;

			// Do not create a vault here: load the Main Vault instead.
			if (string.IsNullOrEmpty(vaultName) || !vaultExists(vaultName))
				vaultName = VaultService.MAINVAULT;
		}

		var skipTitle = args.IsAutomatic || settings.SkipTitle;

		return new StartupSelection(characterName, vaultName, skipTitle, skipTitle ? args.Player ?? string.Empty : string.Empty);
	}
}
