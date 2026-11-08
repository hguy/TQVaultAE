using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Config;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

public class VaultService : IVaultService
{
	private readonly ILogger Log;
	private readonly SessionContext userContext;
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly IGamePathService GamePathResolver;
	private readonly IGameFileService GameFileService;
	private readonly IPlayerCollectionProvider PlayerCollectionProvider;
	private readonly IFileIO FileIO;
	private readonly IPathIO PathIO;
	private readonly UserSettings UserSettings;
	public const string MAINVAULT = "Main Vault";

	public VaultService(
		ILogger<VaultService> log,
		SessionContext userContext,
		IItemDatabaseService itemDatabaseService,
		IPlayerCollectionProvider playerCollectionProvider,
		IGamePathService gamePathResolver,
		IGameFileService iGameFileService,
		IFileIO fileIO,
		IPathIO pathIO,
		UserSettings userSettings)
	{
		this.Log = log;
		this.userContext = userContext;
		this.ItemDatabaseService = itemDatabaseService;
		this.GamePathResolver = gamePathResolver;
		this.GameFileService = iGameFileService;
		this.PlayerCollectionProvider = playerCollectionProvider;
		this.FileIO = fileIO;
		this.PathIO = pathIO;
		this.UserSettings = userSettings;
	}

	/// <summary>
	/// Creates a new empty vault file.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <param name="file">File name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	public PlayerCollection CreateVault(string name, string file)
	{
		// From .json to .vault
		var oldFormatfileName = this.PathIO.Combine(this.PathIO.GetDirectoryName(file), this.PathIO.GetFileNameWithoutExtension(file));

		PlayerCollection vault = new PlayerCollection(name, file);
		vault.IsVault = true;

		// Convert by reading old format
		if (this.FileIO.Exists(oldFormatfileName))
		{
			LoadVault(vault, oldFormatfileName);

			if (vault.Sacks is { Length: > 0 } sacks)
				sacks[0].IsModified = true;// Force Save into json on closing

			return vault;
		}

		// CreatNew
		vault.CreateEmptySacks(12); // number of bags
		return vault;
	}

	/// <summary>
	/// Creates a new empty vault with auto-generated filename.
	/// </summary>
	/// <param name="name">Name of the vault.</param>
	/// <returns>Player instance of the new vault.</returns>
	public PlayerCollection CreateVault(string name)
	{
		var fileName = $"{name}.json";
		return CreateVault(name, fileName);
	}

	/// <summary>
	/// Attempts to save all modified vault files.
	/// </summary>
	/// <param name="vaultOnError">If save fails, this will contain the vault that failed.</param>
	/// <returns>Number of vaults saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	public int SaveAllModifiedVaults(ref PlayerCollection? vaultOnError)
	{
		int saved = 0;

		foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in this.userContext.Vaults)
		{
			string vaultFile = kvp.Key;
			PlayerCollection vault = kvp.Value.Value;

			if (vault == null) continue;

			if (vault.IsModified)
			{
				// backup the file
				vaultOnError = vault;

				if (!this.UserSettings.DisableLegacyBackup)
					GameFileService.BackupFile(vault.PlayerName, vaultFile);

				PlayerCollectionProvider.Save(vault, vaultFile);
				vault.Saved();
				saved++;
			}
		}
		return saved;
	}

	/// <summary>
	/// Loads a vault file.
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <returns>Vault load result with vault and error information.</returns>
	public VaultLoadResult LoadVault(string vaultName)
	{
		var result = new VaultLoadResult();

		try
		{
			// Get the filename
			result.Filename = GamePathResolver.GetVaultFile(vaultName);

			// Check the cache
			var resultVault = this.userContext.Vaults.GetOrAddAtomic(result.Filename, k =>
			{
				PlayerCollection pc;
				// We need to load vault.
				if (!this.FileIO.Exists(k))
				{
					// the file does not exist so create a new vault or convert old format to Json.
					pc = this.CreateVault(vaultName, k);
					pc.VaultLoaded = true;
				}
				else
				{
					pc = new PlayerCollection(vaultName, k);
					pc.IsVault = true;
					LoadVault(pc, k);
					pc.VaultLoaded = true;
				}

				// Add vault items to the search database
				int sackNumber = -1;
				foreach (var sack in pc)
				{
					sackNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack)
						this.ItemDatabaseService.AddItemToDatabase(item, k, vaultName, sackNumber, SackType.Vault);
				}

				return pc;
			});
			result.Vault = resultVault;
			result.VaultLoaded = resultVault.VaultLoaded;
			result.ArgumentException = resultVault.ArgumentException;
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "Failed to load vault {VaultName}", vaultName);
			result.Error = ex;
		}

		return result;
	}

	/// <summary>
	/// Gets the list of available vault names.
	/// </summary>
	/// <returns>List of vault names.</returns>
	public IReadOnlyList<string> GetVaultList()
	{
		try
		{
			var fileNames = Directory.EnumerateFiles(this.GamePathResolver.TQVaultSaveFolder, "*.json").ToList();

			return fileNames
				.Select(f => this.PathIO.GetFileNameWithoutExtension(f))
				.ToList()
				.AsReadOnly();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			Log.LogWarning(ex, "Unable to list the vaults in {Folder}", this.GamePathResolver.TQVaultSaveFolder);
			return Array.Empty<string>();
		}
	}

	/// <inheritdoc />
	public VaultMaintenanceError ValidateMaintenance(VaultMaintenance action, string? source, string? target, IEnumerable<string> existingVaults)
	{
		var existing = existingVaults as ICollection<string> ?? existingVaults.ToList();

		bool Exists(string? name)
			=> name is not null && existing.Any(vault => name.Equals(vault, StringComparison.OrdinalIgnoreCase));

		bool needsSource = action is VaultMaintenance.Copy or VaultMaintenance.Delete or VaultMaintenance.Rename;
		bool needsTarget = action is VaultMaintenance.New or VaultMaintenance.Copy or VaultMaintenance.Rename;

		if (needsTarget && string.IsNullOrWhiteSpace(target))
			return VaultMaintenanceError.EmptyName;

		if (needsSource && !Exists(source))
			return VaultMaintenanceError.SourceMissing;

		if (needsTarget)
		{
			if (target!.IndexOfAny(this.PathIO.GetInvalidFileNameChars()) != -1)
				return VaultMaintenanceError.InvalidCharacters;

			if (Exists(target))
				return VaultMaintenanceError.DuplicateName;
		}

		return VaultMaintenanceError.None;
	}

	/// <inheritdoc />
	public bool CopyVault(string sourceName, string targetName)
	{
		var sourceFileName = this.GamePathResolver.GetVaultFile(sourceName);
		var targetFileName = this.GamePathResolver.GetVaultFile(targetName);

		if (!this.FileIO.Exists(sourceFileName) || this.FileIO.Exists(targetFileName))
			return false;

		this.FileIO.Copy(sourceFileName, targetFileName);

		// A vault created but never saved under the target name may sit in the cache.
		this.userContext.Vaults.TryRemove(targetFileName, out _);
		return true;
	}

	/// <inheritdoc />
	public bool DeleteVault(string vaultName)
	{
		var fileName = this.GamePathResolver.GetVaultFile(vaultName);
		bool deleted = false;

		if (this.FileIO.Exists(fileName))
		{
			this.FileIO.Delete(fileName);
			deleted = true;
		}

		this.userContext.Vaults.TryRemove(fileName, out _);
		return deleted;
	}

	/// <inheritdoc />
	public bool RenameVault(string oldName, string newName)
	{
		var oldFileName = this.GamePathResolver.GetVaultFile(oldName);
		var newFileName = this.GamePathResolver.GetVaultFile(newName);

		if (!this.FileIO.Exists(oldFileName) || this.FileIO.Exists(newFileName))
			return false;

		this.FileIO.Move(oldFileName, newFileName);

		this.userContext.Vaults.TryRemove(oldFileName, out _);
		// The Main Vault, or a vault created but never saved, may sit in the cache under the new name.
		this.userContext.Vaults.TryRemove(newFileName, out _);
		return true;
	}

	/// <summary>
	/// Updates VaultPath key from the configuration UI.
	/// Needed since all vaults will need to be reloaded if this key changes.
	/// </summary>
	/// <param name="vaultPath">Path to the vault files</param>
	public void UpdateVaultPath(string vaultPath)
		=> this.UserSettings.VaultPath = vaultPath;

	/// <summary>
	/// Saves a vault to disk and registers it in the session cache.
	/// </summary>
	/// <param name="vault">The vault to save.</param>
	/// <param name="vaultName">Name of the vault.</param>
	public void SaveVault(PlayerCollection vault, string vaultName)
	{
		var fileName = GamePathResolver.GetVaultFile(vaultName);
		PlayerCollectionProvider.Save(vault, fileName);
		vault.Saved();

		userContext.Vaults.GetOrAddAtomic(fileName, _ => vault);
	}

	/// <summary>
	/// Shows the vault maintenance dialog.
	/// </summary>
	public void ShowVaultMaintenance()
	{
		// Placeholder for vault maintenance dialog
		// Implementation depends on GUI framework
	}

	/// <summary>
	/// Loads a vault from a file path (internal helper).
	/// </summary>
	private void LoadVault(PlayerCollection pc, string fileName)
	{
		try
		{
			this.PlayerCollectionProvider.LoadFile(pc, fileName);
		}
		catch (ArgumentException ae)
		{
			pc.ArgumentException = ae;
		}
	}
}
