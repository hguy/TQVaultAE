using Microsoft.Extensions.Logging;
using TQVaultAE.Presentation;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

/// <summary>
/// IGameFileService implementation
/// </summary>
public partial class GameFileService : IGameFileService
{
	protected readonly ILogger Log;
	protected readonly IGamePathService GamePathService;
	protected readonly IUserNotifier UserNotifier;
	protected readonly IFileIO FileIO;
	protected readonly IPathIO PathIO;
	protected readonly IDirectoryIO DirectoryIO;

	protected readonly UserSettings UserSettings;

	public GameFileService(ILogger<GameFileService> log, IGamePathService iGamePathService, IUserNotifier userNotifier, IFileIO fileIO, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings userSettings)
	{
		this.Log = log;
		this.GamePathService = iGamePathService;
		this.UserNotifier = userNotifier;
		this.FileIO = fileIO;
		this.PathIO = pathIO;
		this.DirectoryIO = directoryIO;
		this.UserSettings = userSettings;
	}

	public bool GitAmIBehind()
	{
		var repoUrl = this.UserSettings.GitBackupRepository;
		string errMess = string.Format(Resources.GitUnableToFetch, repoUrl);

		var fetch = this.RunGitInRepository("fetch", "origin");
		if (HandleExecuteOut(fetch, errMess, out var fetchoutStd, out var fetcherrStd))
		{
			var status = this.RunGitInRepository("status", "--ahead-behind");
			if (HandleExecuteOut(status, errMess, out var statusoutStd, out var statuserrStd))
			{
				var output = statusoutStd.JoinString(" ");
				return output.Contains("behind");
			}
		}
		return false;
	}

	public bool GitClone()
	{
		var repoUrl = this.UserSettings.GitBackupRepository;
		string errMess = string.Format(Resources.GitUnableToClone, repoUrl);
		DirectoryIO.CreateDirectory(GamePathService.LocalGitRepositoryDirectory);

		var clone = ShellCommand.Run("git"
			, new[] { "clone", repoUrl, GamePathService.LocalGitRepositoryDirectory }
			, GamePathService.LocalGitRepositoryDirectory
		);

		return HandleExecuteOut(clone, errMess, out var outStd, out var errStd);
	}

	public bool GitAddCommitTagAndPush()
	{
		if (!this.UserSettings.GitBackupEnabled || !GamePathService.LocalGitRepositoryGitDirExist)
			return false;

		var repoUrl = this.UserSettings.GitBackupRepository;
		string errMess = string.Format(Resources.GitUnableToPush, repoUrl);

		ShellCommand? status, stageAll, commit, tagVersion, tagLatest, push, pushTags;
		status = stageAll = commit = tagVersion = tagLatest = push = pushTags = null;

		try
		{
			/*
				 * Reporting synchronously via event starts good but break at some point, job is done though.
				 * 
				 * IProgress : Should be the new way of reporting progress. 
				 * It report progress on time and is not blocking, but the Form is delaying result processing and flush them all AFTER the job. 
				 * 
				 * BackgroundWorker.ReportProgress() : should work for Winform 
				 * but Command.Wait() inside BackgroundWorker.DoWork() simply brutaly kill the app, like a BSOD for app. LOL
				 * 
				 * I really tried.
				 * hguy
				 */

			// Is there anything new to commit ?
			status = this.RunGitInRepository("status", "-suall");
			var statusSuccess = HandleExecuteOut(status, errMess, out var statusoutStd, out var statuserrStd);
			if (statusSuccess && statusoutStd.Count > 0)
			{
				stageAll = this.RunGitInRepository("stage", "*");
				if (HandleExecuteOut(stageAll, errMess, out var stageAlloutStd, out var stageAllerrStd))
				{
					commit = this.RunGitInRepository("commit", "-m", @"TQVaultAE update!");
					if (HandleExecuteOut(commit, errMess, out var commitoutStd, out var commiterrStd))
					{
						var tag = DateTime.Now.ToString("yy.MM.dd.HHmmss");// Use date for versioning

						tagVersion = this.RunGitInRepository("tag", tag);
						if (HandleExecuteOut(tagVersion, errMess, out var tagVersionoutStd, out var tagVersionerrStd))
						{
							tagLatest = this.RunGitInRepository("tag", "-f", "latest");
							if (HandleExecuteOut(tagLatest, errMess, out var tagLatestoutStd, out var tagLatesterrStd))
							{
								using (push = this.RunGitInRepository("push", "-v", "--progress")) // You need "--progress" to capture percentage in StandardError
								{
									if (HandleExecuteOut(push, errMess, out var pushoutStd, out var pusherrStd))
									{
										pushTags = this.RunGitInRepository("push", "origin", "latest", "-f");
										if (HandleExecuteOut(pushTags, errMess, out var pushTagsoutStd, out var pushTagserrStd))
										{
											pushTags = this.RunGitInRepository("push", "origin", tag);
											return HandleExecuteOut(pushTags, errMess, out pushTagsoutStd, out pushTagserrStd);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			errMess += Environment.NewLine + ex.Message;
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, Buttons: ShowMessageButtons.OK);
		}
		return false;
	}

	public void GitRepositorySetup()
	{
		if (!this.UserSettings.GitBackupEnabled) return;

		bool overrideFiles = false;

		var RepoTQPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQ, GamePathService.SaveDataDirName);
		var RepoTQITPath = this.PathIO.Combine(GamePathService.LocalGitRepositoryDirectory, GamePathService.SaveDirNameTQIT, GamePathService.SaveDataDirName);
		bool RepoTQPathExists = false, RepoTQITPathExists = false;

		var TQPathSaveData = this.PathIO.Combine(GamePathService.SaveFolderTQ, GamePathService.SaveDataDirName);
		var TQITPathSaveData = this.PathIO.Combine(GamePathService.SaveFolderTQIT, GamePathService.SaveDataDirName);
		var TQPathSaveDataExists = DirectoryIO.Exists(TQPathSaveData);
		var TQITPathSaveDataExists = DirectoryIO.Exists(TQITPathSaveData);

		// does Vault Dir has any vault file ?
		var vaultfiles = GamePathService.GetVaultList();

		// Cloned already
		if (GamePathService.LocalGitRepositoryGitDirExist)
		{
			// is there any remote commit i'm not aware of ?
			// I do pull only if i'm behind
			if (GitAmIBehind())
			{
				// pull player save files changes from github 
				// Untracked files will stay there, remote deleted tracked character will be deleted localy, remote updated will be localy updated here too.
				if (GitPull())
				{
					// Are they here after pull
					RepoTQPathExists = DirectoryIO.Exists(RepoTQPath);
					RepoTQITPathExists = DirectoryIO.Exists(RepoTQITPath);

					// Do you want to refresh your vault and local Save with last git save ?
					if (vaultfiles.Length != 0)
					{
						overrideFiles = DoYouWantToReplaceLocalVault();
					}

					// new remote vault files will be deployed, localy updated will be overrided if needed, else leave in place
					CopyVaultFilesFromRepoToVaultData(overrideFiles);

					if (this.UserSettings.GitBackupPlayerSavesEnabled)
					{
						//		Do you want to replace your local TQ character save with git repository version ? Are you sure ?
						if (RepoTQPathExists || RepoTQITPathExists)
						{
							overrideFiles = DoYouWantToReplaceLocalCharacterSave();

							//			if Yes - Copy Repo Player files to local SaveData override file
							//			if No - Copy Repo Player files to local SaveData ignore file

							if (RepoTQPathExists)
							{
								CopySaveDataFromRepoToMyGames(RepoTQPath, TQPathSaveData, overrideFiles);
							}

							if (RepoTQITPathExists)
							{
								CopySaveDataFromRepoToMyGames(RepoTQITPath, TQITPathSaveData, overrideFiles);
							}
						}
					}
				}
			}
			return;
		}

		// clone it
		var repoSuccess = GitClone();
		if (repoSuccess)
		{
			// Are they here after Clone
			RepoTQPathExists = DirectoryIO.Exists(RepoTQPath);
			RepoTQITPathExists = DirectoryIO.Exists(RepoTQITPath);

			overrideFiles = false;
			if (vaultfiles.Length != 0)
			{
				overrideFiles = DoYouWantToReplaceLocalVault();
			}
			CopyVaultFilesFromRepoToVaultData(overrideFiles);

			// Now Local vault files are refreshed from git repo

			//		Remove $Repo TQVaultData
			//			$Repo\TQVaultData
			this.RemoveRepoTQVaultData();

			//		Make Junction into $Repo for the TQVault directory
			this.LinkRepoTQVaultData();

			if (this.UserSettings.GitBackupPlayerSavesEnabled)
			{
				//		Do you want to replace your local TQ character save with git repository version ? Are you sure ?
				if (RepoTQPathExists || RepoTQITPathExists)
				{
					overrideFiles = DoYouWantToReplaceLocalCharacterSave();

					//			if Yes - Copy Repo Player files to local SaveData override file
					//			if No - Copy Repo Player files to local SaveData ignore file

					if (RepoTQPathExists)
					{
						CopySaveDataFromRepoToMyGames(RepoTQPath, TQPathSaveData, overrideFiles);
					}

					if (RepoTQITPathExists)
					{
						CopySaveDataFromRepoToMyGames(RepoTQITPath, TQITPathSaveData, overrideFiles);
					}
				}

				// Now Local game directories are refreshed from git repo

				// Clean repo and put inside NTFS Junctions to local game directories.
				// to make sure that git automaticaly sees the changes, save disk space and file copy time

				//		Remove $Repo SaveData 
				//			$Repo\Titan Quest\SaveData
				//			$Repo\Titan Quest - Immortal Throne\SaveData
				this.RemoveRepoSaveData(RepoTQPathExists, RepoTQITPathExists);

				//		Make Junction into $Repo for the 2 SaveData directories
				this.LinkRepoSaveData(
					TQPathSaveData, TQPathSaveDataExists, RepoTQPath
					, TQITPathSaveData, TQITPathSaveDataExists, RepoTQITPath
				);
			}
		}
	}


	#region This is specific to OS FileSystem

	protected virtual void CopyVaultFilesFromRepoToVaultData(bool overrideFiles)
	{
		throw new NotImplementedException();
	}

	protected virtual void CopySaveDataFromRepoToMyGames(string repoTQPath, string tQPathSaveData, bool overrideFiles)
	{
		throw new NotImplementedException();
	}

	protected virtual bool RemoveRepoSaveData(bool repoTQPathExists, bool repoTQITPathExists)
	{
		throw new NotImplementedException();
	}

	protected virtual bool LinkRepoSaveData(string tQPathSaveData, bool tQPathSaveDataExists, string repoTQPath, string tQITPathSaveData, bool tQITPathSaveDataExists, string repoTQITPath)
	{
		throw new NotImplementedException();
	}

	protected virtual bool LinkRepoTQVaultData()
	{
		throw new NotImplementedException();
	}

	protected virtual bool RemoveRepoTQVaultData()
	{
		throw new NotImplementedException();
	}

	#endregion

	public bool GitPull()
	{
		if (!GamePathService.LocalGitRepositoryGitDirExist)
			return false;

		var repoUrl = this.UserSettings.GitBackupRepository;
		string errMess = string.Format(Resources.GitUnableToPull, repoUrl);

		var pull = this.RunGitInRepository("pull", "origin");
		return HandleExecuteOut(pull, errMess, out var pulloutStd, out var pullerrStd);
	}

	/// <summary>
	/// Starts git in the local backup repository.
	/// </summary>
	private ShellCommand RunGitInRepository(params string[] arguments)
		=> ShellCommand.Run("git", arguments, this.GamePathService.LocalGitRepositoryDirectory);

	/// <summary>
	/// An external command started by <see cref="Run"/>: it runs hidden, and its standard output and error are read
	/// asynchronously from the start, so the process never blocks on a full pipe.
	/// </summary>
	protected sealed class ShellCommand : IDisposable
	{
		private readonly Process _process;
		private readonly List<string> _outputLines = new();
		private readonly List<string> _errorLines = new();

		private ShellCommand(Process process)
		{
			this._process = process;
		}

		/// <summary>
		/// Starts <paramref name="fileName"/> with <paramref name="arguments"/>, each one quoted as needed.
		/// </summary>
		/// <param name="workingDirectory">The working directory, or <c>null</c> for the current one.</param>
		/// <exception cref="System.ComponentModel.Win32Exception">The program cannot be started.</exception>
		public static ShellCommand Run(string fileName, IEnumerable<string> arguments, string? workingDirectory = null)
		{
			var startInfo = new ProcessStartInfo(fileName)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			foreach (var argument in arguments)
				startInfo.ArgumentList.Add(argument);
			if (workingDirectory is not null)
				startInfo.WorkingDirectory = workingDirectory;

			var process = new Process { StartInfo = startInfo };
			var command = new ShellCommand(process);
			// Each stream raises its events one at a time, and each one fills its own list.
			process.OutputDataReceived += (_, e) => { if (e.Data is not null) command._outputLines.Add(e.Data); };
			process.ErrorDataReceived += (_, e) => { if (e.Data is not null) command._errorLines.Add(e.Data); };
			try
			{
				process.Start();
			}
			catch
			{
				process.Dispose();
				throw;
			}
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return command;
		}

		/// <summary>
		/// Blocks until the process has exited and both streams are read to the end, then releases the process.
		/// </summary>
		/// <param name="outputLines">Receives the standard output lines, unless <c>null</c>.</param>
		/// <param name="errorLines">Receives the standard error lines, unless <c>null</c>.</param>
		/// <returns>The exit code.</returns>
		public int WaitForExit(ICollection<string>? outputLines, ICollection<string>? errorLines)
		{
			try
			{
				// Without a timeout, WaitForExit also waits for the end of the redirected streams.
				this._process.WaitForExit();

				if (outputLines is not null)
				{
					foreach (var line in this._outputLines)
						outputLines.Add(line);
				}

				if (errorLines is not null)
				{
					foreach (var line in this._errorLines)
						errorLines.Add(line);
				}

				return this._process.ExitCode;
			}
			finally
			{
				this._process.Dispose();
			}
		}

		public void Dispose() => this._process.Dispose();
	}

	protected bool HandleExecuteOut(ShellCommand cmd, string errMess, out BindingList<string> outputLines, out BindingList<string> errorLines, bool pipeOutputStandard = true)
	{
		outputLines = new BindingList<string>();
		errorLines = new BindingList<string>();

		return HandleExecuteRef(cmd, errMess, ref outputLines, ref errorLines, pipeOutputStandard);
	}

	protected bool HandleExecuteRef(ShellCommand cmd, string errMess, ref BindingList<string> outputLines, ref BindingList<string> errorLines, bool pipeOutputStandard = true)
	{
		string errLog = string.Empty;
		try
		{
			// Deliberately synchronous: callers (e.g. the push on app close) must finish before continuing.
			var exitCode = pipeOutputStandard
				? cmd.WaitForExit(outputLines, errorLines)
				: cmd.WaitForExit(null, null);

			var success = exitCode == 0;
			if (!success)
				errLog = errorLines.JoinString(Environment.NewLine);

			return success;
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "{Message}", errMess);
			errLog = ex.Message;
		}
		finally
		{
			if (errLog != string.Empty)
			{
				errMess += Environment.NewLine + errLog;
				this.Log.LogError("{Message}", errMess);
				this.UserNotifier.ShowError(errMess, Buttons: ShowMessageButtons.OK);
			}
		}
		return false;
	}

	public bool DoYouWantToReplaceLocalCharacterSave()
	{
		bool replaceAnswerOk = false, sureAnswer = true, overrideFiles = false;
		replaceAnswerOk = UserNotifier.ShowWarning(Resources.GitCloneDoYouWantToReplaceLocalCharacterSave).IsOK;
		sureAnswer = true;
		if (replaceAnswerOk)
			sureAnswer = UserNotifier.ShowWarning(Resources.GlobalConfirm).IsOK;

		overrideFiles = replaceAnswerOk && sureAnswer;
		return overrideFiles;
	}

	public bool DoYouWantToReplaceLocalVault()
	{
		bool replaceAnswerOk = false, sureAnswer = true, overrideFiles = false;
		replaceAnswerOk = UserNotifier.ShowWarning(Resources.GitCloneDoYouWantToReplaceLocalVault).IsOK;
		sureAnswer = true;
		if (replaceAnswerOk)
			sureAnswer = UserNotifier.ShowWarning(Resources.GlobalConfirm).IsOK;

		overrideFiles = replaceAnswerOk && sureAnswer;
		return overrideFiles;
	}

	public string? BackupFile(string prefix, string file)
	{
		if (this.FileIO.Exists(file))
		{
			// only backup if it exists!
			string backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, file);

			this.FileIO.Copy(file, backupFile);

			// Added by VillageIdiot
			// Backup the file pairs for the player stash files.
			if (PathIO.GetFileName(file).Equals(GamePathService.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
			{
				string dxgfile = this.PathIO.ChangeExtension(file, ".dxg");

				if (this.FileIO.Exists(dxgfile))
				{
					// only backup if it exists!
					backupFile = GamePathService.ConvertFilePathToBackupPath(prefix, dxgfile);

					this.FileIO.Copy(dxgfile, backupFile);
				}
			}

			return backupFile;
		}
		else
			return null;
	}

	public string DuplicateCharacterFiles(string playerSaveDirectory, string newname)
	{
		var baseFolder = this.PathIO.GetDirectoryName(playerSaveDirectory);
		var newFolder = this.PathIO.Combine(baseFolder, $"_{newname}");
		var newPlayerFile = this.PathIO.Combine(newFolder, GamePathService.PlayerSaveFileName);

		var playerFile = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerSaveFileName);
		var stashFileB = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameB);
		var stashFileG = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerStashFileNameG);
		var settingsFile = this.PathIO.Combine(playerSaveDirectory, GamePathService.PlayerSettingsFileName);

		DirectoryIO.CreateDirectory(newFolder);
		this.FileIO.Copy(playerFile, newPlayerFile);
		if (this.FileIO.Exists(stashFileB)) this.FileIO.Copy(stashFileB, this.PathIO.Combine(newFolder, GamePathService.PlayerStashFileNameB));
		if (this.FileIO.Exists(stashFileG)) this.FileIO.Copy(stashFileG, this.PathIO.Combine(newFolder, GamePathService.PlayerStashFileNameG));
		if (this.FileIO.Exists(settingsFile)) this.FileIO.Copy(settingsFile, this.PathIO.Combine(newFolder, GamePathService.PlayerSettingsFileName));

		// Copy Progression directory
		var sourceProgressionDir = this.PathIO.Combine(playerSaveDirectory, "Levels_World_World01.map");
		var destProgressionDir = this.PathIO.Combine(newFolder, "Levels_World_World01.map");
		if (DirectoryIO.Exists(sourceProgressionDir))
		{
			CopyDirectoryRecursive(sourceProgressionDir, destProgressionDir);
		}

		return newFolder;
	}

	private void CopyDirectoryRecursive(string sourceDir, string destDir)
	{
		if (!DirectoryIO.Exists(destDir))
			DirectoryIO.CreateDirectory(destDir);

		foreach (var file in DirectoryIO.GetFiles(sourceDir))
		{
			var destFile = this.PathIO.Combine(destDir, this.PathIO.GetFileName(file));
			this.FileIO.Copy(file, destFile, true);
		}

		foreach (var subDir in DirectoryIO.GetDirectories(sourceDir))
		{
			var destSubDir = this.PathIO.Combine(destDir, this.PathIO.GetFileName(subDir));
			CopyDirectoryRecursive(subDir, destSubDir);
		}
	}

	public void BackupStupidPlayerBackupFolder(string playerFile)
	{
		string playerFolder = this.PathIO.GetDirectoryName(playerFile);
		string backupFolder = this.PathIO.Combine(playerFolder, "Backup");
		string newFolder = this.PathIO.Combine(playerFolder, "Backup-moved by TQVault");
		var existsbackupFolder = DirectoryIO.Exists(backupFolder);
		var existsnewFolder = DirectoryIO.Exists(newFolder);

		if (existsbackupFolder)
		{
			// we need to move it.
			if (existsnewFolder)
			{
				try
				{
					// It already exists--we need to remove it
					DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
				{
					this.Log.LogWarning(ex, "Unable to remove {Folder}, using a numbered folder instead", newFolder);
					int fn = 1;
					while (DirectoryIO.Exists(string.Format("{0}({1})", newFolder, fn)))
					{
						fn++;
					}
					newFolder = string.Format("{0}({1})", newFolder, fn);
				}
			}

			DirectoryIO.Move(backupFolder, newFolder);
		}

		// Change TQVault moved backup to zip to avoid game reading it's content : https://github.com/EtienneLamoureux/TQVaultAE/issues/535
		existsnewFolder = DirectoryIO.Exists(newFolder);
		if (existsnewFolder)
		{
			var zipFile = $"{newFolder}.zip";
			var zipAlreadyExists= FileIO.Exists(zipFile);

			// Delete old zip file
			if (zipAlreadyExists)
			{
				try
				{
					FileIO.Delete(zipFile);
					zipAlreadyExists = false;
				}
				catch (Exception e)
				{
					this.Log.LogWarning(e, "Unable to delete {zipFile}", zipFile);
				}
			}
			
			// Create new one
			if (!zipAlreadyExists)
			{
				try
				{
					ZipFile.CreateFromDirectory(newFolder, zipFile, CompressionLevel.Fastest, false);
					DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception e)
				{
					this.Log.LogWarning(e, "Unable to zip new {zipFile}", zipFile);
				}
			}
		}
	}

	public bool Archive(PlayerSave ps)
	{
		if (ps is null)
			return false;

		if (ps.IsArchived)// Double Check
			return true;

		// Disable filewatchers
		ToggleFileWatchers(ps, false);

		// Move directory
		var oldFolder = ps.Folder;

		// -- Prep arbo
		var archDirNameTarget = this.GamePathService.GetBaseCharacterFolder(ps.IsImmortalThrone, true);
		if (!DirectoryIO.Exists(archDirNameTarget))
			DirectoryIO.CreateDirectory(archDirNameTarget);

		var newFolder = GamePathService.ArchiveTogglePath(oldFolder);

		Exception? ex = null;
		try
		{
			DirectoryIO.Move(oldFolder, newFolder);
		}
		catch (Exception exx)
		{
			ex = exx;
			var errMess = "Error during move directory!";
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, ex, Buttons: ShowMessageButtons.OK);
		}
		finally
		{
			// Update PlayerSave
			if (ex is null)
			{
				ps.IsArchived = true;
				ps.Folder = newFolder;

				if (ps.PlayerSaveWatcher is not null)
					ps.PlayerSaveWatcher.Path = newFolder;

				if (ps.PlayerStashWatcher is not null)
					ps.PlayerStashWatcher.Path = newFolder;
			}
			// Restart filewatchers
			ToggleFileWatchers(ps, true);
		}

		return ex is null;
	}

	static void ToggleFileWatchers(PlayerSave ps, bool enable)
	{
		if (ps.PlayerSaveWatcher is not null)
			ps.PlayerSaveWatcher.EnableRaisingEvents = enable;

		if (ps.PlayerStashWatcher is not null)
			ps.PlayerStashWatcher.EnableRaisingEvents = enable;
	}

	public bool Unarchive(PlayerSave ps)
	{
		if (ps is null)
			return false;

		if (!ps.IsArchived)// Double Check
			return true;

		// Disable filewatchers
		ToggleFileWatchers(ps, false);

		// Move directory
		var oldFolder = ps.Folder;// Should include archDirName

		var newFolder = GamePathService.ArchiveTogglePath(oldFolder);

		Exception? ex = null;
		try
		{
			DirectoryIO.Move(oldFolder, newFolder);
		}
		catch (Exception exx)
		{
			ex = exx;
			var errMess = "Error during move directory!";
			this.Log.LogError(ex, "{Message}", errMess);
			this.UserNotifier.ShowError(errMess, ex, Buttons: ShowMessageButtons.OK);
		}
		finally
		{
			// Update PlayerSave
			if (ex is null)
			{
				ps.IsArchived = false;
				ps.Folder = newFolder;

				if (ps.PlayerSaveWatcher is not null)
					ps.PlayerSaveWatcher.Path = newFolder;

				if (ps.PlayerStashWatcher is not null)
					ps.PlayerStashWatcher.Path = newFolder;
			}

			// Restart filewatchers
			ToggleFileWatchers(ps, true);
		}

		return ex is null;
	}
}