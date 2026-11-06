//-----------------------------------------------------------------------
// <copyright file="NativeWindows.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Runtime.InteropServices;
using System.Text;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Win32 helpers to find and answer the process' own top-level windows, including native message boxes
/// that are not part of <see cref="System.Windows.Forms.Application.OpenForms"/>.
/// All calls work from any thread.
/// </summary>
internal static class NativeWindows
{
	private const int WM_COMMAND = 0x0111;
	private const int BN_CLICKED = 0;

	/// <summary>
	/// Standard dialog command ids (IDOK, IDCANCEL, ...) used as a fallback when a button is not a child window (task dialogs).
	/// </summary>
	private static readonly Dictionary<string, int> StandardCommandIds = new(StringComparer.OrdinalIgnoreCase)
	{
		["OK"] = 1,
		["Cancel"] = 2,
		["Abort"] = 3,
		["Retry"] = 4,
		["Ignore"] = 5,
		["Yes"] = 6,
		["No"] = 7,
		["Close"] = 8,
	};

	public sealed record NativeWindow(IntPtr Handle, string Title, string ClassName, bool Visible, bool Enabled, IntPtr Owner)
	{
		/// <summary>
		/// Gets a value indicating whether this is a standard Win32 dialog (message box, task dialog, common dialog).
		/// </summary>
		public bool IsDialogClass => this.ClassName == "#32770";
	}

	public sealed record NativeButton(IntPtr Handle, int ControlId, string Text);

	/// <summary>
	/// Lists the visible top-level windows that belong to the current process.
	/// </summary>
	public static List<NativeWindow> GetProcessTopLevelWindows()
	{
		var pid = (uint)Environment.ProcessId;
		var result = new List<NativeWindow>();
		EnumWindows((hwnd, _) =>
		{
			GetWindowThreadProcessId(hwnd, out var windowPid);
			if (windowPid == pid && IsWindowVisible(hwnd))
				result.Add(Describe(hwnd));
			return true;
		}, IntPtr.Zero);
		return result;
	}

	/// <summary>
	/// Lists the button child windows of a dialog.
	/// </summary>
	public static List<NativeButton> GetButtons(IntPtr dialog)
	{
		var result = new List<NativeButton>();
		EnumChildWindows(dialog, (hwnd, _) =>
		{
			// "Button" for native dialogs, "WindowsForms10.BUTTON.app..." for WinForms buttons.
			if (GetClass(hwnd).Contains("BUTTON", StringComparison.OrdinalIgnoreCase) && IsWindowVisible(hwnd) && IsWindowEnabled(hwnd))
				result.Add(new NativeButton(hwnd, GetDlgCtrlID(hwnd), GetText(hwnd)));
			return true;
		}, IntPtr.Zero);
		return result;
	}

	/// <summary>
	/// Presses a dialog button by its text (accelerator ampersands are ignored).
	/// Falls back to standard command ids (OK, Cancel, Yes, No...) for dialogs without button child windows.
	/// </summary>
	/// <returns>The text of the pressed button, or null when nothing matched.</returns>
	public static string? PressButton(IntPtr dialog, string buttonText)
	{
		var wanted = Normalize(buttonText);
		var button = GetButtons(dialog).FirstOrDefault(b => Normalize(b.Text) == wanted);
		if (button is not null)
		{
			// WM_COMMAND/BN_CLICKED to the parent works even when the dialog is not the foreground window, unlike BM_CLICK.
			// WinForms reflects it to the button, which raises Click.
			PostMessage(GetParent(button.Handle), WM_COMMAND, (IntPtr)((BN_CLICKED << 16) | (button.ControlId & 0xFFFF)), button.Handle);
			return button.Text;
		}

		if (StandardCommandIds.TryGetValue(wanted, out var id))
		{
			PostMessage(dialog, WM_COMMAND, (IntPtr)id, IntPtr.Zero);
			return buttonText;
		}

		return null;
	}

	/// <summary>
	/// Gets the text of the static (label) children of a dialog, e.g. a message box body.
	/// </summary>
	public static List<string> GetStaticTexts(IntPtr dialog)
	{
		var result = new List<string>();
		EnumChildWindows(dialog, (hwnd, _) =>
		{
			if (GetClass(hwnd) == "Static")
			{
				var text = GetText(hwnd);
				if (!string.IsNullOrWhiteSpace(text))
					result.Add(text);
			}
			return true;
		}, IntPtr.Zero);
		return result;
	}

	private static string Normalize(string text) => text.Replace("&", string.Empty).Trim();

	private static NativeWindow Describe(IntPtr hwnd)
		=> new(hwnd, GetText(hwnd), GetClass(hwnd), IsWindowVisible(hwnd), IsWindowEnabled(hwnd), GetWindow(hwnd, GW_OWNER));

	private static string GetText(IntPtr hwnd)
	{
		var length = GetWindowTextLength(hwnd);
		var sb = new StringBuilder(length + 1);
		GetWindowText(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	private static string GetClass(IntPtr hwnd)
	{
		var sb = new StringBuilder(256);
		GetClassName(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	private const uint GW_OWNER = 4;

	private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsWindowEnabled(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int GetDlgCtrlID(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
