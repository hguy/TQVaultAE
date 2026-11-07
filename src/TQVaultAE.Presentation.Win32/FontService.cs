using EnumsNET;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Presentation.Win32;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Singleton: its font families live as long as the application.")]
public class FontService : IFontService
{
	private readonly UserSettings USettings;
	public IAddFontToOS FontLoader { get; private set; }

	const string ALBERTUSMT_NAME = "Albertus MT";
	const string ALBERTUSMTLIGHT_NAME = "Albertus MT Light";

	private FontFamily? _FONT_ALBERTUSMT = null;
	private FontFamily FONT_ALBERTUSMT
	{
		get
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				// Code here won't run in Visual Studio designer but runtime 
				if (FontLoader != null && _FONT_ALBERTUSMT is null)
				{
					var baseFont = Enums.Parse<FontFamilyList>(USettings.BaseFont ?? FontFamilyList.AlbertusMT.ToString());
					switch (baseFont)
					{
						case FontFamilyList.AlbertusMT:
							_FONT_ALBERTUSMT = FontLoader.AddFontToOS(ALBERTUSMT_NAME, Resources.AlbertusMT);
							break;
						case FontFamilyList.Arial:
						case FontFamilyList.Verdana:
							_FONT_ALBERTUSMT = new FontFamily(baseFont.ToString());
							break;
						case FontFamilyList.TimesNewRoman:
							_FONT_ALBERTUSMT = new FontFamily(baseFont.AsString(EnumFormat.Description) ?? baseFont.ToString());
							break;
					}
				}
			}
			else
			{
				// Design time
				if (_FONT_ALBERTUSMT is null)
				{
					try
					{
						_FONT_ALBERTUSMT = new FontFamily(ALBERTUSMT_NAME);
					}
					catch
					{ // fallback to Safe font
						_FONT_ALBERTUSMT = new FontFamily("Arial");
					}
				}
			}
			// The font could not be created: let GDI+ pick a safe one rather than fail.
			return _FONT_ALBERTUSMT ?? FontFamily.GenericSansSerif;
		}
	}

	private FontFamily? _FONT_ALBERTUSMTLIGHT = null;

	private FontFamily FONT_ALBERTUSMTLIGHT
	{
		get
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				// Code here won't run in Visual Studio designer
				if (FontLoader != null && _FONT_ALBERTUSMTLIGHT is null)
				{
					var baseFont = Enums.Parse<FontFamilyList>(USettings.BaseFont ?? FontFamilyList.AlbertusMT.ToString());
					switch (baseFont)
					{
						case FontFamilyList.AlbertusMT:
							_FONT_ALBERTUSMTLIGHT = FontLoader.AddFontToOS(ALBERTUSMTLIGHT_NAME, Resources.AlbertusMTLight);// Runtime
							break;
						case FontFamilyList.Arial:
						case FontFamilyList.Verdana:
							_FONT_ALBERTUSMTLIGHT = new FontFamily(baseFont.ToString());
							break;
						case FontFamilyList.TimesNewRoman:
							_FONT_ALBERTUSMTLIGHT = new FontFamily(baseFont.AsString(EnumFormat.Description) ?? baseFont.ToString());
							break;
					}
				}

			}
			else
			{
				// Design time
				if (_FONT_ALBERTUSMTLIGHT is null)
				{
					try
					{
						_FONT_ALBERTUSMTLIGHT = new FontFamily(ALBERTUSMTLIGHT_NAME);
					}
					catch
					{ // fallback to Safe font
						_FONT_ALBERTUSMTLIGHT = new FontFamily("Arial");
					}
				}
			}

			// The font could not be created: let GDI+ pick a safe one rather than fail.
			return _FONT_ALBERTUSMTLIGHT ?? FontFamily.GenericSansSerif;
		}
	}

	public FontService(IAddFontToOS addFontToOS, UserSettings uSettings)
	{
		this.FontLoader = addFontToOS;
		this.USettings = uSettings;
	}

	/// <summary>GDI+ default character set, the one the <see cref="Font"/> constructors without <c>gdiCharSet</c> use.</summary>
	private const byte DefaultCharSet = 1;

	/// <summary>
	/// Fonts already created, shared by every caller: a <see cref="Font"/> is immutable, and creating one per paint leaks GDI handles
	/// until the finalizer runs. Callers must not dispose them.
	/// </summary>
	private readonly ConcurrentDictionary<(bool Light, float Size, FontStyle Style, GraphicsUnit Unit, byte CharSet), Font> _fonts = new();

	private Font Get(bool light, float fontSize, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point, byte charSet = DefaultCharSet)
		=> this._fonts.GetOrAdd((light, fontSize, fontStyle, unit, charSet), key => new Font(key.Light ? this.FONT_ALBERTUSMTLIGHT : this.FONT_ALBERTUSMT, key.Size, key.Style, key.Unit, key.CharSet));

	public Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit)
		=> this.Get(false, fontSize, fontStyle, unit);

	public Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		=> this.Get(false, fontSize, fontStyle, unit, b);

	public Font GetFont(float fontSize, GraphicsUnit unit)
		=> this.Get(false, fontSize, unit: unit);

	public Font GetFont(float fontSize, FontStyle fontStyle, float? scale = null)
		=> this.Get(false, fontSize * (scale ?? 1F), fontStyle);

	public Font GetFont(float fontSize, float? scale = null)
		=> this.Get(false, fontSize * (scale ?? 1F));

	public Font GetFontLight(float fontSize, GraphicsUnit unit)
		=> this.Get(false, fontSize, unit: unit);

	public Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit)
		=> this.Get(true, fontSize, fontStyle, unit);

	public Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		=> this.Get(true, fontSize, fontStyle, unit, b);

	public Font GetFontLight(float fontSize)
		=> this.Get(true, fontSize);

	public Font GetFontLight(float fontSize, float? scale = null)
		=> this.Get(true, fontSize * (scale ?? 1F));

	public Font GetFontLight(float fontSize, FontStyle fontStyle, float? scale = null)
		=> this.Get(true, fontSize * (scale ?? 1F), fontStyle);

	private readonly ConcurrentDictionary<(string Family, float Size, FontStyle Style, GraphicsUnit Unit, byte CharSet), Font> _styled = new();

	public Font WithStyle(Font font, FontStyle style)
	{
		ArgumentNullException.ThrowIfNull(font);
		if (font.Style == style)
			return font;

		return this._styled.GetOrAdd((font.FontFamily.Name, font.Size, style, font.Unit, font.GdiCharSet), _ => new Font(font, style));
	}
}