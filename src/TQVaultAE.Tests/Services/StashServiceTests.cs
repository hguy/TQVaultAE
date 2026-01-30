using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Config;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Unit tests for StashService class
/// </summary>
public class StashServiceTests
{
	private readonly Mock<ILogger<StashService>> _mockLogger;
	private readonly SessionContext _sessionContext;
	private readonly Mock<IStashProvider> _mockStashProvider;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<IGameFileService> _mockGameFileService;
	private readonly Mock<IItemProvider> _mockItemProvider;
	private readonly UserSettings _userSettings;
	private readonly StashService _stashService;

	/// <summary>
	/// Initializes test dependencies and StashService instance
	/// </summary>
	public StashServiceTests()
	{
		_mockLogger = new Mock<ILogger<StashService>>();
		_mockStashProvider = new Mock<IStashProvider>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockGameFileService = new Mock<IGameFileService>();
		_mockItemProvider = new Mock<IItemProvider>();
		_userSettings = new UserSettings();

		// Create real SessionContext with mocked dependencies
		_sessionContext = new SessionContext(_mockItemProvider.Object);

		_stashService = new StashService(
			_mockLogger.Object,
			_sessionContext,
			_mockStashProvider.Object,
			_mockGamePathService.Object,
			_mockGameFileService.Object,
			_userSettings
		);
	}

	/// <summary>
	/// Test LoadPlayerStash with null selectedSave returns empty result
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithNullSelectedSave_ReturnsEmptyResult()
	{
		// Arrange
		PlayerSave? nullSave = null;

		// Act
		var result = _stashService.LoadPlayerStash(nullSave);

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().BeNull();
		result.StashFile.Should().BeNull();
	}

	/// <summary>
	/// Test LoadPlayerStash with valid player save loads stash successfully
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithValidPlayerSave_LoadsStashSuccessfully()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = "C:\\Test\\Saves\\TestPlayer\\winsys.dxb";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("C:\\Test\\Saves\\_TestPlayer")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("C:\\Test\\Saves\\_TestPlayer", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadPlayerStash(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(stashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.PlayerName.Should().Be("TestPlayer - Immortal Throne");
	}

	/// <summary>
	/// Test LoadPlayerStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithLoadException_CapturesException()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = "C:\\Test\\Saves\\TestPlayer\\winsys.dxb";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("C:\\Test\\Saves\\_TestPlayer")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("C:\\Test\\Saves\\_TestPlayer", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);
		var expectedException = new ArgumentException("Invalid stash format");

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadPlayerStash(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test LoadPlayerStash with fromFileWatcher true updates existing stash
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithFromFileWatcherTrue_UpdatesExistingStash()
	{
		// Arrange
		var playerName = "TestPlayer";
		var stashFile = "C:\\Test\\Saves\\TestPlayer\\winsys.dxb";
		var mockPathIO = new Mock<IPathIO>();
		var mockTranslationService = new Mock<ITranslationService>();
		mockPathIO.Setup(x => x.GetFileName("C:\\Test\\Saves\\_TestPlayer")).Returns("_TestPlayer");
		var playerSave = new PlayerSave("C:\\Test\\Saves\\_TestPlayer", true, false, false, "", mockTranslationService.Object, mockPathIO.Object);

		_mockGamePathService.Setup(x => x.GetPlayerStashFile(playerName, false)).Returns(stashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// First load
		var result1 = _stashService.LoadPlayerStash(playerSave, false);
		var originalStash = result1.Stash;

		// Second load with fromFileWatcher
		var result2 = _stashService.LoadPlayerStash(playerSave, true);

		// Assert
		result2.Should().NotBeNull();
		result2.Stash.Should().NotBeSameAs(originalStash); // Should be updated
		result2.StashFile.Should().Be(stashFile);
	}

	/// <summary>
	/// Test LoadTransferStash loads transfer stash successfully
	/// </summary>
	[Fact]
	public void LoadTransferStash_LoadsSuccessfully()
	{
		// Arrange
		var transferStashFile = "C:\\Test\\Saves\\Sys\\winsys.dxb";

		_mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadTransferStash();

		// Assert
		result.Should().NotBeNull();
		result.TransferStashFile.Should().Be(transferStashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.IsImmortalThrone.Should().BeTrue();
	}

	/// <summary>
	/// Test LoadTransferStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadTransferStash_WithLoadException_CapturesException()
	{
		// Arrange
		var transferStashFile = "C:\\Test\\Saves\\Sys\\winsys.dxb";
		var expectedException = new ArgumentException("Invalid transfer stash format");

		_mockGamePathService.Setup(x => x.TransferStashFileFullPath).Returns(transferStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadTransferStash();

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test LoadRelicVaultStash loads relic vault stash successfully
	/// </summary>
	[Fact]
	public void LoadRelicVaultStash_LoadsSuccessfully()
	{
		// Arrange
		var relicVaultStashFile = "C:\\Test\\Saves\\Sys\\miscsys.dxb";

		_mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicVaultStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Returns(true);

		// Act
		var result = _stashService.LoadRelicVaultStash();

		// Assert
		result.Should().NotBeNull();
		result.RelicVaultStashFile.Should().Be(relicVaultStashFile);
		result.Stash.Should().NotBeNull();
		result.Stash.StashFound.Should().BeTrue();
		result.Stash.IsImmortalThrone.Should().BeTrue();
		result.Stash.Sack.StashType.Should().Be(SackType.RelicVaultStash);
	}

	/// <summary>
	/// Test LoadRelicVaultStash handles ArgumentException from StashProvider
	/// </summary>
	[Fact]
	public void LoadRelicVaultStash_WithLoadException_CapturesException()
	{
		// Arrange
		var relicVaultStashFile = "C:\\Test\\Saves\\Sys\\miscsys.dxb";
		var expectedException = new ArgumentException("Invalid relic vault format");

		_mockGamePathService.Setup(x => x.RelicVaultStashFileFullPath).Returns(relicVaultStashFile);
		_mockStashProvider.Setup(x => x.LoadFile(It.IsAny<Stash>())).Callback<Stash>(s => s.CreateEmptySack()).Throws(expectedException);

		// Act
		var result = _stashService.LoadRelicVaultStash();

		// Assert
		result.Should().NotBeNull();
		result.Stash.Should().NotBeNull();
		result.Stash.ArgumentException.Should().Be(expectedException);
		result.Stash.StashFound.Should().BeNull();
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with no modified stashes returns zero
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithNoModifiedStashes_ReturnsZero()
	{
		// Arrange
		var stash = new Stash("TestPlayer", "C:\\Test\\Saves\\TestPlayer\\winsys.dxb")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		Stash? stashOnError = null;

		// Act
		var result = _stashService.SaveAllModifiedStashes(ref stashOnError);

		// Assert
		result.Should().Be(0);
		stashOnError.Should().BeNull();
		_mockStashProvider.Verify(x => x.Save(It.IsAny<Stash>(), It.IsAny<string>()), Times.Never);
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with modified stashes saves successfully
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithModifiedStashes_SavesSuccessfully()
	{
		// Arrange
		var stash = new Stash("TestPlayer", "C:\\Test\\Saves\\TestPlayer\\winsys.dxb")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();
		stash.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		Stash? stashOnError = null;
		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = _stashService.SaveAllModifiedStashes(ref stashOnError);

			// Assert
			result.Should().Be(1);
			stashOnError.Should().Be(stash);
			_mockGameFileService.Verify(x => x.BackupFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			_mockStashProvider.Verify(x => x.Save(stash, "TestPlayer.dxb"), Times.Once);
			stash.IsModified.Should().BeFalse();
		}
		finally
		{
			_userSettings.DisableLegacyBackup = false;
		}
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with backup enabled calls backup and save
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithBackupEnabled_CallsBackupAndSave()
	{
		// Arrange
		var stash = new Stash("TestPlayer", "C:\\Test\\Saves\\TestPlayer\\winsys.dxb")
		{
			IsImmortalThrone = true
		};
		stash.CreateEmptySack();
		stash.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => stash);

		Stash? stashOnError = null;
		_userSettings.DisableLegacyBackup = false;

		// Act
		var result = _stashService.SaveAllModifiedStashes(ref stashOnError);

		// Assert
		result.Should().Be(1);
		stashOnError.Should().Be(stash);
		_mockGameFileService.Verify(x => x.BackupFile("TestPlayer - Immortal Throne", "TestPlayer.dxb"), Times.Once);
		_mockStashProvider.Verify(x => x.Save(stash, "TestPlayer.dxb"), Times.Once);
		stash.IsModified.Should().BeFalse();
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with multiple modified stashes saves all
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithMultipleModifiedStashes_SavesAll()
	{
		// Arrange
		var stash1 = new Stash("TestPlayer1", "C:\\Test\\Saves\\TestPlayer1\\winsys.dxb")
		{
			IsImmortalThrone = true
		};
		stash1.CreateEmptySack();
		stash1.Sack.IsModified = true;

		var stash2 = new Stash("TestPlayer2", "C:\\Test\\Saves\\TestPlayer2\\winsys.dxb")
		{
			IsImmortalThrone = true
		};
		stash2.CreateEmptySack();
		stash2.Sack.IsModified = true;

		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer1.dxb", _ => stash1);
		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer2.dxb", _ => stash2);

		Stash? stashOnError = null;
		_userSettings.DisableLegacyBackup = true;

		try
		{
			// Act
			var result = _stashService.SaveAllModifiedStashes(ref stashOnError);

			// Assert
			result.Should().Be(2);
			_mockStashProvider.Verify(x => x.Save(stash1, "TestPlayer1.dxb"), Times.Once);
			_mockStashProvider.Verify(x => x.Save(stash2, "TestPlayer2.dxb"), Times.Once);
			stash1.IsModified.Should().BeFalse();
			stash2.IsModified.Should().BeFalse();
		}
		finally
		{
			_userSettings.DisableLegacyBackup = false;
		}
	}

	/// <summary>
	/// Test SaveAllModifiedStashes with null stash in collection handles gracefully
	/// </summary>
	[Fact]
	public void SaveAllModifiedStashes_WithNullStash_HandlesGracefully()
	{
		// Arrange
		_sessionContext.Stashes.GetOrAddAtomic("TestPlayer.dxb", _ => null);

		Stash? stashOnError = null;

		// Act
		var result = _stashService.SaveAllModifiedStashes(ref stashOnError);

		// Assert
		result.Should().Be(0);
		stashOnError.Should().BeNull();
	}
}