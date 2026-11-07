using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Class for rendering the context menu strip.
/// </summary>
internal class CustomProfessionalRenderer : ToolStripProfessionalRenderer
{
	/// <summary>
	/// Handler for rendering the contect meny strip.
	/// </summary>
	/// <param name="e">ToolStripItemTextRenderEventArgs data</param>
	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		if (e.Item.Selected)
			e.TextColor = Color.Black;
		else
		{
			if (e.Item?.Name?.StartsWith(ComboBoxCharacter.TAGKEY, StringComparison.Ordinal) ?? false)
				e.TextColor = Color.Black;
			else
				e.TextColor = Color.FromArgb(200, 200, 200);
		}

		base.OnRenderItemText(e);
	}
}
