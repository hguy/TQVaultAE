using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	/// <summary>Width of the word wrapped skill description, in characters.</summary>
	private const int GrantedSkillDescriptionWidth = 40;

	/// <summary>
	/// Gets the skills granted by an item: by its base record, its affixes, its relics and their completion bonuses
	/// (<c>itemSkillName</c> at <c>itemSkillLevel</c>).
	/// </summary>
	/// <remarks>Must run inside <see cref="GetFriendlyNames"/>, which sets <see cref="Item.CurrentFriendlyNameResult"/>.</remarks>
	private GrantedSkill[] GetGrantedSkills(Item itm)
	{
		var skills = new List<GrantedSkill>();
		foreach (var (source, skillId) in this.GrantedSkillSources(itm))
		{
			int level = Math.Max(source.GetInt32("itemSkillLevel", 0), 1);
			if (this.GetGrantedSkill(itm, skillId, level, source.GetString("itemSkillAutoController", 0)) is { } skill)
				skills.Add(skill);
		}

		return skills.ToArray();
	}

	/// <inheritdoc/>
	public bool HasGrantedSkill(Item itm)
		=> this.GrantedSkillSources(itm).Any(s => this.DisplayedSkillRecord(s.SkillId) is not null);

	/// <summary>
	/// The records of an item that grant a skill (<c>itemSkillName</c>): its base, its affixes, its relics and their
	/// completion bonuses, each once.
	/// </summary>
	private IEnumerable<(DBRecordCollection Source, RecordId SkillId)> GrantedSkillSources(Item itm)
	{
		RecordId?[] sources = [itm.BaseItemId, itm.prefixID, itm.suffixID, itm.relicID, itm.RelicBonusId, itm.relic2ID, itm.RelicBonus2Id];

		var seen = new HashSet<RecordId>();
		foreach (var sourceId in sources)
		{
			if (RecordId.IsNullOrEmpty(sourceId) || !seen.Add(sourceId))
				continue;

			if (Database.GetRecordFromFile(sourceId) is not { } source)
				continue;

			string skillName = source.GetString("itemSkillName", 0);
			if (!string.IsNullOrEmpty(skillName))
				yield return (source, skillName.ToRecordId());
		}
	}

	/// <summary>
	/// The record that names a granted skill: its buff record (<c>buffSkillName</c>) when it has one, else the skill itself.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private DBRecordCollection? DisplayedSkillRecord(RecordId skillId)
	{
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

		var buffId = skillRecord.GetString("buffSkillName", 0).ToRecordId();
		var shown = (RecordId.IsNullOrEmpty(buffId) ? null : Database.GetRecordFromFile(buffId)) ?? skillRecord;
		return string.IsNullOrEmpty(shown.GetString("skillDisplayName", 0)) ? null : shown;
	}

	/// <summary>
	/// Describes one granted skill at a level: name, icon, activation, description and effects.
	/// A buff skill (<c>buffSkillName</c>) takes its name, icon, description and effects from its buff record.
	/// </summary>
	/// <returns><see langword="null"/> when the skill record is missing or has no name, like the game's hidden helper skills.</returns>
	private GrantedSkill? GetGrantedSkill(Item itm, RecordId skillId, int level, string? autoController)
	{
		if (Database.GetRecordFromFile(skillId) is not { } skillRecord)
			return null;

		var buffId = skillRecord.GetString("buffSkillName", 0).ToRecordId();
		var buffRecord = RecordId.IsNullOrEmpty(buffId) ? null : Database.GetRecordFromFile(buffId);
		var shown = buffRecord ?? skillRecord;

		string nameTag = shown.GetString("skillDisplayName", 0);
		if (string.IsNullOrEmpty(nameTag))
			return null;

		if (!TranslationService.TryTranslateXTag(nameTag, out var name))
			name = PathIO.GetFileNameWithoutExtension(skillId.Raw);

		string icon = shown.GetString("skillUpBitmapName", 0);
		if (string.IsNullOrEmpty(icon))
			icon = skillRecord.GetString("skillUpBitmapName", 0);

		string[] description = [];
		string descriptionTag = shown.GetString("skillBaseDescription", 0);
		if (!string.IsNullOrEmpty(descriptionTag) && TranslationService.TryTranslateXTag(descriptionTag, out var descriptionText))
			description = StringHelper.WrapWords(descriptionText, GrantedSkillDescriptionWidth).ToArray();

		if (!TranslationService.TryTranslateXTag("MenuLevel", out var levelFormat))
			levelFormat = "Level:   {0}";
		else
			levelFormat = ItemAttributeProvider.ConvertFormat(levelFormat);

		var activation = this.GetSkillActivationText(autoController);

		// Effects at the granted level: the skill's own parameters (energy cost, cooldown, radius...), then the buff's.
		var attributes = new List<string>();
		if (skillRecord.GetString("Class", 0).Equals("SKILL_SPAWNPET", noCase))
			this.ConvertPetStats(itm, skillRecord, attributes);
		else
		{
			this.GetAttributesFromRecord(itm, skillRecord, true, skillId, attributes, skillLevelIndex: level - 1);
			if (buffRecord is not null)
				this.GetAttributesFromRecord(itm, buffRecord, true, buffId, attributes, skillLevelIndex: level - 1);
		}

		return new GrantedSkill(
			skillId,
			name,
			level,
			Format(levelFormat, level),
			string.IsNullOrEmpty(icon) ? null : icon.ToRecordId(),
			string.IsNullOrEmpty(activation) ? null : activation,
			description,
			TrimEmptyLines(attributes)
		);
	}

	/// <summary>Drops the blank lines at both ends; the ones between groups stay as separators.</summary>
	private static string[] TrimEmptyLines(List<string> lines)
	{
		int start = lines.FindIndex(l => !string.IsNullOrWhiteSpace(l));
		if (start < 0)
			return [];

		int end = lines.FindLastIndex(l => !string.IsNullOrWhiteSpace(l));
		return lines.GetRange(start, end - start + 1).ToArray();
	}
}
