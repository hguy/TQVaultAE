namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Colors of the TQVault look that several controls share, named once.
/// </summary>
/// <remarks>
/// Colors used by one control only stay in that control, next to the drawing that uses them; the designer files keep
/// their own literal colors, since the designer rewrites them.
/// </remarks>
internal static class Theme
{
	/// <summary>Dark brown background of the panels and context menus.</summary>
	public static readonly Color Background = Color.FromArgb(46, 41, 31);

	/// <summary>Text of the context menu items that are not selected.</summary>
	public static readonly Color MenuText = Color.FromArgb(200, 200, 200);

	/// <summary>Dark brown text drawn on the gold buttons.</summary>
	public static readonly Color ButtonText = Color.FromArgb(51, 44, 28);

	/// <summary>
	/// <see cref="ButtonText"/> with no opacity: the hover and pressed back color of the flat picture buttons, so only
	/// their pictures change.
	/// </summary>
	public static readonly Color TransparentButton = Color.FromArgb(0, ButtonText);
}
