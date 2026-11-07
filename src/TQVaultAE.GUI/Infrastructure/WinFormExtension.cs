namespace TQVaultAE.GUI.Infrastructure;

public static class WinFormExtension
{
	/// <summary>
	/// Text of the selected item, or an empty string when nothing is selected.
	/// </summary>
	public static string SelectedItemText(this ComboBox comboBox)
		=> comboBox.SelectedItem?.ToString() ?? string.Empty;

	public static void ProcessAllControls(this Control rootControl, Action<Control> action)
	{
		foreach (Control childControl in rootControl.Controls)
		{
			ProcessAllControls(childControl, action);
			action(childControl);
		}
	}

	public static void ProcessAllControlsWithExclusion(this Control rootControl, Func<Control, bool> action, Control? extraControl = null)
	{
		if (extraControl is not null)
			action(extraControl);// Used to add rootControl in the loop

		foreach (Control childControl in rootControl.Controls)
		{
			var excludeFromHere = action(childControl);
			if (excludeFromHere)
				continue;

			ProcessAllControlsWithExclusion(childControl, action);
		}
	}

	public static void AdjustToMaxTextWidth(this CheckedListBox ctrl, int? maxVerticalItems)
	{
		var width = ctrl.GetMaxTextWidth();

		// i add this for the size of the checkbox control in the begining of the item {CheckBoxWidth} + {TextWidth}
		width += SystemInformation.VerticalScrollBarWidth;

		ctrl.Width = ctrl.ColumnWidth = width;// The control must fit the size of the column
	}

	/// <summary>Sizes the list to one column as wide as its widest item, check box included.</summary>
	public static void AdjustToMaxTextWidth(this Components.Layout.VirtualCheckedListBox ctrl, int? maxVerticalItems)
	{
		// Check box, the gap before the text and a little room after it.
		var width = ctrl.TextLeft + ctrl.GetMaxTextWidth() + 6;

		ctrl.Width = ctrl.ColumnWidth = width;// The control must fit the size of the column
	}

	public static int GetMaxTextWidth(this CheckedListBox ctrl)
	{
		int maxwidth = 0, width;
		foreach (var item in ctrl.Items)
		{
			width = TextRenderer.MeasureText(item.ToString(), ctrl.Font).Width;
			maxwidth = Math.Max(width, maxwidth);
		}
		return maxwidth;
	}

}