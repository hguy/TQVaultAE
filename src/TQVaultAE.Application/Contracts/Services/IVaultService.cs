using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

public interface IVaultService
{
	/// <summary>
	/// Creates a new empty vault file.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <param name="file">File name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	PlayerCollection CreateVault(string name, string file);

	/// <summary>
	/// Creates a new empty vault with auto-generated filename.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	PlayerCollection CreateVault(string name);

	/// <summary>
	/// Loads a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <returns>Vault load result with vault and error information.</returns>
	VaultLoadResult LoadVault(string vaultName);

	/// <summary>
	/// Gets the list of available vault names.
	/// </summary>
	/// <returns>List of vault names.</returns>
	IReadOnlyList<string> GetVaultList();

	/// <summary>
	/// Checks a vault maintenance request before it runs.
	/// </summary>
	/// <param name="action">The maintenance function.</param>
	/// <param name="source">The vault to copy, delete or rename; ignored for <see cref="VaultMaintenance.New"/>.</param>
	/// <param name="target">The new vault name, already trimmed; ignored for <see cref="VaultMaintenance.Delete"/>.</param>
	/// <param name="existingVaults">The names of the existing vaults.</param>
	/// <returns><see cref="VaultMaintenanceError.None"/> when the request is valid, else the first problem found.</returns>
	VaultMaintenanceError ValidateMaintenance(VaultMaintenance action, string? source, string? target, IEnumerable<string> existingVaults);

	/// <summary>
	/// Copies a vault file when the source exists and the target does not.
	/// </summary>
	/// <param name="sourceName">Name of the vault to copy.</param>
	/// <param name="targetName">Name of the new vault.</param>
	/// <returns>True if the file was copied.</returns>
	bool CopyVault(string sourceName, string targetName);

	/// <summary>
	/// Deletes a vault file when it exists and drops it from the session cache.
	/// </summary>
	/// <param name="vaultName">Name of the vault to delete.</param>
	/// <returns>True if a file was deleted.</returns>
	bool DeleteVault(string vaultName);

	/// <summary>
	/// Renames a vault file when the old one exists and the new one does not,
	/// and drops both names from the session cache.
	/// </summary>
	/// <param name="oldName">Current name of the vault.</param>
	/// <param name="newName">New name for the vault.</param>
	/// <returns>True if the file was renamed.</returns>
	bool RenameVault(string oldName, string newName);

	/// <summary>
	/// Attempts to save all modified vault files.
	/// </summary>
	/// <param name="vaultOnError">If save fails, this will contain the vault that failed.</param>
	/// <returns>Number of vaults saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	int SaveAllModifiedVaults(ref PlayerCollection? vaultOnError);

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	void UpdateVaultPath(string vaultPath);

	/// <summary>
	/// Saves a vault to disk.
	/// </summary>
	/// <param name="vault">The vault to save.</param>
	/// <param name="vaultName">Name of the vault.</param>
	void SaveVault(PlayerCollection vault, string vaultName);

	/// <summary>
	/// Shows the vault maintenance dialog.
	/// </summary>
	void ShowVaultMaintenance();
}
