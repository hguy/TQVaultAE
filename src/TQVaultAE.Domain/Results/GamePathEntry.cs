namespace TQVaultAE.Domain.Results
{
	public class GamePathEntry
	{
		public readonly string Path;
		public readonly string DisplayName;
		public GamePathEntry(string path, string displayName)
		{
			this.Path = path;
			this.DisplayName = displayName;
		}
		public override string ToString() 
			=> DisplayName ?? Path ?? "Empty";
	}
}
