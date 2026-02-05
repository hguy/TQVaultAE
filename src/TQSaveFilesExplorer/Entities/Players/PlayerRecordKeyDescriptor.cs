namespace TQ.SaveFilesExplorer.Entities.Players
{
	public class PlayerRecordKeyDescriptor
	{
		public TQFilePlayerRecordKey Enum { get; set; }
		public string Name { get; set; }
		public TQFileDataType DataType { get; set; }
		public TQVersion Version { get; set; }
	}
}
