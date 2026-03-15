using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
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
	private readonly Mock<IStashService> _mockStashService;
	private readonly Mock<FileIO> _mockFileIO;
	private readonly Mock<PathIO> _mockPathIO;
	private readonly SessionContext _sessionContext;
	private readonly UserSettings _userSettings;
	private readonly PlayerService _playerService;

	/// <summary>
	/// Helper to create a PlayerSave with mocked PathIO
	/// </summary>
	private PlayerSave CreatePlayerSave(string folder, bool isIT = false, bool isArchived = false, bool isCustom = false, string customMap = "")
	{
		var mockTranslationService = new Mock<ITranslationService>();

		// Get filename from path - extracts the folder name (e.g., "/Test/_PlayerA" -> "_PlayerA")
		var folderName = Path.GetFileName(folder);

		_mockPathIO.Setup(x => x.GetFileName(folder)).Returns(folderName);
		_mockPathIO.Setup(x => x.GetFileName(customMap)).Returns(string.IsNullOrEmpty(customMap) ? "" : Path.GetFileName(customMap));

		return new PlayerSave(folder, isIT, isArchived, isCustom, customMap, mockTranslationService.Object, _mockPathIO.Object);
	}

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
		_mockStashService = new Mock<IStashService>();
		_mockFileIO = new Mock<FileIO>();
		_mockPathIO = new Mock<PathIO>();
		_userSettings = new UserSettings();

		_sessionContext = new SessionContext(null);

		_playerService = new PlayerService(
			_mockLogger.Object,
			_sessionContext,
			_mockPlayerCollectionProvider.Object,
			_mockStashService.Object,
			_mockGameFileService.Object,
			_mockGamePathService.Object,
			_mockTranslationService.Object,
			_mockTQDataService.Object,
			_mockTagService.Object,
			_mockFileIO.Object,
			_mockPathIO.Object,
			_userSettings
		);
	}

	/// <summary>
	/// Test LoadPlayer method with null PlayerSave
	/// </summary>
	[Fact]
	public void LoadPlayer_WithNullPlayerSave_ReturnsEmptyResult()
	{
		// Act
		var result = _playerService.LoadPlayer(null);

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
		// Arrange - Use Unix paths for cross-platform compatibility
		var playerSave = CreatePlayerSave("/Test/_");

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
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = "/Test/Players/TestPlayer.plr";
		var playerSave = CreatePlayerSave(playerFolder);

		_mockGamePathService.Setup(x => x.GetPlayerFile("TestPlayer", false, false))
			.Returns(playerFile);
		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null));

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.PlayerFile.Should().Be(playerFile);
		_mockTagService.Verify(x => x.LoadTags(playerSave), Times.Once);
	}

	/// <summary>
	/// Test LoadPlayer method handles ArgumentException from LoadFile
	/// </summary>
	[Fact]
	public void LoadPlayer_WithLoadFileException_ReturnsResultWithException()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerFile = "/Test/Players/TestPlayer.plr";
		var playerSave = CreatePlayerSave(playerFolder);

		_mockGamePathService.Setup(x => x.GetPlayerFile("TestPlayer", false, false))
			.Returns(playerFile);

		_mockPlayerCollectionProvider.Setup(x => x.LoadFile(It.IsAny<PlayerCollection>(), null))
			.Callback<PlayerCollection, string>((pc, path) => pc.ArgumentException = new ArgumentException("Test error"));

		// Act
		var result = _playerService.LoadPlayer(playerSave);

		// Assert
		result.Should().NotBeNull();
		result.Player.Should().NotBeNull();
		result.Player.ArgumentException.Should().NotBeNull();
		result.Player.ArgumentException.Message.Should().Be("Test error");
	}

	/// <summary>
	/// Test GetPlayerSaveList returns sorted list
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithCharacters_ReturnsSortedList()
	{
		// Arrange - Use Unix paths for cross-platform compatibility
		var characterFolders = new[] {
			"/Test/_PlayerC",
			"/Test/_PlayerA",
			"/Test/_PlayerB"
		};

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		// Mock PathIO.GetFileName for each character folder
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerC")).Returns("_PlayerC");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerA")).Returns("_PlayerA");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerB")).Returns("_PlayerB");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

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
		// Arrange - Use Unix paths for cross-platform compatibility
		var characterFolders = new[] {
			"/Test/Titan Quest - Immortal Throne/TestPlayer"
		};

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("Titan Quest - Immortal Throne");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		_mockPathIO.Setup(x => x.GetFileName(It.IsAny<string>()))
			.Returns<string>(s => string.IsNullOrEmpty(s) ? "" : Path.GetFileName(s));

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

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().BeEmpty();
	}

	/// <summary>
	/// Test GetPlayerSaveListReadOnly returns same as GetPlayerSaveList
	/// </summary>
	[Fact]
	public void GetPlayerSaveListReadOnly_ReturnsSameAsGetPlayerSaveList()
	{
		// Arrange
		var characterFolders = new[] { "/Test/_PlayerA" };
		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");
		_mockPathIO.Setup(x => x.GetFileName("/Test/_PlayerA")).Returns("_PlayerA");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

		// Act
		var result = _playerService.GetPlayerSaveList();
		var resultReadOnly = _playerService.GetPlayerSaveListReadOnly();

		// Assert
		resultReadOnly.Should().HaveCount(result.Length);
		resultReadOnly.Should().BeEquivalentTo(result);
	}

	/// <summary>
	/// Test AlterNameInPlayerFileSave functionality
	/// </summary>
	[Fact]
	public void AlterNameInPlayerFileSave_WithValidData_CallsTQDataService()
	{
		// Arrange
		var newName = "NewPlayerName";
		var saveFolder = "/Test/SaveFolder";
		var playerFileName = "player.chr";
		var playerFile = Path.Combine(saveFolder, playerFileName);
		var originalContent = new byte[] { 0x01, 0x02, 0x03 };

		_mockGamePathService.Setup(x => x.PlayerSaveFileName).Returns(playerFileName);
		_mockPathIO.Setup(x => x.Combine(saveFolder, playerFileName)).Returns(playerFile);
		_mockFileIO.Setup(x => x.ReadAllBytes(playerFile)).Returns(originalContent);

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

	/// <summary>
	/// Test LoadPlayerStash returns result from StashService
	/// </summary>
	[Fact]
	public void LoadPlayerStash_WithValidPlayerSave_ReturnsStashResult()
	{
		// Arrange
		var playerSave = CreatePlayerSave("/Test/Players/_TestPlayer", isIT: true);
		var expectedResult = new StashLoadResult { StashFile = "/path/to/stash.dxt" };

		_mockStashService.Setup(x => x.LoadPlayerStash(playerSave, false))
			.Returns(expectedResult);

		// Act
		var result = _playerService.LoadPlayerStash(playerSave, false);

		// Assert
		result.Should().NotBeNull();
		result.StashFile.Should().Be(expectedResult.StashFile);
		_mockStashService.Verify(x => x.LoadPlayerStash(playerSave, false), Times.Once);
	}

	/// <summary>
	/// Test CreatePlayerFileWatcher with valid player - uses real temp directory
	/// </summary>
	[Fact]
	public void CreatePlayerFileWatcher_WithValidPlayer_CreatesWatcher()
	{
		// Arrange - Use real temp directory for FileSystemWatcher
		var tempDir = Path.Combine(Path.GetTempPath(), $"tqvault_test_{Guid.NewGuid()}");
		Directory.CreateDirectory(tempDir);
		var playerFile = Path.Combine(tempDir, "TestPlayer.plr");
		File.WriteAllText(playerFile, "test"); // Create the file
		var playerFolder = tempDir;
		var playerSave = CreatePlayerSave(playerFolder);

		_mockGamePathService.Setup(x => x.GetPlayerFile(It.IsAny<string>(), false, false))
			.Returns(playerFile);

		try
		{
			// Act
			var result = _playerService.CreatePlayerFileWatcher(playerSave, (ps, b) => { });

			// Assert
			result.Should().NotBeNull();
			result.Path.Should().Be(tempDir);
			result.Filter.Should().Be("TestPlayer.plr");
			result.EnableRaisingEvents.Should().BeTrue();
		}
		finally
		{
			// Cleanup
			if (Directory.Exists(tempDir))
				Directory.Delete(tempDir, true);
		}
	}

	/// <summary>
	/// Test CreatePlayerFileWatcher with invalid player file path
	/// </summary>
	[Fact]
	public void CreatePlayerFileWatcher_WithInvalidPath_ReturnsNull()
	{
		// Arrange
		var playerFolder = "/Test/Players/_TestPlayer";
		var playerSave = CreatePlayerSave(playerFolder);

		_mockGamePathService.Setup(x => x.GetPlayerFile("TestPlayer", false, false))
			.Returns("");

		// Act
		var result = _playerService.CreatePlayerFileWatcher(playerSave, (ps, b) => { });

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Test SaveAllModifiedPlayers with no modified players
	/// </summary>
	[Fact]
	public void SaveAllModifiedPlayers_WithNoModifiedPlayers_ReturnsFalse()
	{
		// Arrange
		PlayerCollection playerOnError = null;

		// Act
		var result = _playerService.SaveAllModifiedPlayers(ref playerOnError);

		// Assert
		result.Should().BeFalse();
		playerOnError.Should().BeNull();
	}

	/// <summary>
	/// Test GetPlayerSaveList with archived character
	/// </summary>
	[Fact]
	public void GetPlayerSaveList_WithArchivedCharacter_SetsIsArchived()
	{
		// Arrange
		var characterFolders = new[] { "/Test/ArchivedCharacters/_ArchivedPlayer" };

		_mockGamePathService.Setup(x => x.GetCharacterList()).Returns(characterFolders);
		_mockGamePathService.Setup(x => x.SaveDirNameTQIT).Returns("");
		_mockGamePathService.Setup(x => x.ArchiveDirName).Returns("ArchivedCharacters");
		_mockGamePathService.Setup(x => x.IsCustom).Returns(false);
		_mockGamePathService.Setup(x => x.MapName).Returns("");

		_mockPathIO.Setup(x => x.GetFileName("/Test/ArchivedCharacters/_ArchivedPlayer")).Returns("_ArchivedPlayer");
		_mockPathIO.Setup(x => x.GetFileName("")).Returns("");

		// Act
		var result = _playerService.GetPlayerSaveList();

		// Assert
		result.Should().HaveCount(1);
		result[0].IsArchived.Should().BeTrue();
	}
}
