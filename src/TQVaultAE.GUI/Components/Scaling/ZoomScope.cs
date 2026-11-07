namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// Marks a <see cref="Control.Scale(SizeF)"/> made for the user's zoom, as opposed to the DPI scaling Windows Forms does
/// by itself when a form loads.
/// </summary>
/// <remarks>
/// <see cref="Control.Scale(SizeF)"/> scales bounds but not fonts, so the scaling controls grow their fonts in
/// <c>ScaleControl</c>. That is right for a zoom, but fonts sized in points already follow the DPI: growing them during
/// the DPI scaling too would apply it twice.
/// </remarks>
internal static class ZoomScope
{
	[ThreadStatic]
	private static int depth;

	/// <summary>Gets a value indicating whether a zoom is being applied on this thread.</summary>
	public static bool IsZooming => depth > 0;

	/// <summary>Starts a zoom, until the result is disposed.</summary>
	public static Scope Enter()
	{
		depth++;
		return default;
	}

	/// <summary><paramref name="font"/> grown by <paramref name="factor"/> during a zoom, itself otherwise.</summary>
	public static Font ScaleFont(Font font, float factor)
		=> IsZooming && factor != 1.0F ? new Font(font.FontFamily, font.SizeInPoints * factor, font.Style) : font;

	/// <summary>Ends the zoom started by <see cref="Enter"/>.</summary>
	public readonly struct Scope : IDisposable
	{
		public void Dispose() => depth--;
	}
}
