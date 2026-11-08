using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// Applies the character edit dialog to a character: level, experience, difficulty, attributes and masteries.
/// </summary>
public class CharacterEditService(IPathIO pathIO, IPlayerCollectionProvider playerCollectionProvider) : ICharacterEditService
{
	private readonly IPathIO _pathIO = pathIO;
	private readonly IPlayerCollectionProvider _playerCollectionProvider = playerCollectionProvider;

	/// <inheritdoc/>
	public PlayerInfo BuildPlayerInfo(PlayerInfo source, CharacterEditRequest request)
	{
		var playerInfo = new PlayerInfo(this._pathIO)
		{
			CurrentLevel = request.Level,
			MasteriesAllowed = PlayerLevel.GetMasteriesAllowed(request.Level),
			CurrentXP = int.TryParse(request.XpText, out var xp) ? xp : source.CurrentXP,
			Money = source.Money > 0 ? source.Money : 0,
			DifficultyUnlocked = request.DifficultyUnlocked ?? source.DifficultyUnlocked,
			SkillPoints = request.SkillPoints,
			AttributesPoints = request.AttributePoints,
			BaseStrength = request.Strength,
			BaseDexterity = request.Dexterity,
			BaseIntelligence = request.Intelligence,
			BaseHealth = request.Health,
			BaseMana = request.Mana,
		};
		playerInfo.SkillRecordList.AddRange(source.SkillRecordList);

		if (request.ResetAttributes)
		{
			playerInfo.BaseStrength = PlayerLevel.MinStrength;
			playerInfo.BaseDexterity = PlayerLevel.MinDexterity;
			playerInfo.BaseIntelligence = PlayerLevel.MinIntelligence;
			playerInfo.BaseHealth = PlayerLevel.MinHealth;
			playerInfo.BaseMana = PlayerLevel.MinMana;

			// Return invested points into attributes point pool
			playerInfo.AttributesPoints = source.AttributesPoints + PlayerLevel.GetInvestedAttributePoints(
				source.BaseStrength, source.BaseDexterity, source.BaseIntelligence, source.BaseHealth, source.BaseMana);
		}

		playerInfo.MasteriesResetRequiered = request.ResetMasteries;

		if (playerInfo.MustResetMasteries)
		{
			playerInfo.ResetMasteries();

			// Adjust "skillPoints"
			playerInfo.SkillPoints += playerInfo.ReleasedSkillPoints;
			playerInfo.Class = string.Empty;
		}

		playerInfo.Money = PlayerLevel.GetMoneyAfterDifficultyChange(playerInfo.Money, source.DifficultyUnlocked, playerInfo.DifficultyUnlocked);

		return playerInfo;
	}

	/// <inheritdoc/>
	public bool Apply(PlayerCollection player, CharacterEditRequest request)
	{
		if (player.PlayerInfo is null)
			return false;

		var playerInfo = this.BuildPlayerInfo(player.PlayerInfo, request);
		this._playerCollectionProvider.CommitPlayerInfo(player, playerInfo);
		return true;
	}
}
