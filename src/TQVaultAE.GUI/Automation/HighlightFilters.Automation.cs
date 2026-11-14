//-----------------------------------------------------------------------
// <copyright file="HighlightFilters.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Components;

/// <summary>
/// Debug-only entry points used by the automation endpoint.
/// </summary>
public partial class HighlightFilters
{
	/// <summary>The check boxes of the panel, in layout order.</summary>
	internal IReadOnlyList<CheckBox> AutomationCheckBoxes => Descendants(this).OfType<CheckBox>().ToList();

	/// <summary>Applies the filters like the Apply button.</summary>
	internal void AutomationApply() => this.buttonSave_Click(this, EventArgs.Empty);

	private static IEnumerable<Control> Descendants(Control parent)
	{
		foreach (Control child in parent.Controls)
		{
			yield return child;
			foreach (var descendant in Descendants(child))
				yield return descendant;
		}
	}
}
