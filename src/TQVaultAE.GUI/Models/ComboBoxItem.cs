namespace TQVaultAE.GUI.Models;

internal class ComboBoxItem
{
	public string DisplayName = string.Empty;
	public string Value = string.Empty;

	public override string ToString()
	{
		// Generates the text shown in the combo box
		return DisplayName;
	}
}

internal class ComboBoxItem<TComboValue, TValue>
{
	public required string DisplayName;
	public required TComboValue ComboValue;
	public required TValue Value;

	public override string ToString()
	{
		// Generates the text shown in the combo box
		return DisplayName;
	}
}