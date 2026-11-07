using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class TranslationServiceTests
{
	private readonly Mock<IDatabase> _mockDatabase;
	private readonly TranslationService _service;

	public TranslationServiceTests()
	{
		_mockDatabase = new Mock<IDatabase>();
		_service = new TranslationService(_mockDatabase.Object);
	}

	[Fact]
	public void TryTranslateXTag_WithUnknownTag_ReturnsFalseAndTheTag()
	{
		// Act
		var found = _service.TryTranslateXTag("tagUnknownForTest", out var translation);

		// Assert - the tag stays visible in the UI, so a missing translation can be spotted
		found.Should().BeFalse();
		translation.Should().Be("tagUnknownForTest");
	}

	[Fact]
	public void TryTranslateXTag_WithTagInDatabase_ReturnsTheTranslation()
	{
		// Arrange
		_mockDatabase.Setup(d => d.GetFriendlyName("tagKnownForTest")).Returns("Known");

		// Act
		var found = _service.TryTranslateXTag("tagKnownForTest", out var translation);

		// Assert
		found.Should().BeTrue();
		translation.Should().Be("Known");
	}
}
