using System.IO;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data
{
	public class DBRecordCollectionProvider : IDBRecordCollectionProvider
	{
		private readonly IDirectoryIO DirectoryIO;
		private readonly IPathIO PathIO;

		public DBRecordCollectionProvider(IDirectoryIO directoryIO, IPathIO pathIO)
		{
			this.DirectoryIO = directoryIO;
			PathIO = pathIO;
		}

		/// <summary>
		/// Writes all variables into a file.
		/// </summary>
		/// <param name="drc">source</param>
		/// <param name="baseFolder">Path in the file.</param>
		/// <param name="fileName">file name to be written</param>
		public void Write(DBRecordCollection drc, string baseFolder, string fileName = null)
		{
			// construct's full path
			string fullPath = PathIO.Combine(baseFolder, drc.Id.Normalized);
			string destinationFolder = PathIO.GetDirectoryName(fullPath);

			if (fileName != null)
			{
				fullPath = PathIO.Combine(baseFolder, fileName);
				destinationFolder = baseFolder;
			}

			// Create's folder path if necessary
			if (!DirectoryIO.Exists(destinationFolder))
				DirectoryIO.CreateDirectory(destinationFolder);

			// Open's file
			using (StreamWriter outStream = new StreamWriter(fullPath, false))
			{
				// Write's all variables
				foreach (Variable variable in drc)
				{
					outStream.WriteLine(variable.ToString());
				}
			}
		}
	}
}
