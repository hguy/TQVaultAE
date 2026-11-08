using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class PlayerInfoTextTests
{
	private const string ClassTag = "tagCClass01";

	private readonly Mock<ITranslationService> _translations = new();

	private readonly PlayerInfo _info = new(new Mock<IPathIO>().Object)
	{
		Class = ClassTag,
		CurrentLevel = 42,
		Money = 1234,
		DifficultyUnlocked = 1,
	};

	[Fact]
	public void BuildLines_TranslatesTheClassMasteryAndDifficulty()
	{
		this._translations.Setup(x => x.TranslateXTag(ClassTag, true, true, false)).Returns("Conqueror");
		this._translations.Setup(x => x.TranslateMastery(ClassTag)).Returns("Warfare / Defense");
		this._translations.Setup(x => x.TranslateDifficulty(1)).Returns("Epic");

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Class].Should().Be("Conqueror");
		lines[Resources.Masteries].Should().Be("Warfare / Defense");
		lines[Resources.DifficultyUnlocked].Should().Be("Epic");
		lines[Resources.CurrentLevel].Should().Be("42");
		lines[Resources.Money].Should().Be("1234");
	}

	[Fact]
	public void BuildLines_MasteryNotFound_ShowsNothing()
	{
		// Without a known mastery, the translation falls back to the "Masteries" label itself.
		this._translations.Setup(x => x.TranslateMastery(ClassTag)).Returns(Resources.Masteries);

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Masteries].Should().BeEmpty();
	}

	[Fact]
	public void BuildLines_UntranslatedValues_FallBackToEmptyAndUnknown()
	{
		this._translations.Setup(x => x.TranslateXTag(It.IsAny<string>(), true, true, false)).Returns((string)null!);
		this._translations.Setup(x => x.TranslateMastery(It.IsAny<string>())).Returns((string)null!);
		this._translations.Setup(x => x.TranslateDifficulty(It.IsAny<int>())).Returns((string)null!);

		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines[Resources.Class].Should().BeEmpty();
		lines[Resources.Masteries].Should().BeEmpty();
		lines[Resources.DifficultyUnlocked].Should().Be("unknown");
	}

	[Fact]
	public void BuildLines_StartsWithTheLevelAndClass()
	{
		var lines = PlayerInfoText.BuildLines(this._info, this._translations.Object);

		lines.Keys.Take(3).Should().Equal(Resources.CurrentLevel, Resources.Class, Resources.Masteries);
	}
}
