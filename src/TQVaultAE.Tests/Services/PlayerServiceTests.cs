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
/// Unit tests for PlayerService class
/// </summary>
	public class PlayerServiceTests
	{
	private readonly Mock<ILogger<PlayerService>> _mockLogger;
	private readonly Mock<IPlayerCollectionProvider> _mockPlayerCollectionProvider;
	private readonly Mock<IGameFileService> _mockGameFileService;
	private readonly Mock<IGamePathService> _mockGamePathService;
	private readonly Mock<ITranslationService> _mockTranslationService;
	private readonly Mock<ITQDataService> _mockTQDataService;
	private readonly Mock<ITagService> _mockTagService;
	private readonly Mock<FileIO> _mockFileIO;
	private readonly PathIO _pathIO;
	private readonly SessionContext _sessionContext;
	private readonly UserSettings _userSettings;
	private readonly PlayerService _playerService;

	/// <summary>
	/// Initializes test dependencies and PlayerService instance
	/// </summary>
	public PlayerServiceTests()
	{
		_mockLogger = new Mock<ILogger<PlayerService>>();
		_mockPlayerCollectionProvider = new Mock<IPlayerCollectionProvider>();
		_mockGameFileService = new Mock<IGameFileService>();
		_mockGamePathService = new Mock<IGamePathService>();
		_mockTranslationService = new Mock<ITranslationService>();
		_mockTQDataService = new Mock<ITQDataService>();
		_mockTagService = new Mock<ITagService>();
		_mockFileIO = new Mock<FileIO>();
		_pathIO = new PathIO();
		_userSettings = new UserSettings();

		_sessionContext = new SessionContext(null);

		_playerService = new PlayerService(
			_mockLogger.Object,
			_sessionContext,
			_mockPlayerCollectionProvider.Object,
			null,
			_mockGameFileService.Object,
			_mockGamePathService.Object,
			_mockTranslationService.Object,
			_mockTQDataService.Object,
			_mockTagService.Object,
			_mockFileIO.Object,
			_pathIO,
			_userSettings
		);
		}

    /// <summary>
    /// Test LoadPlayer method with null PlayerSave
    /// </summary>
    [Fact]
    public void LoadPlayer_WithNullPlayerSave_ReturnsEmptyResult()
    {
        // Arrange
        PlayerSave nullPlayerSave = null;

        // Act
        var result = _playerService.LoadPlayer(nullPlayerSave);

        // Assert
        result.Should().NotBeNull();
        result.Player.Should().BeNull();
        result.PlayerFile.Should().BeNull();
    }

    /// <summary>
    /// Test LoadPlayer method with empty/whitespace PlayerSave name
    /// </summary>
    [Fact]
    public void LoadPlayer_WithEmptyPlayerSaveName_ReturnsEmptyResult()
    {
        // Arrange
        var mockTranslationService = new Mock<ITranslationService>();
        var playerSave = new PlayerSave("C:\\Test\\_", false, false, false, "", mockTranslationService.Object, _pathIO);

        // Act
        var result = _playerService.LoadPlayer(playerSave);

        // Assert
        result.Should().NotBeNull();
        result.Player.Should().BeNull();
        result.PlayerFile.Should().BeNull();
    }

    /// <summary>
    /// Test LoadPlayer method with valid PlayerSave
    /// </summary>
    [Fact]
    public void LoadPlayer_WithValidPlayerSave_LoadsSuccessfully()
    {
        // Arrange
        var playerFolder = "C:\\Test\\Players\\_TestPlayer";
        var playerFile = "C:\\Test\\Players\\TestPlayer.plr";
        
        var mockTranslationService = new Mock<ITranslationService>();
        var playerSave = new PlayerSave(playerFolder, false, false, false, "", mockTranslationService.Object, _pathIO);

        _mockGamePathService.Setup(x => x.GetPlayerFile("TestPlayer", false, false))
            .Returns(playerFile);
        
        _mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>()));

        // Act
        var result = _playerService.LoadPlayer(playerSave);

        // Assert
        result.Should().NotBeNull();
        result.PlayerFile.Should().Be(playerFile);
        _mockTagService.Verify(x => x.LoadTags(playerSave), Times.Once);
    }

    /// <summary>
    /// Test GetPlayerSaveList returns sorted list
    /// </summary>
    [Fact]
    public void GetPlayerSaveList_WithCharacters_ReturnsSortedList()
    {
        // Arrange
        var characterFolders = new[] { 
            "C:\\Test\\_PlayerC",
            "C:\\Test\\_PlayerA", 
            "C:\\Test\\_PlayerB"
        };

        _mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
        _mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
        _mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
        _mockGamePathService.Setup(x => x.IsCustom).Returns(false);
        _mockGamePathService.Setup(x => x.MapName).Returns("");

        // Act
        var result = _playerService.GetPlayerSaveList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("PlayerA"); // Should be sorted, Substring(1) removes underscore
        result[1].Name.Should().Be("PlayerB");
        result[2].Name.Should().Be("PlayerC");
    }

    /// <summary>
    /// Test GetPlayerSaveList identifies Immortal Throne characters
    /// </summary>
    [Fact]
    public void GetPlayerSaveList_WithTQITCharacter_SetsIsImmortalThrone()
    {
        // Arrange
        var characterFolders = new[] { 
            "C:\\Test\\Titan Quest - Immortal Throne\\TestPlayer"
        };

        _mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
        _mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
        _mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
        _mockGamePathService.Setup(x => x.IsCustom).Returns(false);
        _mockGamePathService.Setup(x => x.MapName).Returns("");

        // Act
        var result = _playerService.GetPlayerSaveList();

        // Assert
        result.Should().HaveCount(1);
        result[0].IsImmortalThrone.Should().BeTrue();
    }

    /// <summary>
    /// Test GetPlayerSaveList with empty character list
    /// </summary>
    [Fact]
    public void GetPlayerSaveList_WithEmptyList_ReturnsEmptyArray()
    {
        // Arrange
        var characterFolders = new string[0];

        _mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
        _mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
        _mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
        _mockGamePathService.Setup(x => x.IsCustom).Returns(false);
        _mockGamePathService.Setup(x => x.MapName).Returns("");

        // Act
        var result = _playerService.GetPlayerSaveList();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Test AlterNameInPlayerFileSave functionality
    /// </summary>
    [Fact]
    public void AlterNameInPlayerFileSave_WithValidData_CallsTQDataService()
    {
        // Arrange
        var newName = "NewPlayerName";
        var saveFolder = "C:\\Test\\SaveFolder";
        var playerFileName = "player.chr";
        var playerFile = System.IO.Path.Combine(saveFolder, playerFileName);
        var originalContent = new byte[] { 0x01, 0x02, 0x03 };
        var modifiedContent = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        _mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns(playerFileName);
        _mockFileIO.Setup(x => x.ReadAllBytes(playerFile)).Returns(originalContent);
        _mockFileIO.Setup(x => x.WriteAllBytes(playerFile, modifiedContent));

        // Act
        _playerService.AlterNameInPlayerFileSave(newName, saveFolder);

        // Assert
        _mockTQDataService.Verify(x => x.ReplaceUnicodeValueAfter(
            ref It.Ref<byte[]>.IsAny, 
            "myPlayerName", 
            newName, 
            0), Times.Once);
        _mockFileIO.Verify(x => x.ReadAllBytes(playerFile), Times.Once);
        _mockFileIO.Verify(x => x.WriteAllBytes(playerFile, It.IsAny<byte[]>()), Times.Once);
    }
}