using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GameFiles;
using TQVaultAE.Logs;

namespace TQVaultAE.Data;

public class ArcFileProvider : IArcFileProvider
{
	private readonly ILogger Log;
	private readonly IPathIO PathIO;
	private readonly IDirectoryIO DirectoryIO;
	private readonly UserSettings USettings;

	public ArcFileProvider(ILogger<ArcFileProvider> log, IPathIO pathIO, IDirectoryIO directoryIO, UserSettings uSettings)
	{
		this.Log = log;
		this.PathIO = pathIO;
		this.DirectoryIO = directoryIO;
		this.USettings = uSettings;
	}

	#region ArcFile Public Methods

	/// <summary>
	/// Reads the ARC file table of contents to determine if the file is readable.
	/// </summary>
	/// <returns>True if able to read the ToC</returns>
	public bool Read(ArcFile file)
	{
		try
		{
			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			return file.DirectoryEntries.Any();
		}
		catch (IOException exception)
		{
			Log.ErrorException(exception);
			return false;
		}
	}

	/// <summary>
	/// Writes a record to a file.
	/// </summary>
	/// <param name="baseFolder">string holding the base folder path</param>
	/// <param name="record">Record we are writing</param>
	/// <param name="destinationFileName">Filename for the new file.</param>
	public void Write(ArcFile file, string baseFolder, RecordId record, string destinationFileName)
	{
		try
		{
			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			string dataID = string.Concat(this.PathIO.GetFileNameWithoutExtension(file.FileName), "\\", record.Raw);
			byte[]? data = this.GetData(file, dataID);
			if (data == null)
				return;

			string destination = baseFolder;
			if (!destination.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
				destination = string.Concat(destination, "\\");

			destination = string.Concat(destination, destinationFileName);

			// If there is a sub directory in the arc file then we need to create it.
			var destDirName = PathIO.GetDirectoryName(destination);
			if (!DirectoryIO.Exists(destDirName))
				DirectoryIO.CreateDirectory(destDirName);

			using (FileStream outStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
			{
				outStream.Write(data, 0, data.Length);
			}
		}
		catch (IOException exception)
		{
			Log.ErrorException(exception);
			return;
		}
	}

	/// <summary>
	/// Reads data from an ARC file and puts it into a Byte array (or NULL if not found)
	/// </summary>
	/// <param name="dataId">The string ID for the data which we are retieving.</param>
	/// <returns>Returns byte array of the data corresponding to the string ID.</returns>
	public byte[]? GetData(ArcFile file, RecordId dataId)
	{
		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.GetData({DataId})", dataId);

		if (!file.FileHasBeenRead)
			this.ReadARCToC(file);

		if (!file.DirectoryEntries.Any())
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - Could not read {FileName}", file.FileName);

			return null;
		}

		if (USettings.ARCFileDebugLevel > 1)
			Log.LogDebug("Normalized dataID = {DataId}", dataId);

		int firstPathDelim = dataId.Normalized.IndexOf('\\');
		if (firstPathDelim != -1)
			dataId = dataId.Normalized.Substring(firstPathDelim + 1);

		ArcDirEntry directoryEntry;

		if (file.DirectoryEntries.ContainsKey(dataId))
			directoryEntry = file.DirectoryEntries[dataId];
		else
		{
			if (USettings.ARCFileDebugLevel > 1)
				Log.LogDebug("Error - {DataId} not found.", dataId);

			return null;
		}

		byte[] data = file.Archive!.ReadAll(directoryEntry.Entry!);

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("Exiting ARCFile.GetData()");

		return data;
	}

	/// <summary>
	/// Extracts the decoded ARC file contents into a folder.
	/// </summary>
	/// <param name="destination">Destination folder for the files.</param>
	/// <returns>true if successful, false on error.</returns>
	public bool ExtractArcFile(ArcFile file, string destination)
	{
		try
		{
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("ARCFile.ReadARCFile()");

			if (!file.FileHasBeenRead)
				this.ReadARCToC(file);

			foreach (ArcDirEntry dirEntry in file.DirectoryEntries.Values)
			{
				RecordId dataID = string.Concat(
					this.PathIO.GetFileNameWithoutExtension(file.FileName), "\\"
					, dirEntry.FileName.Raw
				);

				if (USettings.ARCFileDebugLevel > 1)
				{
					Log.LogDebug("Directory Filename = {DirEntryFileName}", dirEntry.FileName);
					Log.LogDebug("dataID = {DataID}", dataID);
				}

				byte[]? data = this.GetData(file, dataID);
				if (data is null)
					continue;

				string filename = destination;
				if (!filename.EndsWith("\\", StringComparison.Ordinal))
					filename = string.Concat(filename, "\\");

				filename = string.Concat(filename, dirEntry.FileName);

				// If there is a sub directory in the arc file then we need to create it.
				var dirName = PathIO.GetDirectoryName(filename);
				if (!DirectoryIO.Exists(dirName))
					DirectoryIO.CreateDirectory(dirName);

				if (USettings.ARCFileDebugLevel > 1)
					Log.LogDebug("Creating File - {Filename}", filename);

				using (FileStream outStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
				{
					outStream.Write(data, 0, data.Length);
				}
			}

			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug("Exiting ARCFile.ReadARCFile()");

			return true;
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "ARCFile.ReadARCFile() - Error reading arcfile");
			return false;
		}
	}

	#endregion ArcFile Public Methods

	#region ArcFile Private Methods

	/// <summary>
	/// Read the table of contents of the ARC file through the shared <see cref="IArcArchive"/> reader.
	/// </summary>
	/// <remarks>An unreadable archive leaves <see cref="ArcFile.DirectoryEntries"/> empty.</remarks>
	public void ReadARCToC(ArcFile file)
	{
		file.FileHasBeenRead = true;

		IArcArchive archive;
		try
		{
			archive = ArcArchiveFactory.Open(file.FileName);
		}
		catch (InvalidDataException exception)
		{
			// Empty or truncated archives (e.g. XPack4\Underground.arc) have no readable entries.
			if (USettings.ARCFileDebugLevel > 0)
				Log.LogDebug(exception, "ARCFile.ReadARCToC({File}) - not a readable archive", file.FileName);

			file.DirectoryEntries = new();
			return;
		}

		file.Archive?.Dispose();
		file.Archive = archive;

		var entries = new Dictionary<RecordId, ArcDirEntry>(archive.Entries.Count);
		foreach (var entry in archive.Entries)
		{
			var dirEntry = new ArcDirEntry
			{
				FileName = entry.Name,
				StorageType = entry.StorageType,
				CompressedSize = entry.CompressedSize,
				RealSize = entry.Size,
				Entry = entry,
			};
			entries.TryAdd(dirEntry.FileName, dirEntry);
		}

		file.DirectoryEntries = entries;

		if (USettings.ARCFileDebugLevel > 0)
			Log.LogDebug("ARCFile.ReadARCToC({File}) - {Count} entries", file.FileName, entries.Count);
	}

	#endregion ArcFile Private Methods
}
