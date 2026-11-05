//-----------------------------------------------------------------------
// <copyright file="ArcFile.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities;

using TQVaultAE.GameFiles;

/// <summary>
/// Holds information about the directory entry.
/// </summary>
public class ArcDirEntry
{
	/// <summary>
	/// Gets or sets the filename.
	/// </summary>
	public RecordId FileName { get; set; }

	/// <summary>
	/// Gets or sets the storage type.
	/// Data is either compressed (3) or stored (1)
	/// </summary>
	public int StorageType { get; set; }

	/// <summary>
	/// Gets or sets the compressed size of this entry.
	/// </summary>
	public long CompressedSize { get; set; }

	/// <summary>
	/// Gets or sets the real size of this entry.
	/// </summary>
	public long RealSize { get; set; }

	/// <summary>
	/// Entry of the shared reader, used to read the content.
	/// </summary>
	public ArcEntry? Entry { get; set; }
}
