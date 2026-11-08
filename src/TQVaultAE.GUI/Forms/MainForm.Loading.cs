using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Concurrent;
using TQVaultAE.Application;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Logs;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Files


	/// <summary>
	/// Counts the number of files which LoadAllFiles will load.  Used to set the max value of the progress bar.
	/// </summary>
	/// <returns>Total number of files that LoadAllFiles() will load.</returns>
	private int LoadAllFilesTotal()
		=> LoadAllFilesCount(GamePathResolver.GetCharacterList().Length, GamePathResolver.GetVaultList().Length);

	/// <summary>
	/// Number of progress steps of <see cref="LoadAllFiles"/>: one per character, one per character stash, one per vault.
	/// </summary>
	private static int LoadAllFilesCount(int characters, int vaults) => (characters * 2) + vaults;

	/// <summary>
	/// Loads all of the players, stashes, and vaults.
	/// Shows a progress dialog.
	/// Used for the searching function.
	/// </summary>
	/// <param name="progress">Receives one step per loaded file.</param>
	private void LoadAllFiles(IProgress<int> progress)
	{
		// Check to see if we failed the last time we tried loading all of the files.
		// If we did fail then turn it off and skip it.
		if (!USettings.LoadAllFilesCompleted)
		{
			if (this.Services.MessageBoxes.Confirm(Resources.MainFormDisableLoadAllFiles, Resources.MainFormDisableLoadAllFilesCaption, MessageBoxIcon.Information))
			{
				USettings.LoadAllFilesCompleted = true;
				USettings.LoadAllFiles = false;
				USettings.Save();
				return;
			}
		}

		string[] vaults = GamePathResolver.GetVaultList();
		var charactersIT = this.comboBoxCharacter.Items.OfType<PlayerSave>().ToArray();

		// Since this takes a while, show a progress dialog box.
		int total = LoadAllFilesCount(charactersIT.Length, vaults.Length);

		if (total > 0)
		{
			// We were successful last time so we reset the flag for this attempt.
			USettings.LoadAllFilesCompleted = false;
			USettings.Save();
		}
		else
			return;

		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();

		// Load all of the Immortal Throne player files and stashes.
		var bagPlayer = new ConcurrentBag<PlayerLoadResult>();
		var bagPlayerStashes = new ConcurrentBag<StashLoadResult>();
		var bagVault = new ConcurrentBag<VaultLoadResult>();

		var lambdacharactersIT = charactersIT.Select(c => (Action)(() =>
		{
			// Get the player
			var result = this.playerService.LoadPlayer(c);
			bagPlayer.Add(result);
			progress.Report(1);
		})).ToArray();

		var lambdacharacterStashes = charactersIT.Select(c => (Action)(() =>
		{
			// Get the player's stash
			var result = this.stashService.LoadPlayerStash(c);
			bagPlayerStashes.Add(result);
			progress.Report(1);
		})).ToArray();

		var lambdaVault = vaults.Select(c => (Action)(() =>
		{
			// Load all of the vaults.
			var result = this.vaultService.LoadVault(c);
			bagVault.Add(result);
			progress.Report(1);
		})).ToArray();

		Parallel.Invoke(lambdacharactersIT.Concat(lambdacharacterStashes).Concat(lambdaVault).ToArray());// Parallel loading

		// Dispay errors
		var readErrors = bagPlayer.Select(p => (File: (string?)p.PlayerFile, Error: p.Player?.ArgumentException))
			.Concat(bagPlayerStashes.Select(p => (File: p.StashFile, Error: p.Stash?.ArgumentException)))
			.Concat(bagVault.Select(p => (File: (string?)p.Filename, Error: p.ArgumentException)))
			.ToList();
		foreach (var (file, error) in readErrors)
		{
			if (error is null)
				continue;

			string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, file, error.Message);
			this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
		}

		stopWatch.Stop();
		// Get the elapsed time as a TimeSpan value.
		TimeSpan ts = stopWatch.Elapsed;

		// Format and display the TimeSpan value.
		Log.LogInformation("LoadTime {Hours:00}:{Minutes:00}:{Seconds:00}.{Centiseconds:00}",
			ts.Hours, ts.Minutes, ts.Seconds,
			ts.Milliseconds / 10);

		// We made it so set the flag to indicate we were successful.
		USettings.LoadAllFilesCompleted = true;
		USettings.Save();
	}

	/// <summary>
	/// Attempts to save all modified files.
	/// </summary>
	/// <returns>true if players have been modified</returns>
	private bool SaveAllModifiedFiles()
	{
#if DEBUG
		if (Automation.AutomationHost.IsSaveBlocked("save players, vaults and stashes"))
			return false;
#endif
		bool playersModified = this.SaveAllModifiedPlayers();
		bool vaultsModified = this.SaveAllModifiedVaults();
		bool stashesModified = this.SaveAllModifiedStashes();

		// Notification Last Save
		if (playersModified || vaultsModified || stashesModified)
		{
			var saved = string.Format(Resources.SavedAtNotification, DateTime.Now);
			this.NotificationText.Text = saved;
			this.toolTip.SetToolTip(this.saveButton, saved);
		}

		return playersModified;
	}

	/// <summary>
	/// Runs a save of players, vaults or stashes. On a file error, logs it and asks: Abort rethrows, Retry saves again,
	/// Ignore gives up this save.
	/// </summary>
	/// <param name="save">The save; returns whether something was saved.</param>
	/// <param name="nameInError">The name of the file being saved when the error happened.</param>
	/// <returns>Whether something was saved.</returns>
	private bool SaveWithRetry(Func<bool> save, Func<string?> nameInError)
	{
		while (true)
		{
			try
			{
				return save();
			}
			catch (IOException exception)
			{
				string title = string.Format(CultureInfo.CurrentCulture, Resources.MainFormSaveError, nameInError());
				Log.LogError(exception, "{Message}", title);
				switch (this.Services.MessageBoxes.Ask(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Abort:
						throw;
					case DialogResult.Retry:
						continue;
					default:
						return false;
				}
			}
		}
	}

	#endregion

	#region SplashScreen & Tooltip

	/// <summary>
	/// Handler for closing the splash screen
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">FormClosedEventArgs data</param>
	private void SplashScreenClosed(object? sender, FormClosedEventArgs e)
	{
		if (this.resourcesLoaded)
		{
			if (!this.loadingComplete)
				this.loadCancellation.Cancel();

			this.ShowMainForm();
		}
		else
			System.Windows.Forms.Application.Exit();
	}

	/// <summary>
	/// Starts the fade in of the main form.
	/// </summary>
	private void ShowMainForm()
	{
		this.fadeInTimer.Start();
		this.ShowInTaskbar = true;
		this.Enabled = true;
		this.Show();
		this.Activate();
	}


	/// <summary>
	/// Tooltip callback
	/// </summary>
	/// <param name="windowHandle">handle of the main window form</param>
	/// <returns>tooltip string</returns>
	private void ToolTipCallback(int windowHandle)
	{
		this.vaultPanel.ToolTipCallback(windowHandle);
		this.playerPanel.ToolTipCallback(windowHandle);
		this.stashPanel.ToolTipCallback(windowHandle);
		this.secondaryVaultPanel.ToolTipCallback(windowHandle);

		// Changed by VillageIdiot
		// If we are dragging something around, clear the tooltip and text box.
		if (this.DragInfo.IsActive)
			this.NotificationText.Text = string.Empty;
	}

	#endregion

	#region Game Resources

	/// <summary>
	/// Cancelled when the splash screen is closed before the loading completes.
	/// </summary>
	private readonly CancellationTokenSource loadCancellation = new();

	/// <summary>
	/// Loads the resources off the UI thread, advancing the splash screen, then shows them.
	/// </summary>
	private async Task LoadResourcesAsync()
	{
		// Created on the UI thread, so the splash screen is updated there.
		var progress = new Progress<int>(_ => this.splashScreen.IncrementValue());
		var token = this.loadCancellation.Token;
		try
		{
			await Task.Run(() => this.LoadResources(progress, token));
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
			if (!this.resourcesLoaded)
				System.Windows.Forms.Application.Exit();
			return;
		}
		catch (Exception ex)
		{
			Log.LogError(ex, "resourcesLoaded = {ResourcesLoaded}", this.resourcesLoaded);

			if (this.Services.MessageBoxes.Confirm(string.Concat(ex.Message, Resources.Form1BadLanguage), Resources.Form1ErrorLoadingResources, MessageBoxIcon.Exclamation)
			) System.Windows.Forms.Application.Restart();
			else
				System.Windows.Forms.Application.Exit();
			return;
		}

		this.ResourcesLoaded();
	}

	/// <summary>
	/// Loads the resources. Runs off the UI thread.
	/// </summary>
	/// <param name="progress">Receives one step per loaded resource.</param>
	/// <param name="token">Cancelled when the splash screen is closed.</param>
	private void LoadResources(IProgress<int> progress, CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var args = this.commandLineArgs;
		args.ThrowIfInvalid();

		// Check to see if we loaded something from the command line.
		if (args.HasMapName)
			GamePathResolver.MapName = args.MapName ?? string.Empty;

		this.resourcesLoaded = true;
		progress.Report(1);

		if (USettings.LoadAllFiles)
			this.LoadAllFiles(progress);
	}

	/// <summary>
	/// Shows the loaded resources: stashes, last character and vault.
	/// </summary>
	private void ResourcesLoaded()
	{
		this.loadingComplete = true;
		this.Enabled = true;

		// The characters may have been loaded by LoadAllFiles.
		this.comboBoxCharacter.RefreshItems();

		// Prepare the search data now, so the first search does not have to,
		// then the bag button icons, so the bag button settings open at once.
		var iconService = this.Services.IconService;
		_ = this.Services.ItemDatabaseService.WarmUpAsync()
			.ContinueWith(_ => iconService.GetIconDatabaseAsync(), TaskScheduler.Default);

		this.LoadTransferStash();
		this.LoadRelicVaultStash();

		if (USettings.EnableHotReload)
		{
			var relicPath = GamePathResolver.RelicVaultStashFileFullPath;
			var transferPath = GamePathResolver.TransferStashFileFullPath;

			this.fileSystemWatcherRelicStash.EnableRaisingEvents = false;
			if (FileIO.Exists(relicPath))
			{
				this.fileSystemWatcherRelicStash.Path = PathIO.GetDirectoryName(relicPath);
				this.fileSystemWatcherRelicStash.Filter = PathIO.GetFileName(relicPath);
				this.fileSystemWatcherRelicStash.EnableRaisingEvents = true;
			}

			this.fileSystemWatcherTransferStash.EnableRaisingEvents = false;
			if (FileIO.Exists(transferPath))
			{
				this.fileSystemWatcherTransferStash.Path = PathIO.GetDirectoryName(transferPath);
				this.fileSystemWatcherTransferStash.Filter = PathIO.GetFileName(transferPath);
				this.fileSystemWatcherTransferStash.EnableRaisingEvents = true;
			}
		}

		// Load last character here if selected
		if (USettings.LoadLastCharacter)
		{
			var lastPlayerSave = this.comboBoxCharacter.Items.OfType<PlayerSave>()
				.FirstOrDefault(ps => ps.Name == USettings.LastCharacterName);

			if (lastPlayerSave != null)
				this.comboBoxCharacter.SelectedItem = lastPlayerSave;
		}

		string currentVault = VaultService.MAINVAULT;

		// See if we should load the last loaded vault
		if (USettings.LoadLastVault)
		{
			currentVault = USettings.LastVaultName;

			// Make sure there is something in the config file to load else load the Main Vault
			// We do not want to create new here.
			if (string.IsNullOrEmpty(currentVault) || !FileIO.Exists(GamePathResolver.GetVaultFile(currentVault)))
				currentVault = VaultService.MAINVAULT;
		}

		this.vaultListComboBox.SelectedItem = currentVault;

		// Finally load Vault
		this.LoadVault(currentVault, false);

		this.splashScreen.UpdateText();
		this.splashScreen.ShowMainForm = true;

		var args = this.commandLineArgs;

		// Allows skipping of title screen with setting
		if (args.IsAutomatic || USettings.SkipTitle == true)
		{
			string? player = args.Player;
			int index = this.comboBoxCharacter.FindString(player ?? string.Empty);
			if (index != -1)
				this.comboBoxCharacter.SelectedIndex = index;

			this.splashScreen.CloseForm();
		}
	}

	#endregion

	#region Settings Dialog

	/// <summary>
	/// Updates configuration settings
	/// </summary>
	private void SaveConfiguration()
	{
#if DEBUG
		if (Automation.AutomationHost.IsSaveBlocked("save configuration"))
			return;
#endif
		string? vaultName = this.vaultListComboBox.SelectedItem != null ? this.vaultListComboBox.SelectedItemText() : null;

		string? characterName = this.comboBoxCharacter.SelectedItem is PlayerSave ps
			? ps.Name
			: this.comboBoxCharacter.SelectedItem?.ToString();

		// No character is selected
		if (characterName == Resources.MainFormSelectCharacter)
			characterName = string.Empty;

		this.configChanged = this.settingsPersistenceService.SaveConfiguration(vaultName, characterName, UIService.Zoom, this.configChanged);
	}

	/// <summary>
	/// Handler for clicking the configure button.
	/// Shows the Settings Dialog.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ConfigureButtonClick(object? sender, EventArgs e)
	{
		SettingsDialog settingsDialog = new SettingsDialog(this);
		bool restart = false;
		using (ZoomScope.Enter())
			settingsDialog.Scale(new SizeF(UIService.Zoom, UIService.Zoom));

		if (settingsDialog.ShowDialog() == DialogResult.OK && settingsDialog.ConfigurationChanged)
		{
			if (settingsDialog.PlayerFilterChanged)
			{
				this.comboBoxCharacter.SelectedItem = Resources.MainFormSelectCharacter;
				if (this.playerPanel.Player != null)
					this.playerPanel.Player = null;

				this.GetPlayerList();
			}

			if (settingsDialog.VaultPathChanged)
			{
				GamePathResolver.TQVaultSaveFolder = settingsDialog.VaultPath;
				this.vaultService.UpdateVaultPath(settingsDialog.VaultPath);
				this.GetVaultList(true);
			}

			if (settingsDialog.EnableTQVaultSoundsChanged)
			{
				var soundSrv = this.Services.SoundService;
				soundSrv.InitAllPlayers();
			}

			var restartReason = RestartRule.Evaluate(
				settingsDialog.LanguageChanged
				, settingsDialog.GamePathChanged
				, settingsDialog.CustomMapsChanged
				, settingsDialog.UISettingChanged);

			if (restartReason != RestartReason.None)
			{
				var (title, message) = restartReason switch
				{
					RestartReason.Settings => (Resources.MainFormSettingsChanged, Resources.MainFormSettingsChangedMsg),
					RestartReason.GamePath => (Resources.MainFormPathsChanged, Resources.MainFormPathsChangedMsg),
					RestartReason.CustomMaps => (Resources.MainFormMapsChanged, Resources.MainFormMapsChangedMsg),
					_ => (Resources.MainFormLanguageChanged, Resources.MainFormLanguageChangedMsg),
				};

				restart = this.Services.MessageBoxes.Confirm(message, title);
			}

			if (settingsDialog.ItemBGColorOpacityChanged || settingsDialog.EnableItemRequirementRestrictionChanged)
				this.Refresh();

			if (settingsDialog.EnableCharacterEditChanged)
				stashPanel?.UpdatePlayerInfo();

			this.configChanged = true;
			this.SaveConfiguration();


			AdjustMenuButtonVisibility();

			if (restart)
			{
				if (this.DoCloseStuff())
					System.Windows.Forms.Application.Restart();
			}
		}
	}

	#endregion
}
