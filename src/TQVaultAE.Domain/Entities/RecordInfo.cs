using TQVaultAE.GameFiles;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Record table entry of an <see cref="ArzFile"/>: available without decompressing the record.
/// </summary>
public class RecordInfo
{
	public RecordInfo()
	{
		this.RecordType = string.Empty;
	}

	/// <summary>Absolute file offset of the compressed record.</summary>
	public long Offset;

	/// <summary>Compressed size in bytes.</summary>
	public int CompressedSize;

	/// <summary>Entry of the shared reader, used to decode the record.</summary>
	public ArzRecordInfo? Entry { get; set; }

	public RecordId ID { get; set; } = RecordId.Empty;

	public string RecordType { get; set; }
}
