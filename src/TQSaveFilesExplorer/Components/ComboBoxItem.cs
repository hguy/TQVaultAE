namespace TQ.SaveFilesExplorer.Components
{
	internal sealed class ComboBoxItem
	{
		public string DisplayName;
		public string Value;

		public override string ToString()
		{
			// Generates the text shown in the combo box
			return DisplayName;
		}
	}
}
