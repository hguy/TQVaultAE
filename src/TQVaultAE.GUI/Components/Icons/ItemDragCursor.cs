using System.Runtime.InteropServices;

namespace TQVaultAE.GUI.Components.Icons;

/// <summary>
/// Mouse cursor showing an item bitmap, with its hot spot at the centre: Windows draws and moves it, so dragging over
/// a control costs no repaint, and the bitmap keeps its transparency.
/// </summary>
internal sealed partial class ItemDragCursor : IDisposable
{
	private IntPtr _handle;

	private ItemDragCursor(IntPtr handle)
	{
		this._handle = handle;
		this.Cursor = new Cursor(handle);
	}

	public Cursor Cursor { get; }

	/// <summary>Builds a cursor from <paramref name="bitmap"/>, drawn at <paramref name="scale"/>.</summary>
	/// <returns><c>null</c> when Windows cannot create it.</returns>
	public static ItemDragCursor? Create(Bitmap bitmap, float scale)
	{
		int width = Math.Max(1, (int)Math.Round(bitmap.Width * scale));
		int height = Math.Max(1, (int)Math.Round(bitmap.Height * scale));
		using var sized = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (var g = Graphics.FromImage(sized))
		{
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.DrawImage(bitmap, 0, 0, width, height);
		}

		IntPtr icon = sized.GetHicon();
		try
		{
			if (!GetIconInfo(icon, out var info))
				return null;
			try
			{
				info.fIcon = 0; // FALSE: a cursor
				info.xHotspot = width / 2;
				info.yHotspot = height / 2;
				IntPtr cursor = CreateIconIndirect(ref info);
				return cursor == IntPtr.Zero ? null : new ItemDragCursor(cursor);
			}
			finally
			{
				DeleteObject(info.hbmColor);
				DeleteObject(info.hbmMask);
			}
		}
		finally
		{
			DestroyIcon(icon);
		}
	}

	public void Dispose()
	{
		if (this._handle == IntPtr.Zero)
			return;
		this.Cursor.Dispose();
		DestroyCursor(this._handle);
		this._handle = IntPtr.Zero;
	}

	/// <summary>
	/// ICONINFO. <c>fIcon</c> is a Win32 BOOL kept as an <see cref="int"/> so the struct stays blittable
	/// for the source-generated P/Invokes.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	private struct IconInfo
	{
		public int fIcon;
		public int xHotspot;
		public int yHotspot;
		public IntPtr hbmMask;
		public IntPtr hbmColor;
	}

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial IntPtr CreateIconIndirect(ref IconInfo icon);

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DestroyIcon(IntPtr hIcon);

	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DestroyCursor(IntPtr hCursor);

	[LibraryImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DeleteObject(IntPtr hObject);
}
