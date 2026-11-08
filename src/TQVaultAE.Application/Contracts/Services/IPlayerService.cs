using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

public interface IPlayerService
{
	/// <summary>
	/// Loads a player and their stash.
	/// </summary>
	/// <param name="selectedSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Player load result with player, stash, and error information.</returns>
	PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false);

	/// <summary>
	/// Loads a player stash.
	/// </summary>
	/// <param name="playerSave">Player save information.</param>
	/// <param name="fromFileWatcher">When <c>true</c> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns>Stash load result.</returns>
	StashLoadResult LoadPlayerStash(PlayerSave playerSave, bool fromFileWatcher = false);

	/// <summary>
	/// Attempts to save all modified player files.
	/// </summary>
	/// <param name="playerOnError">If save fails, this will contain the player that failed.</param>
	/// <returns>True if there were any modified player files.</returns>
	/// <exception cref="IOException">can happen during file save</exception>
	bool SaveAllModifiedPlayers(ref PlayerCollection? playerOnError);

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>List of character files descriptor</returns>
	PlayerSave[] GetPlayerSaveList();

	/// <summary>
	/// Gets a list of all of the character files in the save folder.
	/// </summary>
	/// <returns>Read-only list of character files descriptor</returns>
	IReadOnlyList<PlayerSave> GetPlayerSaveListReadOnly();

	/// <summary>
	/// Alters the player name in the save file.
	/// </summary>
	/// <param name="newname">New player name.</param>
	/// <param name="saveFolder">Save folder path.</param>
	void AlterNameInPlayerFileSave(string newname, string saveFolder);

	/// <summary>
	/// Checks the name of a character about to be duplicated from <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The character to duplicate.</param>
	/// <param name="newName">The new name, already trimmed.</param>
	/// <param name="existingNames">The names of the known characters.</param>
	/// <returns><see cref="CharacterNameError.None"/> when the name can be used, else the first problem found.</returns>
	CharacterNameError ValidateNewCharacterName(PlayerSave source, string newName, IEnumerable<string> existingNames);

	/// <summary>
	/// Copies the save files of <paramref name="source"/> to a new save folder and writes <paramref name="newName"/>
	/// into the new player file. On failure, the new folder is removed.
	/// </summary>
	/// <param name="source">The character to duplicate.</param>
	/// <param name="newName">The new name, validated by <see cref="ValidateNewCharacterName"/>.</param>
	/// <returns>The new save folder, or the failure.</returns>
	CharacterDuplicateResult DuplicateCharacter(PlayerSave source, string newName);

	/// <summary>
	/// Creates a file watcher for the player file.
	/// </summary>
	/// <param name="playerSave">Player save information.</param>
	/// <param name="onChanged">Action to call when the file changes.</param>
	/// <returns>A FileSystemWatcher instance or null if the file path is invalid.</returns>
	FileSystemWatcher? CreatePlayerFileWatcher(PlayerSave playerSave, Action<PlayerSave, bool> onChanged);

	/// <summary>
	/// Updates the player file path in the player collection cache after archiving/unarchiving.
	/// </summary>
	/// <param name="oldPath">The old file path of the player.</param>
	/// <param name="newPath">The new file path of the player.</param>
	void UpdatePlayerFilePath(string oldPath, string newPath);

	/// <summary>
	/// Archives a character (moves its save folder) and keys the loaded player by its new file path.
	/// </summary>
	/// <returns><see langword="true"/> when the save was archived.</returns>
	bool Archive(PlayerSave ps);

	/// <summary>
	/// Unarchives a character (moves its save folder back) and keys the loaded player by its new file path.
	/// </summary>
	/// <returns><see langword="true"/> when the save was unarchived.</returns>
	bool Unarchive(PlayerSave ps);
}
