using System.Drawing;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Config;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Tests.Win32.Presentation;

public class FontServiceTests
{
	// Arial: a system font, so no game font has to be installed.
	private readonly FontService _service = new(Mock.Of<IAddFontToOS>(), new UserSettings { BaseFont = "Arial" });

	[Fact]
	public void GetFont_SameArguments_ReturnsTheSharedInstance()
	{
		var first = this._service.GetFont(10F, FontStyle.Bold, 1.5F);

		this._service.GetFont(15F, FontStyle.Bold).Should().BeSameAs(first, "size times scale is the same font");
	}

	[Fact]
	public void GetFont_DifferentStyleUnitOrFamily_ReturnsDistinctFonts()
	{
		var regular = this._service.GetFont(10F, FontStyle.Regular, GraphicsUnit.Pixel);

		this._service.GetFont(10F, FontStyle.Bold, GraphicsUnit.Pixel).Should().NotBeSameAs(regular);
		this._service.GetFont(10F, FontStyle.Regular, GraphicsUnit.Point).Should().NotBeSameAs(regular);
		this._service.GetFontLight(10F, FontStyle.Regular, GraphicsUnit.Pixel).Should().NotBeSameAs(regular);
	}

	[Fact]
	public void GetFont_KeepsTheRequestedSizeStyleAndUnit()
	{
		var font = this._service.GetFontLight(12F, FontStyle.Italic, GraphicsUnit.Pixel);

		font.Size.Should().Be(12F);
		font.Style.Should().Be(FontStyle.Italic);
		font.Unit.Should().Be(GraphicsUnit.Pixel);
		font.FontFamily.Name.Should().Be("Arial");
	}

	[Fact]
	public void WithStyle_ReturnsOneSharedCopyInTheNewStyle_AndTheFontItselfForItsOwnStyle()
	{
		var regular = this._service.GetFontLight(20F, FontStyle.Regular, GraphicsUnit.Pixel);

		var bold = this._service.WithStyle(regular, FontStyle.Bold);

		bold.Style.Should().Be(FontStyle.Bold);
		bold.Size.Should().Be(20F);
		bold.Unit.Should().Be(GraphicsUnit.Pixel);
		this._service.WithStyle(regular, FontStyle.Bold).Should().BeSameAs(bold);
		this._service.WithStyle(regular, FontStyle.Regular).Should().BeSameAs(regular);
	}
}
