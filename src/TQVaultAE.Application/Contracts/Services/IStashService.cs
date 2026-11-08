using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IStashService
{
	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadPlayerStash(PlayerSave? selectedSave, bool fromFileWatcher = false);

	/// <summary>
	/// Loads the relic vault stash.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false);

	/// <summary>
	/// Loads the transfer stash for immortal throne.
	/// </summary>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadTransferStash(bool fromFileWatcher = false);

	/// <summary>
	/// Attempts to save all modified stash files.
	/// </summary>
	/// <param name="stashOnError">If save fails, this will contain the stash that failed.</param>
	/// <returns>Number of stash saved.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	int SaveAllModifiedStashes(ref Stash? stashOnError);

	/// <summary>
	/// Creates a file watcher for the relic vault stash file.
	/// </summary>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	FileSystemWatcher? CreateRelicStashFileWatcher(Action onChanged);

	/// <summary>
	/// Creates a file watcher for the transfer stash file.
	/// </summary>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	FileSystemWatcher? CreateTransferStashFileWatcher(Action onChanged);

	/// <summary>
	/// Brings a stash panel bag id within the bags it has: below the equipment panel gives the equipment panel,
	/// beyond the relic vault stash gives the relic vault stash.
	/// </summary>
	/// <param name="bagId">The requested bag id (see <see cref="BagIdConstants"/>).</param>
	/// <returns>A bag id from <see cref="BagIdConstants.BAGID_EQUIPMENTPANEL"/> to <see cref="BagIdConstants.BAGID_RELICVAULTSTASH"/>.</returns>
	int ClampBagId(int bagId);

	/// <summary>
	/// Gets the sack a stash panel bag shows.
	/// </summary>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>).</param>
	/// <param name="player">The character, for the equipment panel.</param>
	/// <param name="transferStash">The transfer stash (caravan).</param>
	/// <param name="playerStash">The character's stash.</param>
	/// <param name="relicVaultStash">The relic vault stash.</param>
	/// <returns>The sack; null when the bag has nothing to show (no character, or a stash without sacks).</returns>
	SackCollection? GetBagSack(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash);
}
