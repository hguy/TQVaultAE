using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class SaveOrchestratorTests
{
	// The services report the file in error through a ref parameter set before the write that throws.
	// A mocked ref parameter is only copied back when the call returns, so these fakes set it for real.

	/// <summary>
	/// Sets the player in error, then runs <see cref="Save"/>, which may throw.
	/// </summary>
	private sealed class PlayerSaveFake : IPlayerService
	{
		public Func<bool> Save { get; set; } = () => false;

		public bool SaveAllModifiedPlayers(ref PlayerCollection? playerOnError)
		{
			playerOnError = new PlayerCollection("Hero", "hero.chr");
			return this.Save();
		}

		public PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher = false) => throw new NotSupportedException();
		public StashLoadResult LoadPlayerStash(PlayerSave playerSave, bool fromFileWatcher = false) => throw new NotSupportedException();
		public PlayerSave[] GetPlayerSaveList() => throw new NotSupportedException();
		public IReadOnlyList<PlayerSave> GetPlayerSaveListReadOnly() => throw new NotSupportedException();
		public void AlterNameInPlayerFileSave(string newname, string saveFolder) => throw new NotSupportedException();
		public CharacterNameError ValidateNewCharacterName(PlayerSave source, string newName, IEnumerable<string> existingNames) => throw new NotSupportedException();
		public CharacterDuplicateResult DuplicateCharacter(PlayerSave source, string newName) => throw new NotSupportedException();
		public FileSystemWatcher? CreatePlayerFileWatcher(PlayerSave playerSave, Action<PlayerSave, bool> onChanged) => throw new NotSupportedException();
		public void UpdatePlayerFilePath(string oldPath, string newPath) => throw new NotSupportedException();
		public bool Archive(PlayerSave ps) => throw new NotSupportedException();
		public bool Unarchive(PlayerSave ps) => throw new NotSupportedException();
		public bool CurrentPlayerMeetsRequirements(Item item) => throw new NotSupportedException();
		public bool IsSuitableForCurrentPlayer(Item item) => throw new NotSupportedException();
		public void UpdateGearStatBonuses(PlayerCollection player, IEnumerable<Item> equipment, Item? ignored = null) => throw new NotSupportedException();
		public void UpdateSkillStatBonuses(PlayerCollection player) => throw new NotSupportedException();
	}

	/// <summary>
	/// Sets the vault in error, then runs <see cref="Save"/>, which may throw.
	/// </summary>
	private sealed class VaultSaveFake : IVaultService
	{
		public Func<int> Save { get; set; } = () => 0;

		public int SaveAllModifiedVaults(ref PlayerCollection? vaultOnError)
		{
			vaultOnError = new PlayerCollection("Secondary Vault", "secondary.vault");
			return this.Save();
		}

		public PlayerCollection CreateVault(string name, string file) => throw new NotSupportedException();
		public PlayerCollection CreateVault(string name) => throw new NotSupportedException();
		public VaultLoadResult LoadVault(string vaultName) => throw new NotSupportedException();
		public IReadOnlyList<string> BuildVaultChoices(IEnumerable<string> vaultNames, string? leadingEntry = null, string? excludedVault = null, bool alwaysIncludeMainVault = true) => throw new NotSupportedException();
		public BagTransferCheck CheckBagTransfer(PlayerCollection vault, int source, int destination) => throw new NotSupportedException();
		public string BuildCsvExport(PlayerCollection vault, int? bagIndex, char delimiter) => throw new NotSupportedException();
		public VaultMaintenanceError ValidateMaintenance(VaultMaintenance action, string? source, string? target, IEnumerable<string> existingVaults) => throw new NotSupportedException();
		public bool CopyVault(string sourceName, string targetName) => throw new NotSupportedException();
		public bool DeleteVault(string vaultName) => throw new NotSupportedException();
		public bool RenameVault(string oldName, string newName) => throw new NotSupportedException();
		public void UpdateVaultPath(string vaultPath) => throw new NotSupportedException();
		public void SaveVault(PlayerCollection vault, string vaultName) => throw new NotSupportedException();
	}

	/// <summary>
	/// Sets the stash in error, then runs <see cref="Save"/>, which may throw.
	/// </summary>
	private sealed class StashSaveFake : IStashService
	{
		public Func<int> Save { get; set; } = () => 0;

		public int SaveAllModifiedStashes(ref Stash? stashOnError)
		{
			stashOnError = new Stash("Hero", "winsys.dxb");
			return this.Save();
		}

		public StashLoadResult LoadPlayerStash(PlayerSave? selectedSave, bool fromFileWatcher = false) => throw new NotSupportedException();
		public StashLoadResult LoadRelicVaultStash(bool fromFileWatcher = false) => throw new NotSupportedException();
		public StashLoadResult LoadTransferStash(bool fromFileWatcher = false) => throw new NotSupportedException();
		public FileSystemWatcher? CreateRelicStashFileWatcher(Action onChanged) => throw new NotSupportedException();
		public FileSystemWatcher? CreateTransferStashFileWatcher(Action onChanged) => throw new NotSupportedException();
		public int ClampBagId(int bagId) => throw new NotSupportedException();
		public SackCollection? GetBagSack(int bagId, PlayerCollection? player, Stash? transferStash, Stash? playerStash, Stash? relicVaultStash) => throw new NotSupportedException();
	}

	private readonly PlayerSaveFake _playerService = new();
	private readonly VaultSaveFake _vaultService = new();
	private readonly StashSaveFake _stashService = new();
	private readonly Mock<ISettingsPersistenceService> _settingsPersistence = new(MockBehavior.Strict);
	private readonly Mock<IGameFileService> _gameFileService = new(MockBehavior.Strict);
	private readonly UserSettings _settings = new() { GitBackupEnabled = true };
	private readonly SaveOrchestrator _orchestrator;

	/// <summary>
	/// The calls to the services, in order.
	/// </summary>
	private readonly List<string> _calls = [];

	private readonly List<SaveError> _asked = [];

	private static readonly SessionConfiguration Configuration = new("Main Vault", "Hero", 1.5F, true);

	public SaveOrchestratorTests()
	{
		this.SetupPlayers(_ => true);
		this.SetupVaults(_ => 2);
		this.SetupStashes(_ => 1);
		this._settingsPersistence
			.Setup(s => s.SaveConfiguration(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<float>(), It.IsAny<bool>()))
			.Callback(() => this._calls.Add("configuration"))
			.Returns(false);
		this._gameFileService
			.Setup(g => g.GitAddCommitTagAndPush())
			.Callback(() => this._calls.Add("git"))
			.Returns(true);

		this._orchestrator = new SaveOrchestrator(
			this._playerService
			, this._vaultService
			, this._stashService
			, this._settingsPersistence.Object
			, this._gameFileService.Object
			, this._settings);
	}

	/// <summary>
	/// The player save runs <paramref name="attempt"/> with the attempt number (1 based); it may throw.
	/// </summary>
	private void SetupPlayers(Func<int, bool> attempt)
	{
		int count = 0;
		this._playerService.Save = () =>
		{
			this._calls.Add("players");
			return attempt(++count);
		};
	}

	private void SetupVaults(Func<int, int> attempt)
	{
		int count = 0;
		this._vaultService.Save = () =>
		{
			this._calls.Add("vaults");
			return attempt(++count);
		};
	}

	private void SetupStashes(Func<int, int> attempt)
	{
		int count = 0;
		this._stashService.Save = () =>
		{
			this._calls.Add("stashes");
			return attempt(++count);
		};
	}

	private Func<SaveError, SaveDecision> Answer(params SaveDecision[] decisions)
	{
		var queue = new Queue<SaveDecision>(decisions);
		return error =>
		{
			this._asked.Add(error);
			return queue.Dequeue();
		};
	}

	private static Func<SaveError, SaveDecision> NeverAsked
		=> _ => throw new InvalidOperationException("No file error was expected");

	[Fact]
	public void Save_OnClose_SavesPlayersVaultsStashesThenConfigurationThenGit()
	{
		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), NeverAsked);

		this._calls.Should().Equal("players", "vaults", "stashes", "configuration", "git");
		result.PlayersSaved.Should().BeTrue();
		result.VaultsSaved.Should().Be(2);
		result.StashesSaved.Should().Be(1);
		result.GitPushed.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Save_PassesSessionStateToConfigurationAndReturnsItsChangeFlag()
	{
		this._settingsPersistence
			.Setup(s => s.SaveConfiguration("Main Vault", "Hero", 1.5F, true))
			.Returns(true);

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration), NeverAsked);

		result.ConfigChanged.Should().BeTrue();
	}

	[Fact]
	public void Save_FilesOnly_DoesNotTouchConfigurationNorGit()
	{
		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		this._calls.Should().Equal("players", "vaults", "stashes");
		result.ConfigChanged.Should().BeNull();
		result.GitPushed.Should().BeFalse();
	}

	[Fact]
	public void Save_FilesRefused_StillSavesConfigurationAndGit()
	{
		this._orchestrator.Save(new SaveRequest(false, Configuration, true), NeverAsked);

		this._calls.Should().Equal("configuration", "git");
	}

	[Fact]
	public void Save_GitBackupDisabled_DoesNotPush()
	{
		this._settings.GitBackupEnabled = false;

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), NeverAsked);

		this._gameFileService.Verify(g => g.GitAddCommitTagAndPush(), Times.Never);
		result.GitPushed.Should().BeFalse();
	}

	[Fact]
	public void Save_NothingModified_ReportsNothingSaved()
	{
		this.SetupPlayers(_ => false);
		this.SetupVaults(_ => 0);
		this.SetupStashes(_ => 0);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		result.AnythingSaved.Should().BeFalse();
	}

	[Fact]
	public void Save_OnlyAStashSaved_ReportsSomethingSavedButNoPlayers()
	{
		this.SetupPlayers(_ => false);
		this.SetupVaults(_ => 0);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		result.AnythingSaved.Should().BeTrue();
		result.PlayersSaved.Should().BeFalse();
	}

	[Fact]
	public void Save_PlayerFileLocked_AsksWithThePlayerNameAndStep()
	{
		var locked = new IOException("locked");
		this.SetupPlayers(n => n == 1 ? throw locked : true);

		this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry));

		this._asked.Should().ContainSingle();
		this._asked[0].Step.Should().Be(SaveStep.Players);
		this._asked[0].Name.Should().Be("Hero");
		this._asked[0].Exception.Should().BeSameAs(locked);
	}

	[Fact]
	public void Save_Retry_RunsOnlyTheFailedStepAgain()
	{
		this.SetupVaults(n => n == 1 ? throw new IOException("locked") : 1);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry));

		this._calls.Should().Equal("players", "vaults", "vaults", "stashes");
		result.VaultsSaved.Should().Be(1);
		result.Errors.Should().ContainSingle().Which.Step.Should().Be(SaveStep.Vaults);
	}

	[Fact]
	public void Save_RetryFailingAgain_AsksAgainUntilIgnored()
	{
		this.SetupPlayers(_ => throw new IOException("locked"));

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Retry, SaveDecision.Retry, SaveDecision.Ignore));

		this._calls.Should().Equal("players", "players", "players", "vaults", "stashes");
		result.Errors.Should().HaveCount(3);
	}

	[Fact]
	public void Save_IgnorePlayers_GivesUpPlayersAndContinuesWithTheNextSteps()
	{
		this.SetupPlayers(_ => throw new IOException("locked"));

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Ignore));

		this._calls.Should().Equal("players", "vaults", "stashes", "configuration", "git");
		result.PlayersSaved.Should().BeFalse();
		result.VaultsSaved.Should().Be(2);
		result.AnythingSaved.Should().BeTrue();
	}

	[Fact]
	public void Save_IgnoreVaults_CountsNoVaultSaved()
	{
		this.SetupPlayers(_ => false);
		this.SetupVaults(_ => throw new IOException("locked"));
		this.SetupStashes(_ => 0);

		var result = this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Ignore));

		result.VaultsSaved.Should().Be(0);
		result.AnythingSaved.Should().BeFalse();
	}

	[Fact]
	public void Save_Abort_RethrowsTheFileErrorAndRunsNoLaterStep()
	{
		var locked = new IOException("locked");
		this.SetupPlayers(_ => throw locked);

		var save = () => this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Abort));

		save.Should().Throw<IOException>().Which.Should().BeSameAs(locked);
		this._calls.Should().Equal("players");
	}

	[Fact]
	public void Save_VaultFileLocked_ReportsTheVaultName()
	{
		this.SetupVaults(n => n == 1 ? throw new IOException("locked") : 0);

		this._orchestrator.Save(SaveRequest.FilesOnly, this.Answer(SaveDecision.Ignore));

		this._asked.Should().ContainSingle();
		this._asked[0].Step.Should().Be(SaveStep.Vaults);
		this._asked[0].Name.Should().Be("Secondary Vault");
	}

	[Fact]
	public void Save_StashFileLocked_ReportsTheStashOwnerAndStillPushesGit()
	{
		this.SetupStashes(_ => throw new IOException("locked"));

		var result = this._orchestrator.Save(new SaveRequest(true, Configuration, true), this.Answer(SaveDecision.Ignore));

		this._asked.Should().ContainSingle();
		this._asked[0].Step.Should().Be(SaveStep.Stashes);
		this._asked[0].Name.Should().Be("Hero - Immortal Throne");
		result.StashesSaved.Should().Be(0);
		this._calls.Should().EndWith("git");
	}

	[Fact]
	public void Save_ErrorOtherThanFileError_IsNotCaught()
	{
		this.SetupPlayers(_ => throw new InvalidOperationException("bug"));

		var save = () => this._orchestrator.Save(SaveRequest.FilesOnly, NeverAsked);

		save.Should().Throw<InvalidOperationException>();
	}
}
