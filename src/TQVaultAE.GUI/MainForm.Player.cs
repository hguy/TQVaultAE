using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Logs;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI;

public partial class MainForm
{
	private IReadOnlyCollection<string> _watchedPlayerFiles = null;
	private IEnumerable<string> WatchedPlayerFiles
	{
		get
		{
			if (_watchedPlayerFiles is null)
				_watchedPlayerFiles = new List<string> { this.GamePathResolver.PlayerSaveFileName, this.GamePathResolver.PlayerStashFileNameB }.AsReadOnly();

			return _watchedPlayerFiles;
		}
	}

	/// <summary>
	/// Handler for changing the Character drop down selection.
	/// </summary>
	/// <param name="sender">Sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CharacterComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		// Hmm. We can load a character now!
		var selected = this.comboBoxCharacter.SelectedItem;
		var selectedSave = selected as PlayerSave;
		var selectedText = selected.ToString();

		// See if they actually changed their selection and ignore "No TQ characters detected"
		if (selectedText.Equals(Resources.MainFormSelectCharacter) || selectedText.Equals(Resources.MainFormNoCharacters)
			|| selectedText.Equals(Resources.MainFormNoCustomChars))
		{
			// no char selected
			this.ClearPlayer();
		}
		else
		{
			this.LoadPlayerAndStashes(selectedSave);
		}
		this.Refresh();
	}

	/// <summary>
	/// Gets a list of available player files and populates the drop down list.
	/// </summary>
	private void GetPlayerList()
	{
		// Initialize the character combo-box
		this.comboBoxCharacter.Items.Clear();

		var characters = this.playerService.GetPlayerSaveList();

		// Init FileWatcher
		if (base.USettings.EnableHotReload)
		{
			foreach (var ps in characters)
			{
				ps.PlayerSaveWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerSaveFileName);
				ps.PlayerStashWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerStashFileNameB);
			}
		}

		if (!characters.Any())
			this.comboBoxCharacter.Items.Add(Resources.MainFormNoCharacters);
		else
		{
			this.comboBoxCharacter.Items.Add(Resources.MainFormSelectCharacter);

			foreach (var chara in characters)
				this.comboBoxCharacter.Items.Add(chara);
		}

		this.comboBoxCharacter.SelectedIndex = 0;
	}

	// Called on FileSystemWatcher thread
	private void PlayerSaveFile_Changed(object sender, FileSystemEventArgs e)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed
			|| !this.WatchedPlayerFiles.Any(f => f.Equals(e.Name, StringComparison.OrdinalIgnoreCase))
		) return;

		var fw = sender as FileSystemWatcher;
		fw.EnableRaisingEvents = false;

		// retrieve PlayerSave
		var playerSave = this.comboBoxCharacter.Items.OfType<PlayerSave>().FirstOrDefault(ps => ps.Folder == fw.Path);

		this.ReloadFromWatcher(fw, e.FullPath, () =>
		{
			// Reload player file
			PlayerLoadResult playerResult = null;
			if (e.Name.Equals(this.GamePathResolver.PlayerSaveFileName, StringComparison.OrdinalIgnoreCase))
				playerResult = this.LoadPlayer(playerSave, true);

			StashLoadResult stashResult = null;
			if (e.Name.Equals(this.GamePathResolver.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
				stashResult = this.LoadPlayerStash(playerSave, true);

			// Refresh
			this.Invoke((MethodInvoker)delegate
			{
				// if is current displayed character
				if (this.comboBoxCharacter.SelectedItem == playerSave)
				{
					if (playerResult is not null)
					{
						this.playerPanel.Player = playerResult.Player;
						this.stashPanel.Player = playerResult.Player;
					}

					if (stashResult is not null)
						this.stashPanel.Stash = stashResult.Stash;
				}

				fw.EnableRaisingEvents = true;
			});
		});
	}

	/// <summary>Attempts made to reload a file changed by the game before giving up.</summary>
	private const int HotReloadMaxAttempts = 10;

	/// <summary>Pause between two reload attempts.</summary>
	private static readonly TimeSpan HotReloadRetryDelay = TimeSpan.FromMilliseconds(500);

	/// <summary>How long to wait for the game to release a file it is writing.</summary>
	private static readonly TimeSpan HotReloadFileReleaseTimeout = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Runs <paramref name="reload"/> on the <see cref="FileSystemWatcher"/> thread once the game has finished writing
	/// <paramref name="filePath"/>, retrying a bounded number of times while the file stays locked.
	/// <paramref name="reload"/> re-enables <paramref name="watcher"/> when it succeeds; on give-up it is re-enabled here.
	/// </summary>
	private void ReloadFromWatcher(FileSystemWatcher watcher, string filePath, Action reload)
	{
		for (int attempt = 1; ; attempt++)
		{
			try
			{
				WaitForFileRelease(filePath, HotReloadFileReleaseTimeout);
				reload();
				return;
			}
			catch (IOException ex) when (attempt < HotReloadMaxAttempts)
			{
				this.Log.LogWarning(ex, "Hot reload of {File} failed (attempt {Attempt}/{Max}), retrying", filePath, attempt, HotReloadMaxAttempts);
				Thread.Sleep(HotReloadRetryDelay);
			}
			catch (IOException ex)
			{
				this.Log.LogError(ex, "Hot reload of {File} abandoned after {Attempts} attempts", filePath, attempt);
				if (!this.IsDisposed)
					this.BeginInvoke(() => watcher.EnableRaisingEvents = true);
				return;
			}
		}
	}

	/// <summary>
	/// Waits until no other process holds <paramref name="filePath"/> open, which is when the game has finished writing it.
	/// </summary>
	/// <exception cref="IOException">The file is still in use after <paramref name="timeout"/>.</exception>
	private static void WaitForFileRelease(string filePath, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (true)
		{
			try
			{
				// Exclusive open only succeeds once the writer has closed its handle.
				using var probe = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
				return;
			}
			catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
			{
				// Nothing to wait for; the loader reports the missing file itself.
				return;
			}
			catch (IOException) when (DateTime.UtcNow < deadline)
			{
				Thread.Sleep(100);
			}
		}
	}


	private FileSystemWatcher CreateFileWatcher(string folder, string filename)
	{
		// the game write save file when inventory is opened and again when it's closed. We can't distinguish between both events.
		var fw = new FileSystemWatcher();
		fw.BeginInit();
		fw.Path = folder;
		fw.Filter = filename;
		fw.IncludeSubdirectories = false;
		fw.NotifyFilter = NotifyFilters.Attributes;// Attributes triggers after file update
		fw.SynchronizingObject = this;
		fw.Changed += new FileSystemEventHandler(this.PlayerSaveFile_Changed);
		fw.EnableRaisingEvents = true;
		fw.EndInit();
		return fw;
	}

	/// <summary>
	/// Creates the player panel
	/// </summary>
	private void CreatePlayerPanel()
	{
		this.playerPanel = new PlayerPanel(this.DragInfo, 4, new Size(12, 5), new Size(8, 5), this.ServiceProvider);

		this.playerPanel.DrawAsGroupBox = false;

		this.playerPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.playerPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.playerPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.playerPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.playerPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.playerPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.flowLayoutPanelRightPanels.Controls.Add(this.playerPanel);
		this.playerPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.playerPanel.Margin = new Padding(0);
	}



	/// <summary>
	/// Clears out the selected player
	/// Changed by VillageIdiot to a separate function.
	/// </summary>
	private void ClearPlayer()
	{
		if (this.playerPanel.Player != null)
		{
			this.playerPanel.Player = null;
			this.stashPanel.Player = null;
			this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;

			if (this.stashPanel.Stash != null)
				this.stashPanel.Stash = null;
		}
	}

	/// <summary>
	/// Loads a player using the drop down list.
	/// Assumes designators are appended to character name.
	/// Changed by VillageIdiot to a separate function.
	/// </summary>
	/// <param name="selectedSave">Player string from the drop down list.</param>
	/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns></returns>
	private (PlayerLoadResult PlayerResult, StashLoadResult StashResult) LoadPlayerAndStashes(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		var result = LoadPlayer(selectedSave, fromFileWatcher);

		var resultStash = this.LoadPlayerStash(selectedSave, fromFileWatcher);

		if (selectedSave.IsImmortalThrone)
		{
			if (this.stashPanel.RelicVaultStash == null)
				LoadRelicVaultStash();

			if (this.stashPanel.TransferStash == null)
				LoadTransferStash();
		}
		else // Titan Quest original
		{
			this.stashPanel.RelicVaultStash = null;
			this.stashPanel.TransferStash = null;
		}

		return (result, resultStash);
	}

	private PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher)
	{
		// Use management service
		var resultDTO = this.playerService.LoadPlayer(selectedSave, fromFileWatcher);

		// Get the player
		try
		{
			if (resultDTO.Player?.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, resultDTO.PlayerFile, resultDTO.Player.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (!fromFileWatcher)
			{
				this.playerPanel.Player = resultDTO.Player;
				this.stashPanel.Player = resultDTO.Player;
				this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;
			}
			this.comboBoxCharacter.RefreshItem(selectedSave);
		}
		catch (IOException exception)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, resultDTO.PlayerFile, exception.ToString());
			MessageBox.Show(msg, Resources.MainFormPlayerReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			Log.LogError(exception, msg);
			this.playerPanel.Player = null;
			this.comboBoxCharacter.SelectedIndex = 0;
		}

		return resultDTO;
	}

	private StashLoadResult LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		// Get the player's stash - use management service

		// Only if it's IT, TQ doesn't have one
		if (!selectedSave.IsImmortalThrone)
		{
			this.stashPanel.Stash = null;
			return null;
		}

		var resultStash = this.stashService.LoadPlayerStash(selectedSave, fromFileWatcher);
		try
		{
			// Throw a message if the stash is not present.
			if (resultStash.Stash.StashFound.HasValue && !resultStash.Stash.StashFound.Value)
			{
				var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nCharacter : ", selectedSave);
				MessageBox.Show(msg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (resultStash.Stash.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, resultStash.StashFile, resultStash.Stash.ArgumentException.Message);
				MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			if (!fromFileWatcher) this.stashPanel.Stash = resultStash.Stash;
		}
		catch (IOException exception)
		{
			string msg = string.Concat(Resources.MainFormReadError, resultStash.StashFile, exception.ToString());
			MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			Log.LogError(exception, msg);
			this.stashPanel.Stash = null;
		}

		return resultStash;
	}

	/// <summary>
	/// Attempts to save all modified player files
	/// </summary>
	/// <returns>True if there were any modified player files.</returns>
	private bool SaveAllModifiedPlayers()
	{
	retry:
		bool saved = false;
		PlayerCollection playerIfError = null;
		try
		{
			saved = this.playerService.SaveAllModifiedPlayers(ref playerIfError);
		}
		catch (IOException exception)
		{
			string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, playerIfError.PlayerName);
			Log.LogError(exception, title);
			switch (MessageBox.Show(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
			{
				case DialogResult.Abort:
					// rethrow the exception
					throw;
				case DialogResult.Retry:
					goto retry;
					//default: break; // DialogResult.Ignore
			}
		}
		return saved;
	}

	/// <summary>
	/// Attempt to duplicate a character
	/// </summary>
	private void DuplicateCharacter()
	{
		var ps = this.comboBoxCharacter.SelectedItem as PlayerSave;
		if (ps is not null)
		{

		askAgain:
			// Ask for new name
			var newname = InputDialog.Show(Resources.DuplicateCharacter_NewNameRequired, Resources.DuplicateCharacter_ModalTitle, "NewName").Trim();
			if (newname == string.Empty) return;// Cancel button

			// validate new name
			char[] invalidChars = this.PathIO.GetInvalidPathChars().Concat(this.PathIO.GetInvalidFileNameChars()).ToArray();
			if (string.IsNullOrWhiteSpace(newname) || newname.IndexOfAny(invalidChars) > -1)
			{
				MessageBox.Show(Resources.DuplicateCharacter_NewNameMustBeValid, Resources.DuplicateCharacter_ModalTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				goto askAgain;
			}

			var alreadyUsed = this.comboBoxCharacter.Items.OfType<PlayerSave>().Any(ps => newname.Equals(ps.Name, StringComparison.OrdinalIgnoreCase));
			if (alreadyUsed)
			{
				MessageBox.Show(Resources.DuplicateCharacter_NewNameAlreadyUsed, Resources.DuplicateCharacter_ModalTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				goto askAgain;
			}

			string newFolder = null;
			try
			{
				// Duplicate File
				newFolder = this.GameFileService.DuplicateCharacterFiles(ps.Folder, newname);
				this.playerService.AlterNameInPlayerFileSave(newname, newFolder);
			}
			catch (Exception ex)
			{
				// Display Error
				Log.LogError(ex, Resources.DuplicateCharacter_Error);
				MessageBox.Show(Log.FormatException(ex), Resources.DuplicateCharacter_Error, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				// Rollback IO
				try
				{
					if (newFolder is not null) DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception exx)
				{
					// log & mute
					Log.LogError(exx, Resources.DuplicateCharacter_Error);
				}
				return;
			}

			System.Windows.Forms.Application.Restart();
			return;
		}

		// Player must be selected
		MessageBox.Show(Resources.DuplicateCharacter_SourceRequired, Resources.DuplicateCharacter_ModalTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, RightToLeftOptions);
	}
}
