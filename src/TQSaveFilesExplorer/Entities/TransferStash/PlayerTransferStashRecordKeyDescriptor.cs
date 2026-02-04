namespace TQ.SaveFilesExplorer.Entities.TransferStash
{
	public class PlayerTransferStashRecordKeyDescriptor
	{
		public TQFilePlayerTransferStashKey Enum { get; set; }
		public string Name { get; set; }
		public TQFileDataType DataType { get; set; }
		public TQVersion Version { get; set; }
	}
}
