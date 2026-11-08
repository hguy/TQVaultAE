using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Startup decisions made from the user settings and the command line.
/// </summary>
public interface IStartupService
{
	/// <summary>
	/// Gets the name of the culture to show the UI in, or null to keep the culture of the system.
	/// </summary>
	string? ResolveUICultureName();

	/// <summary>
	/// Sets the game paths, auto detected or configured, the game type and the vault folder.
	/// </summary>
	/// <returns>The message the caller shows about the vault folder.</returns>
	StartupNotice SetupGamePaths();

	/// <summary>
	/// Sets the map name: the custom map when mods are enabled, else the main map.
	/// </summary>
	void SetupMapName();
}
