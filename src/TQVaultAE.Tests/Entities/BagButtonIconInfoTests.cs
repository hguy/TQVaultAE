using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class BagButtonIconInfoTests
{
	private static BagButtonIconInfo CreateInfo() => new()
	{
		DisplayMode = BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label,
		Label = "Rings",
		OnStr = """Items\ring_on.tex""",
		OffStr = """Items\ring_off.tex""",
		OverStr = """Items\ring_over.tex""",
	};

	[Fact]
	public void Clone_CopiesEveryField()
	{
		var source = CreateInfo();

		var clone = source.Clone();

		// Compares field by field, so a field added later and left out of the copy fails here.
		clone.Should().BeEquivalentTo(source, options => options.IncludingFields());
	}

	[Fact]
	public void Clone_IsIndependentOfTheSource()
	{
		var source = CreateInfo();

		var clone = source.Clone();
		clone.Label = "Potions";
		clone.DisplayMode = BagButtonDisplayMode.Number;

		clone.Should().NotBeSameAs(source);
		source.Label.Should().Be("Rings");
		source.DisplayMode.Should().Be(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label);
	}
}
