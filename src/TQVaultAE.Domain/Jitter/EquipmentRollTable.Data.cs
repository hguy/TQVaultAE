//-----------------------------------------------------------------------
// <copyright file="EquipmentRollTable.Data.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

// Generated from Game.dll (SHA-256 754907eacf552656945ff9eaf1763630e138506517e91b698ab28a0c3186aa86) with Ghidra 12.1.4.
// Per-class rolled fields, in engine order:
//   CharAttribute / SkillAttribute: the single value (affix via LoadPrefixTable/LoadSuffixTable, base via AddJitter 0x100f9130).
//   DamageAttributeAbs (0x10155550) / DamageAttributeDur (0x10156dc0): per level (min, max - min); Dur durations never roll.
//   DamageAttributeAbsMod (0x10158620): modifier. DamageAttributeDurMod (0x10159230): modifier, then duration modifier.
//   DamageAttribute_Physical (0x10161ff0): (min, uint(max - min)) pairs, then offensivePierceRatio.
//   DefenseAttribute_Typical (0x10166270): the value only (the *Chance variable is never rolled).

public static partial class EquipmentRollTable
{
	private static partial RollAttribute[] CreateAttributes() => [
		// CharAttributeVal_BaseAttackSpeed: LoadPrefix/SuffixTable and AddJitter are all no-ops (0x100fd490/0x100fd4a0/0x100fd4b0). Only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Character, 1, "CharAttributeVal_BaseAttackSpeed", 0f, RollScope.None, [new("characterBaseAttackSpeed")], Items: RollItemFilter.NonArmorOnly, Scales: false), // FUN_10021cf0 @ 0x10021cf0 (vtable 0x1037318c)
		new(RollStore.Character, 2, "CharAttribute@0x10373160", 20f, RollScope.All, [new("characterStrength")]), // FUN_1001ee50 @ 0x1001ee50 (Ghidra vtable label CharAttributeValPri, COMDAT-folded)
		new(RollStore.Character, 3, "CharAttribute@0x10373000", 20f, RollScope.All, [new("characterStrengthModifier")]), // FUN_10021d70 @ 0x10021d70 (Ghidra vtable label CharAttributeMod_Dexterity, COMDAT-folded)
		new(RollStore.Character, 4, "CharAttribute@0x10373160", 20f, RollScope.All, [new("characterIntelligence")]), // FUN_1001ef40 @ 0x1001ef40 (Ghidra vtable label CharAttributeValPri, COMDAT-folded)
		new(RollStore.Character, 5, "CharAttribute@0x10373000", 20f, RollScope.All, [new("characterIntelligenceModifier")]), // FUN_10021e60 @ 0x10021e60 (Ghidra vtable label CharAttributeMod_Dexterity, COMDAT-folded)
		new(RollStore.Character, 6, "CharAttribute@0x10373160", 20f, RollScope.All, [new("characterDexterity")]), // FUN_1001f030 @ 0x1001f030 (Ghidra vtable label CharAttributeValPri, COMDAT-folded)
		new(RollStore.Character, 7, "CharAttribute@0x10373000", 20f, RollScope.All, [new("characterDexterityModifier")]), // FUN_10021f50 @ 0x10021f50 (Ghidra vtable label CharAttributeMod_Dexterity, COMDAT-folded)
		new(RollStore.Character, 8, "CharAttribute@0x10373134", 0f, RollScope.All, [new("characterLife")]), // FUN_1001f120 @ 0x1001f120 (Ghidra vtable label CharAttributeValSec, COMDAT-folded)
		new(RollStore.Character, 9, "CharAttribute@0x10372fd4", 20f, RollScope.All, [new("characterLifeModifier")]), // FUN_10022040 @ 0x10022040 (Ghidra vtable label CharAttributeMod_Life, COMDAT-folded)
		new(RollStore.Character, 10, "CharAttribute@0x10373160", 0f, RollScope.All, [new("characterLifeRegen")]), // FUN_1001f210 @ 0x1001f210 (Ghidra vtable label CharAttributeValPri, COMDAT-folded)
		new(RollStore.Character, 11, "CharAttribute@0x10373000", 20f, RollScope.All, [new("characterLifeRegenModifier")]), // FUN_10022130 @ 0x10022130 (Ghidra vtable label CharAttributeMod_Dexterity, COMDAT-folded)
		new(RollStore.Character, 12, "CharAttribute@0x10373134", 20f, RollScope.All, [new("characterMana")]), // FUN_1001f300 @ 0x1001f300 (Ghidra vtable label CharAttributeValSec, COMDAT-folded)
		new(RollStore.Character, 13, "CharAttribute@0x10372fd4", 20f, RollScope.All, [new("characterManaModifier")]), // FUN_10022220 @ 0x10022220 (Ghidra vtable label CharAttributeMod_Life, COMDAT-folded)
		new(RollStore.Character, 14, "CharAttribute@0x10373160", 0f, RollScope.All, [new("characterManaRegen")]), // FUN_1001f3f0 @ 0x1001f3f0 (Ghidra vtable label CharAttributeValPri, COMDAT-folded)
		new(RollStore.Character, 15, "CharAttribute@0x10373000", 20f, RollScope.All, [new("characterManaRegenModifier")]), // FUN_10022310 @ 0x10022310 (Ghidra vtable label CharAttributeMod_Dexterity, COMDAT-folded)
		new(RollStore.Character, 16, "CharAttribute@0x10373108", 20f, RollScope.All, [new("characterManaLimitReserve")]), // FUN_1001f990 @ 0x1001f990 (Ghidra vtable label CharAttributeVal_ManaLimitReserve, COMDAT-folded)
		new(RollStore.Character, 17, "CharAttribute@0x10372fa8", 20f, RollScope.All, [new("characterManaLimitReserveModifier")]), // FUN_1001fa80 @ 0x1001fa80 (Ghidra vtable label CharAttributeMod, COMDAT-folded)
		new(RollStore.Character, 18, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterManaLimitReserveReduction")]), // FUN_1001fb70 @ 0x1001fb70 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 19, "CharAttribute@0x10372fa8", 20f, RollScope.All, [new("characterManaLimitReserveReductionModifier")]), // FUN_1001fc60 @ 0x1001fc60 (Ghidra vtable label CharAttributeMod, COMDAT-folded)
		new(RollStore.Character, 20, "CharAttribute@0x10373134", 20f, RollScope.All, [new("characterOffensiveAbility")]), // FUN_1001f7b0 @ 0x1001f7b0 (Ghidra vtable label CharAttributeValSec, COMDAT-folded)
		new(RollStore.Character, 21, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterOffensiveAbilityModifier")]), // FUN_1001fef0 @ 0x1001fef0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 22, "CharAttribute@0x10373134", 20f, RollScope.All, [new("characterDefensiveAbility")]), // FUN_1001f8a0 @ 0x1001f8a0 (Ghidra vtable label CharAttributeValSec, COMDAT-folded)
		new(RollStore.Character, 23, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterDefensiveAbilityModifier")]), // FUN_1001ffe0 @ 0x1001ffe0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 24, "CharAttribute@0x10372f7c", 20f, RollScope.All, [new("characterTotalSpeedModifier")]), // FUN_100200d0 @ 0x100200d0 (Ghidra vtable label CharAttributeMod_TotalSpeed, COMDAT-folded)
		// AddJitter is a no-op (0x100f8b50) but LoadPrefixTable/LoadSuffixTable are CharAttribute's, so affix values still roll; base percent is 0 anyway.
		new(RollStore.Character, 25, "CharAttribute@0x10373058", 0f, RollScope.All, [new("characterAttackSpeed")], Scales: false), // FUN_1001f4e0 @ 0x1001f4e0 (Ghidra vtable label CharAttributeAttackSpeed, COMDAT-folded)
		new(RollStore.Character, 26, "CharAttribute@0x10372f24", 20f, RollScope.All, [new("characterAttackSpeedModifier")]), // FUN_10022400 @ 0x10022400 (Ghidra vtable label CharAttributeMod_AttackSpeed, COMDAT-folded)
		// AddJitter is a no-op (0x100f8b50) but LoadPrefixTable/LoadSuffixTable are CharAttribute's, so affix values still roll; base percent is 0 anyway.
		new(RollStore.Character, 27, "CharAttribute@0x10373058", 0f, RollScope.All, [new("characterSpellCastSpeed")], Scales: false), // FUN_1001f5d0 @ 0x1001f5d0 (Ghidra vtable label CharAttributeAttackSpeed, COMDAT-folded)
		new(RollStore.Character, 28, "CharAttribute@0x10372ef8", 20f, RollScope.All, [new("characterSpellCastSpeedModifier")]), // FUN_100224f0 @ 0x100224f0 (Ghidra vtable label CharAttributeAttackSpeedMod, COMDAT-folded)
		// AddJitter is a no-op (0x100f8b50) but LoadPrefixTable/LoadSuffixTable are CharAttribute's, so affix values still roll; base percent is 0 anyway.
		new(RollStore.Character, 29, "CharAttribute@0x10373084", 0f, RollScope.All, [new("characterRunSpeed")], Scales: false), // FUN_1001f6c0 @ 0x1001f6c0 (Ghidra vtable label CharAttributeOtherSpeed, COMDAT-folded)
		new(RollStore.Character, 30, "CharAttribute@0x10372f50", 20f, RollScope.All, [new("characterRunSpeedModifier")]), // FUN_100225e0 @ 0x100225e0 (Ghidra vtable label CharAttributeMod_RunSpeed, COMDAT-folded)
		new(RollStore.Character, 31, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterDefensiveBlockRecoveryReduction")]), // FUN_100201c0 @ 0x100201c0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 32, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterEnergyAbsorptionPercent")]), // FUN_100202b0 @ 0x100202b0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 33, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterDodgePercent")]), // FUN_100203a0 @ 0x100203a0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 34, "CharAttribute@0x103730dc", 20f, RollScope.All, [new("characterDeflectProjectile")]), // FUN_10020490 @ 0x10020490 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 35, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterGlobalReqReduction")]), // FUN_10020580 @ 0x10020580 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 36, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterWeaponStrengthReqReduction")]), // FUN_10020670 @ 0x10020670 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 37, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterWeaponDexterityReqReduction")]), // FUN_10020760 @ 0x10020760 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 38, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterWeaponIntelligenceReqReduction")]), // FUN_10020850 @ 0x10020850 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 39, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterMeleeStrengthReqReduction")]), // FUN_10020940 @ 0x10020940 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 40, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterMeleeDexterityReqReduction")]), // FUN_10020a30 @ 0x10020a30 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 41, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterMeleeIntelligenceReqReduction")]), // FUN_10020b20 @ 0x10020b20 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 42, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterArmorStrengthReqReduction")]), // FUN_10020c10 @ 0x10020c10 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 43, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterArmorDexterityReqReduction")]), // FUN_10020d00 @ 0x10020d00 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 44, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterArmorIntelligenceReqReduction")]), // FUN_10020df0 @ 0x10020df0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 45, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterJewelryStrengthReqReduction")]), // FUN_10020ee0 @ 0x10020ee0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 46, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterJewelryDexterityReqReduction")]), // FUN_10020fd0 @ 0x10020fd0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 47, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterJewelryIntelligenceReqReduction")]), // FUN_100210c0 @ 0x100210c0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 48, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterShieldStrengthReqReduction")]), // FUN_100211b0 @ 0x100211b0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 49, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterShieldDexterityReqReduction")]), // FUN_100212a0 @ 0x100212a0 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 50, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterShieldIntelligenceReqReduction")]), // FUN_10021390 @ 0x10021390 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 51, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterHuntingStrengthReqReduction")]), // FUN_10021480 @ 0x10021480 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 52, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterHuntingDexterityReqReduction")]), // FUN_10021570 @ 0x10021570 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 53, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterHuntingIntelligenceReqReduction")]), // FUN_10021660 @ 0x10021660 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 54, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterStaffStrengthReqReduction")]), // FUN_10021750 @ 0x10021750 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 55, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterStaffDexterityReqReduction")]), // FUN_10021840 @ 0x10021840 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 56, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterStaffIntelligenceReqReduction")]), // FUN_10021930 @ 0x10021930 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		// AddJitter is a no-op (0x100fd2f0) but LoadPrefixTable/LoadSuffixTable are CharAttribute's, so affix values still roll; base percent is 0 anyway.
		new(RollStore.Character, 57, "CharAttribute@0x1037302c", 0f, RollScope.All, [new("characterLevelReqReduction")], Scales: false), // FUN_10021a20 @ 0x10021a20 (Ghidra vtable label CharAttributeVal_LevelReqReduction, COMDAT-folded)
		new(RollStore.Character, 58, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterIncreasedExperience")]), // FUN_10021b10 @ 0x10021b10 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Character, 59, "CharAttribute@0x103730dc", 0f, RollScope.All, [new("characterPhysToElementalRatio")]), // FUN_10021c00 @ 0x10021c00 (Ghidra vtable label CharAttributeVal, COMDAT-folded)
		new(RollStore.Skill, 1, "SkillAttributeVal", 0f, RollScope.All, [new("skillCooldownReduction")]), // FUN_100a1190 @ 0x100a1190
		new(RollStore.Skill, 2, "SkillAttributeVal", 0f, RollScope.All, [new("skillManaCostReduction")]), // FUN_100a1250 @ 0x100a1250
		new(RollStore.Skill, 3, "SkillAttributeVal", 0f, RollScope.All, [new("skillProjectileSpeedModifier")]), // FUN_100a1310 @ 0x100a1310
		new(RollStore.Skill, 4, "SkillAttributeVal", 0f, RollScope.All, [new("skillDurationModifier")]), // FUN_100a13d0 @ 0x100a13d0
		new(RollStore.Skill, 5, "SkillAttributeMod", 0f, RollScope.All, [new("skillCooldownReductionModifier")]), // FUN_100a1490 @ 0x100a1490
		new(RollStore.Skill, 6, "SkillAttributeMod", 0f, RollScope.All, [new("skillManaCostReductionModifier")]), // FUN_100a1550 @ 0x100a1550
		// only loaded when the item IS armor (+0x628 != 0); second roll is (offensivePhysicalMax - offensivePhysicalMin) truncated to uint, jittered, truncated again; no instance when offensivePhysicalMin == 0.
		new(RollStore.Damage, 2, "DamageAttribute_Physical", 20f, RollScope.All, [new("offensivePhysicalMin"), new("offensivePhysicalMax", RoundToInt: true, RangeOf: "offensivePhysicalMin"), new("offensivePierceRatio", Scales: false)], Requires: ["offensivePhysicalMin"], Items: RollItemFilter.ArmorOnly), // FUN_1005bbd0 @ 0x1005bbd0
		// only loaded when the item is NOT armor (+0x628 == 0); AddJitter/Jitter are no-ops.
		new(RollStore.Damage, 3, "DamageAttribute_BasePhysical", 20f, RollScope.None, [new("offensivePhysicalMin"), new("offensivePhysicalMax", RangeOf: "offensivePhysicalMin")], Items: RollItemFilter.NonArmorOnly), // DamageAttribute_BasePhysical::Create_BasePhysical @ 0x10160e40
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 4, "DamageAttributeAbsBase_Fire", 20f, RollScope.None, [new("offensiveBaseFireMin"), new("offensiveBaseFireMax", RangeOf: "offensiveBaseFireMin")], Items: RollItemFilter.NonArmorOnly), // FUN_1005bd50 @ 0x1005bd50
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 5, "DamageAttributeAbsBase_Cold", 20f, RollScope.None, [new("offensiveBaseColdMin"), new("offensiveBaseColdMax", RangeOf: "offensiveBaseColdMin")], Items: RollItemFilter.NonArmorOnly), // FUN_1005bec0 @ 0x1005bec0
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 6, "DamageAttributeAbsBase_Lightning", 20f, RollScope.None, [new("offensiveBaseLightningMin"), new("offensiveBaseLightningMax", RangeOf: "offensiveBaseLightningMin")], Items: RollItemFilter.NonArmorOnly), // FUN_1005c030 @ 0x1005c030
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 7, "DamageAttributeAbsBase_Poison", 20f, RollScope.None, [new("offensiveBasePoisonMin"), new("offensiveBasePoisonMax", RangeOf: "offensiveBasePoisonMin")], Items: RollItemFilter.NonArmorOnly), // FUN_1005c1a0 @ 0x1005c1a0
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 8, "DamageAttributeAbsBase_Life", 20f, RollScope.None, [new("offensiveBaseLifeMin"), new("offensiveBaseLifeMax", RangeOf: "offensiveBaseLifeMin")], Items: RollItemFilter.NonArmorOnly), // FUN_1005c310 @ 0x1005c310
		// second roll is (offensiveBonusPhysicalMax - offensiveBonusPhysicalMin) clamped >= 0, not offensiveBonusPhysicalMax; no draw when max <= min or absent; no instance when offensiveBonusPhysicalMin == 0.
		// only loaded when the item is NOT armor (+0x628 == 0).
		new(RollStore.Damage, 9, "DamageAttributeAbs_BonusPhysical", 20f, RollScope.All, [new("offensiveBonusPhysicalMin"), new("offensiveBonusPhysicalMax", RangeOf: "offensiveBonusPhysicalMin")], Requires: ["offensiveBonusPhysicalMin"], Items: RollItemFilter.NonArmorOnly), // FUN_1005c480 @ 0x1005c480
		new(RollStore.Damage, 10, "DamageAttributeAbsMod_Physical", 20f, RollScope.All, [new("offensivePhysicalModifier")]), // FUN_1005c5f0 @ 0x1005c5f0
		new(RollStore.Damage, 11, "DamageAttributeDurMod_Physical", 20f, RollScope.All, [new("offensiveSlowPhysicalModifier"), new("offensiveSlowPhysicalDurationModifier")]), // FUN_1005c7b0 @ 0x1005c7b0
		new(RollStore.Damage, 12, "DamageAttributeAbsMod_PierceRatio", 20f, RollScope.All, [new("offensivePierceRatioModifier")]), // FUN_1005c930 @ 0x1005c930
		// second roll is (offensivePierceMax - offensivePierceMin) clamped >= 0, not offensivePierceMax; no draw when max <= min or absent; no instance when offensivePierceMin == 0.
		new(RollStore.Damage, 13, "DamageAttributeAbs_Pierce", 20f, RollScope.All, [new("offensivePierceMin"), new("offensivePierceMax", RangeOf: "offensivePierceMin")], Requires: ["offensivePierceMin"]), // FUN_1005caf0 @ 0x1005caf0
		new(RollStore.Damage, 14, "DamageAttributeAbsMod_Pierce", 20f, RollScope.All, [new("offensivePierceModifier")]), // FUN_1005cc60 @ 0x1005cc60
		// second roll is (offensiveSlowBleedingMax - offensiveSlowBleedingMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowBleedingDurationMin == 0.
		new(RollStore.Damage, 15, "DamageAttributeDur_Bleeding", 20f, RollScope.All, [new("offensiveSlowBleedingMin"), new("offensiveSlowBleedingMax", RangeOf: "offensiveSlowBleedingMin")], Requires: ["offensiveSlowBleedingMin", "offensiveSlowBleedingDurationMin"]), // FUN_1005ce20 @ 0x1005ce20
		new(RollStore.Damage, 16, "DamageAttributeAbsMod_Bleeding", 20f, RollScope.All, [new("offensiveBleedingModifier")]), // FUN_1005cfa0 @ 0x1005cfa0
		new(RollStore.Damage, 17, "DamageAttributeDurMod_Bleeding", 20f, RollScope.All, [new("offensiveSlowBleedingModifier"), new("offensiveSlowBleedingDurationModifier")]), // FUN_1005d160 @ 0x1005d160
		// second roll is (offensiveElementalMax - offensiveElementalMin) clamped >= 0, not offensiveElementalMax; no draw when max <= min or absent; no instance when offensiveElementalMin == 0.
		new(RollStore.Damage, 18, "DamageAttributeAbs_ElementalDamage", 20f, RollScope.All, [new("offensiveElementalMin"), new("offensiveElementalMax", RangeOf: "offensiveElementalMin")], Requires: ["offensiveElementalMin"]), // FUN_1005d2e0 @ 0x1005d2e0
		new(RollStore.Damage, 19, "DamageAttributeAbsMod_Elemental", 20f, RollScope.All, [new("offensiveElementalModifier")]), // FUN_1005d450 @ 0x1005d450
		new(RollStore.Damage, 20, "DamageAttributeAbsMod_TotalDamageModifier", 20f, RollScope.All, [new("offensiveTotalDamageModifier")]), // FUN_1005d610 @ 0x1005d610
		// second roll is (offensiveFireMax - offensiveFireMin) clamped >= 0, not offensiveFireMax; no draw when max <= min or absent; no instance when offensiveFireMin == 0.
		new(RollStore.Damage, 21, "DamageAttributeAbs_Fire", 20f, RollScope.All, [new("offensiveFireMin"), new("offensiveFireMax", RangeOf: "offensiveFireMin")], Requires: ["offensiveFireMin"]), // FUN_1005d7d0 @ 0x1005d7d0
		// second roll is (offensiveSlowFireMax - offensiveSlowFireMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowFireDurationMin == 0.
		new(RollStore.Damage, 22, "DamageAttributeDur_Fire", 20f, RollScope.All, [new("offensiveSlowFireMin"), new("offensiveSlowFireMax", RangeOf: "offensiveSlowFireMin")], Requires: ["offensiveSlowFireMin", "offensiveSlowFireDurationMin"]), // FUN_1005d940 @ 0x1005d940
		new(RollStore.Damage, 23, "DamageAttributeAbsMod_Fire", 20f, RollScope.All, [new("offensiveFireModifier")]), // FUN_1005dac0 @ 0x1005dac0
		new(RollStore.Damage, 24, "DamageAttributeDurMod_Fire", 20f, RollScope.All, [new("offensiveSlowFireModifier"), new("offensiveSlowFireDurationModifier")]), // FUN_1005dc80 @ 0x1005dc80
		// second roll is (offensiveColdMax - offensiveColdMin) clamped >= 0, not offensiveColdMax; no draw when max <= min or absent; no instance when offensiveColdMin == 0.
		new(RollStore.Damage, 25, "DamageAttributeAbs_Cold", 20f, RollScope.All, [new("offensiveColdMin"), new("offensiveColdMax", RangeOf: "offensiveColdMin")], Requires: ["offensiveColdMin"]), // FUN_1005de00 @ 0x1005de00
		// second roll is (offensiveSlowColdMax - offensiveSlowColdMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowColdDurationMin == 0.
		new(RollStore.Damage, 26, "DamageAttributeDur_Cold", 20f, RollScope.All, [new("offensiveSlowColdMin"), new("offensiveSlowColdMax", RangeOf: "offensiveSlowColdMin")], Requires: ["offensiveSlowColdMin", "offensiveSlowColdDurationMin"]), // FUN_1005df70 @ 0x1005df70
		new(RollStore.Damage, 27, "DamageAttributeAbsMod_Cold", 20f, RollScope.All, [new("offensiveColdModifier")]), // FUN_1005e0f0 @ 0x1005e0f0
		new(RollStore.Damage, 28, "DamageAttributeDurMod_Cold", 20f, RollScope.All, [new("offensiveSlowColdModifier"), new("offensiveSlowColdDurationModifier")]), // FUN_1005e2b0 @ 0x1005e2b0
		// second roll is (offensiveLightningMax - offensiveLightningMin) clamped >= 0, not offensiveLightningMax; no draw when max <= min or absent; no instance when offensiveLightningMin == 0.
		new(RollStore.Damage, 29, "DamageAttributeAbs_Lightning", 20f, RollScope.All, [new("offensiveLightningMin"), new("offensiveLightningMax", RangeOf: "offensiveLightningMin")], Requires: ["offensiveLightningMin"]), // FUN_1005e430 @ 0x1005e430
		// second roll is (offensiveSlowLightningMax - offensiveSlowLightningMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLightningDurationMin == 0.
		new(RollStore.Damage, 30, "DamageAttributeDur_Lightning", 20f, RollScope.All, [new("offensiveSlowLightningMin"), new("offensiveSlowLightningMax", RangeOf: "offensiveSlowLightningMin")], Requires: ["offensiveSlowLightningMin", "offensiveSlowLightningDurationMin"]), // FUN_1005e5a0 @ 0x1005e5a0
		new(RollStore.Damage, 31, "DamageAttributeAbsMod_Lightning", 20f, RollScope.All, [new("offensiveLightningModifier")]), // FUN_1005e720 @ 0x1005e720
		new(RollStore.Damage, 32, "DamageAttributeDurMod_Lightning", 20f, RollScope.All, [new("offensiveSlowLightningModifier"), new("offensiveSlowLightningDurationModifier")]), // FUN_1005e8e0 @ 0x1005e8e0
		// second roll is (offensivePoisonMax - offensivePoisonMin) clamped >= 0, not offensivePoisonMax; no draw when max <= min or absent; no instance when offensivePoisonMin == 0.
		new(RollStore.Damage, 33, "DamageAttributeAbs_Poison", 20f, RollScope.All, [new("offensivePoisonMin"), new("offensivePoisonMax", RangeOf: "offensivePoisonMin")], Requires: ["offensivePoisonMin"]), // FUN_1005ea60 @ 0x1005ea60
		// second roll is (offensiveSlowPoisonMax - offensiveSlowPoisonMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowPoisonDurationMin == 0.
		new(RollStore.Damage, 34, "DamageAttributeDur_Poison", 20f, RollScope.All, [new("offensiveSlowPoisonMin"), new("offensiveSlowPoisonMax", RangeOf: "offensiveSlowPoisonMin")], Requires: ["offensiveSlowPoisonMin", "offensiveSlowPoisonDurationMin"]), // FUN_1005ebd0 @ 0x1005ebd0
		new(RollStore.Damage, 35, "DamageAttributeAbsMod_Poison", 20f, RollScope.All, [new("offensivePoisonModifier")]), // FUN_1005ed50 @ 0x1005ed50
		new(RollStore.Damage, 36, "DamageAttributeDurMod_Poison", 20f, RollScope.All, [new("offensiveSlowPoisonModifier"), new("offensiveSlowPoisonDurationModifier")]), // FUN_1005ef10 @ 0x1005ef10
		// second roll is (offensivePercentCurrentLifeMax - offensivePercentCurrentLifeMin) clamped >= 0, not offensivePercentCurrentLifeMax; no draw when max <= min or absent; no instance when offensivePercentCurrentLifeMin == 0.
		new(RollStore.Damage, 37, "DamageAttributeAbs_PercentCurrentLife", 20f, RollScope.All, [new("offensivePercentCurrentLifeMin"), new("offensivePercentCurrentLifeMax", RangeOf: "offensivePercentCurrentLifeMin")], Requires: ["offensivePercentCurrentLifeMin"]), // FUN_1005f090 @ 0x1005f090
		// second roll is (offensiveLifeMax - offensiveLifeMin) clamped >= 0, not offensiveLifeMax; no draw when max <= min or absent; no instance when offensiveLifeMin == 0.
		new(RollStore.Damage, 38, "DamageAttributeAbs_Life", 20f, RollScope.All, [new("offensiveLifeMin"), new("offensiveLifeMax", RangeOf: "offensiveLifeMin")], Requires: ["offensiveLifeMin"]), // FUN_1005f200 @ 0x1005f200
		// second roll is (offensiveSlowLifeMax - offensiveSlowLifeMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLifeDurationMin == 0.
		new(RollStore.Damage, 39, "DamageAttributeDur_Life", 20f, RollScope.All, [new("offensiveSlowLifeMin"), new("offensiveSlowLifeMax", RangeOf: "offensiveSlowLifeMin")], Requires: ["offensiveSlowLifeMin", "offensiveSlowLifeDurationMin"]), // FUN_1005f370 @ 0x1005f370
		new(RollStore.Damage, 40, "DamageAttributeAbsMod_Life", 20f, RollScope.All, [new("offensiveLifeModifier")]), // FUN_1005f4f0 @ 0x1005f4f0
		new(RollStore.Damage, 41, "DamageAttributeDurMod_Life", 20f, RollScope.All, [new("offensiveSlowLifeModifier"), new("offensiveSlowLifeDurationModifier")]), // FUN_1005f6b0 @ 0x1005f6b0
		// second roll is (offensiveLifeLeechMax - offensiveLifeLeechMin) clamped >= 0, not offensiveLifeLeechMax; no draw when max <= min or absent; no instance when offensiveLifeLeechMin == 0.
		new(RollStore.Damage, 42, "DamageAttributeAbs_LifeLeech", 20f, RollScope.All, [new("offensiveLifeLeechMin"), new("offensiveLifeLeechMax", RangeOf: "offensiveLifeLeechMin")], Requires: ["offensiveLifeLeechMin"]), // FUN_1005f830 @ 0x1005f830
		// second roll is (offensiveSlowLifeLeachMax - offensiveSlowLifeLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowLifeLeachDurationMin == 0.
		new(RollStore.Damage, 43, "DamageAttributeDur_LifeLeach", 20f, RollScope.All, [new("offensiveSlowLifeLeachMin"), new("offensiveSlowLifeLeachMax", RangeOf: "offensiveSlowLifeLeachMin")], Requires: ["offensiveSlowLifeLeachMin", "offensiveSlowLifeLeachDurationMin"]), // FUN_1005f9a0 @ 0x1005f9a0
		new(RollStore.Damage, 44, "DamageAttributeDurMod_LifeLeach", 20f, RollScope.All, [new("offensiveSlowLifeLeachModifier"), new("offensiveSlowLifeLeachDurationModifier")]), // FUN_1005fb20 @ 0x1005fb20
		// second roll is (offensiveManaBurnDrainMax - offensiveManaBurnDrainMin) clamped >= 0, not offensiveManaBurnDrainMax; no draw when max <= min or absent; no instance when offensiveManaBurnDrainMin == 0.
		new(RollStore.Damage, 45, "DamageAttributeAbs_ManaBurn", 20f, RollScope.All, [new("offensiveManaBurnDrainMin"), new("offensiveManaBurnDrainMax", RangeOf: "offensiveManaBurnDrainMin")], Requires: ["offensiveManaBurnDrainMin"]), // FUN_1005fca0 @ 0x1005fca0
		new(RollStore.Damage, 46, "DamageAttributeAbsMod_ManaBurn", 20f, RollScope.All, [new("offensiveManaBurnRatioAdder")]), // FUN_1005fdd0 @ 0x1005fdd0
		// second roll is (offensiveSlowManaLeachMax - offensiveSlowManaLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowManaLeachDurationMin == 0.
		new(RollStore.Damage, 47, "DamageAttributeDur_ManaLeach", 20f, RollScope.All, [new("offensiveSlowManaLeachMin"), new("offensiveSlowManaLeachMax", RangeOf: "offensiveSlowManaLeachMin")], Requires: ["offensiveSlowManaLeachMin", "offensiveSlowManaLeachDurationMin"]), // FUN_1005ff90 @ 0x1005ff90
		new(RollStore.Damage, 48, "DamageAttributeDurMod_ManaLeach", 20f, RollScope.All, [new("offensiveSlowManaLeachModifier"), new("offensiveSlowManaLeachDurationModifier")]), // FUN_10060110 @ 0x10060110
		// second roll is (offensiveSlowOffensiveAbilityMax - offensiveSlowOffensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowOffensiveAbilityDurationMin == 0.
		new(RollStore.Damage, 49, "DamageAttributeDur_OffensiveAbility", 20f, RollScope.All, [new("offensiveSlowOffensiveAbilityMin"), new("offensiveSlowOffensiveAbilityMax", RangeOf: "offensiveSlowOffensiveAbilityMin")], Requires: ["offensiveSlowOffensiveAbilityMin", "offensiveSlowOffensiveAbilityDurationMin"]), // FUN_10060290 @ 0x10060290
		new(RollStore.Damage, 50, "DamageAttributeDurMod_OffensiveAbility", 20f, RollScope.All, [new("offensiveSlowOffensiveAbilityModifier"), new("offensiveSlowOffensiveAbilityDurationModifier")]), // FUN_10060410 @ 0x10060410
		// second roll is (offensiveSlowDefensiveAbilityMax - offensiveSlowDefensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowDefensiveAbilityDurationMin == 0.
		new(RollStore.Damage, 51, "DamageAttributeDur_DefensiveAbility", 20f, RollScope.All, [new("offensiveSlowDefensiveAbilityMin"), new("offensiveSlowDefensiveAbilityMax", RangeOf: "offensiveSlowDefensiveAbilityMin")], Requires: ["offensiveSlowDefensiveAbilityMin", "offensiveSlowDefensiveAbilityDurationMin"]), // FUN_10060590 @ 0x10060590
		new(RollStore.Damage, 52, "DamageAttributeDurMod_DefensiveAbility", 20f, RollScope.All, [new("offensiveSlowDefensiveAbilityModifier"), new("offensiveSlowDefensiveAbilityDurationModifier")]), // FUN_10060710 @ 0x10060710
		// second roll is (offensiveSlowOffensiveReductionMax - offensiveSlowOffensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowOffensiveReductionDurationMin == 0.
		new(RollStore.Damage, 53, "DamageAttributeDur_OffensiveReduction", 20f, RollScope.All, [new("offensiveSlowOffensiveReductionMin"), new("offensiveSlowOffensiveReductionMax", RangeOf: "offensiveSlowOffensiveReductionMin")], Requires: ["offensiveSlowOffensiveReductionMin", "offensiveSlowOffensiveReductionDurationMin"]), // FUN_10060890 @ 0x10060890
		new(RollStore.Damage, 54, "DamageAttributeDurMod_OffensiveReduction", 20f, RollScope.All, [new("offensiveSlowOffensiveReductionModifier"), new("offensiveSlowOffensiveReductionDurationModifier")]), // FUN_10060a10 @ 0x10060a10
		// second roll is (offensiveSlowDefensiveReductionMax - offensiveSlowDefensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowDefensiveReductionDurationMin == 0.
		new(RollStore.Damage, 55, "DamageAttributeDur_DefensiveReduction", 20f, RollScope.All, [new("offensiveSlowDefensiveReductionMin"), new("offensiveSlowDefensiveReductionMax", RangeOf: "offensiveSlowDefensiveReductionMin")], Requires: ["offensiveSlowDefensiveReductionMin", "offensiveSlowDefensiveReductionDurationMin"]), // FUN_10060b90 @ 0x10060b90
		// second roll is (offensiveTotalDamageReductionPercentMax - offensiveTotalDamageReductionPercentMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalDamageReductionPercentDurationMin == 0.
		new(RollStore.Damage, 56, "DamageAttributeDur_TotalDamageReductionPercent", 20f, RollScope.All, [new("offensiveTotalDamageReductionPercentMin"), new("offensiveTotalDamageReductionPercentMax", RangeOf: "offensiveTotalDamageReductionPercentMin")], Requires: ["offensiveTotalDamageReductionPercentMin", "offensiveTotalDamageReductionPercentDurationMin"]), // FUN_10060d10 @ 0x10060d10
		// second roll is (offensiveTotalDamageReductionAbsoluteMax - offensiveTotalDamageReductionAbsoluteMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalDamageReductionAbsoluteDurationMin == 0.
		new(RollStore.Damage, 57, "DamageAttributeDur_TotalDamageReductionAbsolute", 20f, RollScope.All, [new("offensiveTotalDamageReductionAbsoluteMin"), new("offensiveTotalDamageReductionAbsoluteMax", RangeOf: "offensiveTotalDamageReductionAbsoluteMin")], Requires: ["offensiveTotalDamageReductionAbsoluteMin", "offensiveTotalDamageReductionAbsoluteDurationMin"]), // FUN_10060e90 @ 0x10060e90
		// second roll is (offensiveTotalResistanceReductionPercentMax - offensiveTotalResistanceReductionPercentMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalResistanceReductionPercentDurationMin == 0.
		new(RollStore.Damage, 58, "DamageAttributeDur_TotalResistanceReductionPercent", 20f, RollScope.All, [new("offensiveTotalResistanceReductionPercentMin"), new("offensiveTotalResistanceReductionPercentMax", RangeOf: "offensiveTotalResistanceReductionPercentMin")], Requires: ["offensiveTotalResistanceReductionPercentMin", "offensiveTotalResistanceReductionPercentDurationMin"]), // FUN_10061010 @ 0x10061010
		// second roll is (offensiveTotalResistanceReductionAbsoluteMax - offensiveTotalResistanceReductionAbsoluteMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveTotalResistanceReductionAbsoluteDurationMin == 0.
		new(RollStore.Damage, 59, "DamageAttributeDur_TotalResistanceReductionAbsolute", 20f, RollScope.All, [new("offensiveTotalResistanceReductionAbsoluteMin"), new("offensiveTotalResistanceReductionAbsoluteMax", RangeOf: "offensiveTotalResistanceReductionAbsoluteMin")], Requires: ["offensiveTotalResistanceReductionAbsoluteMin", "offensiveTotalResistanceReductionAbsoluteDurationMin"]), // FUN_10061190 @ 0x10061190
		// second roll is (offensiveFumbleMax - offensiveFumbleMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveFumbleDurationMin == 0.
		new(RollStore.Damage, 60, "DamageAttributeDur_Fumble", 20f, RollScope.All, [new("offensiveFumbleMin"), new("offensiveFumbleMax", RangeOf: "offensiveFumbleMin")], Requires: ["offensiveFumbleMin", "offensiveFumbleDurationMin"]), // FUN_10061310 @ 0x10061310
		// second roll is (offensiveProjectileFumbleMax - offensiveProjectileFumbleMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveProjectileFumbleDurationMin == 0.
		new(RollStore.Damage, 61, "DamageAttributeDur_ProjectileFumble", 20f, RollScope.All, [new("offensiveProjectileFumbleMin"), new("offensiveProjectileFumbleMax", RangeOf: "offensiveProjectileFumbleMin")], Requires: ["offensiveProjectileFumbleMin", "offensiveProjectileFumbleDurationMin"]), // FUN_10061490 @ 0x10061490
		// second roll is (offensiveSleepMax - offensiveSleepMin) clamped >= 0, not offensiveSleepMax; no draw when max <= min or absent; no instance when offensiveSleepMin == 0.
		new(RollStore.Damage, 62, "DamageAttributeReflex_Sleep", 20f, RollScope.All, [new("offensiveSleepMin"), new("offensiveSleepMax", RangeOf: "offensiveSleepMin")], Requires: ["offensiveSleepMin"]), // FUN_10061610 @ 0x10061610
		new(RollStore.Damage, 63, "DamageAttributeAbsMod_Sleep", 20f, RollScope.All, [new("offensiveSleepModifier")]), // FUN_10061780 @ 0x10061780
		// second roll is (offensiveStunMax - offensiveStunMin) clamped >= 0, not offensiveStunMax; no draw when max <= min or absent; no instance when offensiveStunMin == 0.
		new(RollStore.Damage, 64, "DamageAttributeReflex_Stun", 20f, RollScope.All, [new("offensiveStunMin"), new("offensiveStunMax", RangeOf: "offensiveStunMin")], Requires: ["offensiveStunMin"]), // FUN_10061940 @ 0x10061940
		new(RollStore.Damage, 65, "DamageAttributeAbsMod_Stun", 20f, RollScope.All, [new("offensiveStunModifier")]), // FUN_10061ab0 @ 0x10061ab0
		// second roll is (offensiveFreezeMax - offensiveFreezeMin) clamped >= 0, not offensiveFreezeMax; no draw when max <= min or absent; no instance when offensiveFreezeMin == 0.
		new(RollStore.Damage, 66, "DamageAttributeReflex_Freeze", 20f, RollScope.All, [new("offensiveFreezeMin"), new("offensiveFreezeMax", RangeOf: "offensiveFreezeMin")], Requires: ["offensiveFreezeMin"]), // FUN_10061c70 @ 0x10061c70
		// second roll is (offensivePetrifyMax - offensivePetrifyMin) clamped >= 0, not offensivePetrifyMax; no draw when max <= min or absent; no instance when offensivePetrifyMin == 0.
		new(RollStore.Damage, 67, "DamageAttributeReflex_Petrify", 20f, RollScope.All, [new("offensivePetrifyMin"), new("offensivePetrifyMax", RangeOf: "offensivePetrifyMin")], Requires: ["offensivePetrifyMin"]), // FUN_10061de0 @ 0x10061de0
		// second roll is (offensiveConvertMax - offensiveConvertMin) clamped >= 0, not offensiveConvertMax; no draw when max <= min or absent; no instance when offensiveConvertMin == 0.
		new(RollStore.Damage, 68, "DamageAttributeAbs_Convert", 20f, RollScope.All, [new("offensiveConvertMin"), new("offensiveConvertMax", RangeOf: "offensiveConvertMin")], Requires: ["offensiveConvertMin"]), // FUN_10061f50 @ 0x10061f50
		// second roll is (offensiveConfusionMax - offensiveConfusionMin) clamped >= 0, not offensiveConfusionMax; no draw when max <= min or absent; no instance when offensiveConfusionMin == 0.
		new(RollStore.Damage, 69, "DamageAttributeAbs_Confusion", 20f, RollScope.All, [new("offensiveConfusionMin"), new("offensiveConfusionMax", RangeOf: "offensiveConfusionMin")], Requires: ["offensiveConfusionMin"]), // FUN_100620c0 @ 0x100620c0
		// second roll is (offensiveSlowTotalSpeedMax - offensiveSlowTotalSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowTotalSpeedDurationMin == 0.
		new(RollStore.Damage, 70, "DamageAttributeDur_TotalSpeed", 20f, RollScope.All, [new("offensiveSlowTotalSpeedMin"), new("offensiveSlowTotalSpeedMax", RangeOf: "offensiveSlowTotalSpeedMin")], Requires: ["offensiveSlowTotalSpeedMin", "offensiveSlowTotalSpeedDurationMin"]), // FUN_10062230 @ 0x10062230
		// second roll is (offensiveSlowAttackSpeedMax - offensiveSlowAttackSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowAttackSpeedDurationMin == 0.
		new(RollStore.Damage, 71, "DamageAttributeDur_AttackSpeed", 20f, RollScope.All, [new("offensiveSlowAttackSpeedMin"), new("offensiveSlowAttackSpeedMax", RangeOf: "offensiveSlowAttackSpeedMin")], Requires: ["offensiveSlowAttackSpeedMin", "offensiveSlowAttackSpeedDurationMin"]), // FUN_100623b0 @ 0x100623b0
		new(RollStore.Damage, 72, "DamageAttributeDurMod_AttackSpeed", 20f, RollScope.All, [new("offensiveSlowAttackSpeedModifier"), new("offensiveSlowAttackSpeedDurationModifier")]), // FUN_10062530 @ 0x10062530
		// second roll is (offensiveSlowSpellCastSpeedMax - offensiveSlowSpellCastSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowSpellCastSpeedDurationMin == 0.
		new(RollStore.Damage, 73, "DamageAttributeDur_SpellCastSpeed", 20f, RollScope.All, [new("offensiveSlowSpellCastSpeedMin"), new("offensiveSlowSpellCastSpeedMax", RangeOf: "offensiveSlowSpellCastSpeedMin")], Requires: ["offensiveSlowSpellCastSpeedMin", "offensiveSlowSpellCastSpeedDurationMin"]), // FUN_100626b0 @ 0x100626b0
		new(RollStore.Damage, 74, "DamageAttributeDurMod_SpellCastSpeed", 20f, RollScope.All, [new("offensiveSlowSpellCastSpeedModifier"), new("offensiveSlowSpellCastSpeedDurationModifier")]), // FUN_10062830 @ 0x10062830
		// second roll is (offensiveSlowRunSpeedMax - offensiveSlowRunSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when offensiveSlowRunSpeedDurationMin == 0.
		new(RollStore.Damage, 75, "DamageAttributeDur_RunSpeed", 20f, RollScope.All, [new("offensiveSlowRunSpeedMin"), new("offensiveSlowRunSpeedMax", RangeOf: "offensiveSlowRunSpeedMin")], Requires: ["offensiveSlowRunSpeedMin", "offensiveSlowRunSpeedDurationMin"]), // FUN_100629b0 @ 0x100629b0
		new(RollStore.Damage, 76, "DamageAttributeDurMod_RunSpeed", 20f, RollScope.All, [new("offensiveSlowRunSpeedModifier"), new("offensiveSlowRunSpeedDurationModifier")]), // FUN_10062b30 @ 0x10062b30
		// second roll is (offensiveDisruptionMax - offensiveDisruptionMin) clamped >= 0, not offensiveDisruptionMax; no draw when max <= min or absent; no instance when offensiveDisruptionMin == 0.
		new(RollStore.Damage, 77, "DamageAttributeAbs_Disruption", 20f, RollScope.All, [new("offensiveDisruptionMin"), new("offensiveDisruptionMax", RangeOf: "offensiveDisruptionMin")], Requires: ["offensiveDisruptionMin"]), // FUN_10062cb0 @ 0x10062cb0
		// second roll is (retaliationPhysicalMax - retaliationPhysicalMin) clamped >= 0, not retaliationPhysicalMax; no draw when max <= min or absent; no instance when retaliationPhysicalMin == 0.
		new(RollStore.Retaliation, 2, "RetaliationAttributeAbs_Physical", 20f, RollScope.All, [new("retaliationPhysicalMin"), new("retaliationPhysicalMax", RangeOf: "retaliationPhysicalMin")], Requires: ["retaliationPhysicalMin"]), // FUN_1009a0d0 @ 0x1009a0d0
		// second roll is (retaliationSlowPhysicalMax - retaliationSlowPhysicalMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowPhysicalDurationMin == 0.
		new(RollStore.Retaliation, 3, "RetaliationAttributeDur_Physical", 20f, RollScope.All, [new("retaliationSlowPhysicalMin"), new("retaliationSlowPhysicalMax", RangeOf: "retaliationSlowPhysicalMin")], Requires: ["retaliationSlowPhysicalMin", "retaliationSlowPhysicalDurationMin"]), // FUN_1009a240 @ 0x1009a240
		new(RollStore.Retaliation, 4, "RetaliationAttributeAbsMod_Physical", 20f, RollScope.All, [new("retaliationPhysicalModifier")]), // FUN_1009a3c0 @ 0x1009a3c0
		new(RollStore.Retaliation, 5, "RetaliationAttributeDurMod_Physical", 20f, RollScope.All, [new("retaliationSlowPhysicalModifier"), new("retaliationSlowPhysicalDurationModifier")]), // FUN_1009a580 @ 0x1009a580
		new(RollStore.Retaliation, 6, "RetaliationAttributeAbsMod_PierceRatio", 20f, RollScope.All, [new("retaliationPierceRatioModifier")]), // FUN_1009a700 @ 0x1009a700
		// second roll is (retaliationPierceMax - retaliationPierceMin) clamped >= 0, not retaliationPierceMax; no draw when max <= min or absent; no instance when retaliationPierceMin == 0.
		new(RollStore.Retaliation, 7, "RetaliationAttributeAbs_Pierce", 20f, RollScope.All, [new("retaliationPierceMin"), new("retaliationPierceMax", RangeOf: "retaliationPierceMin")], Requires: ["retaliationPierceMin"]), // FUN_1009a8c0 @ 0x1009a8c0
		new(RollStore.Retaliation, 8, "RetaliationAttributeAbsMod_Pierce", 20f, RollScope.All, [new("retaliationPierceModifier")]), // FUN_1009aa30 @ 0x1009aa30
		// second roll is (retaliationSlowBleedingMax - retaliationSlowBleedingMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowBleedingDurationMin == 0.
		new(RollStore.Retaliation, 9, "RetaliationAttributeDur_Bleeding", 20f, RollScope.All, [new("retaliationSlowBleedingMin"), new("retaliationSlowBleedingMax", RangeOf: "retaliationSlowBleedingMin")], Requires: ["retaliationSlowBleedingMin", "retaliationSlowBleedingDurationMin"]), // FUN_1009abf0 @ 0x1009abf0
		new(RollStore.Retaliation, 10, "RetaliationAttributeDurMod_Bleeding", 20f, RollScope.All, [new("retaliationSlowBleedingModifier"), new("retaliationSlowBleedingDurationModifier")]), // FUN_1009ad70 @ 0x1009ad70
		// second roll is (retaliationElementalMax - retaliationElementalMin) clamped >= 0, not retaliationElementalMax; no draw when max <= min or absent; no instance when retaliationElementalMin == 0.
		new(RollStore.Retaliation, 11, "RetaliationAttributeAbs_ElementalDamage", 20f, RollScope.All, [new("retaliationElementalMin"), new("retaliationElementalMax", RangeOf: "retaliationElementalMin")], Requires: ["retaliationElementalMin"]), // FUN_1009aef0 @ 0x1009aef0
		// second roll is (retaliationColdMax - retaliationColdMin) clamped >= 0, not retaliationColdMax; no draw when max <= min or absent; no instance when retaliationColdMin == 0.
		new(RollStore.Retaliation, 12, "RetaliationAttributeAbs_Cold", 20f, RollScope.All, [new("retaliationColdMin"), new("retaliationColdMax", RangeOf: "retaliationColdMin")], Requires: ["retaliationColdMin"]), // FUN_1009b060 @ 0x1009b060
		// second roll is (retaliationSlowColdMax - retaliationSlowColdMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowColdDurationMin == 0.
		new(RollStore.Retaliation, 13, "RetaliationAttributeDur_Cold", 20f, RollScope.All, [new("retaliationSlowColdMin"), new("retaliationSlowColdMax", RangeOf: "retaliationSlowColdMin")], Requires: ["retaliationSlowColdMin", "retaliationSlowColdDurationMin"]), // FUN_1009b1d0 @ 0x1009b1d0
		new(RollStore.Retaliation, 14, "RetaliationAttributeAbsMod_Cold", 20f, RollScope.All, [new("retaliationColdModifier")]), // FUN_1009b350 @ 0x1009b350
		new(RollStore.Retaliation, 15, "RetaliationAttributeDurMod_Cold", 20f, RollScope.All, [new("retaliationSlowColdModifier"), new("retaliationSlowColdDurationModifier")]), // FUN_1009b510 @ 0x1009b510
		// second roll is (retaliationPoisonMax - retaliationPoisonMin) clamped >= 0, not retaliationPoisonMax; no draw when max <= min or absent; no instance when retaliationPoisonMin == 0.
		new(RollStore.Retaliation, 16, "RetaliationAttributeAbs_Poison", 20f, RollScope.All, [new("retaliationPoisonMin"), new("retaliationPoisonMax", RangeOf: "retaliationPoisonMin")], Requires: ["retaliationPoisonMin"]), // FUN_1009b690 @ 0x1009b690
		// second roll is (retaliationSlowPoisonMax - retaliationSlowPoisonMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowPoisonDurationMin == 0.
		new(RollStore.Retaliation, 17, "RetaliationAttributeDur_Poison", 20f, RollScope.All, [new("retaliationSlowPoisonMin"), new("retaliationSlowPoisonMax", RangeOf: "retaliationSlowPoisonMin")], Requires: ["retaliationSlowPoisonMin", "retaliationSlowPoisonDurationMin"]), // FUN_1009b800 @ 0x1009b800
		new(RollStore.Retaliation, 18, "RetaliationAttributeAbsMod_Poison", 20f, RollScope.All, [new("retaliationPoisonModifier")]), // FUN_1009b980 @ 0x1009b980
		new(RollStore.Retaliation, 19, "RetaliationAttributeDurMod_Poison", 20f, RollScope.All, [new("retaliationSlowPoisonModifier"), new("retaliationSlowPoisonDurationModifier")]), // FUN_1009bb40 @ 0x1009bb40
		// second roll is (retaliationLightningMax - retaliationLightningMin) clamped >= 0, not retaliationLightningMax; no draw when max <= min or absent; no instance when retaliationLightningMin == 0.
		new(RollStore.Retaliation, 20, "RetaliationAttributeAbs_Lightning", 20f, RollScope.All, [new("retaliationLightningMin"), new("retaliationLightningMax", RangeOf: "retaliationLightningMin")], Requires: ["retaliationLightningMin"]), // FUN_1009bcc0 @ 0x1009bcc0
		// second roll is (retaliationSlowLightningMax - retaliationSlowLightningMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLightningDurationMin == 0.
		new(RollStore.Retaliation, 21, "RetaliationAttributeDur_Lightning", 20f, RollScope.All, [new("retaliationSlowLightningMin"), new("retaliationSlowLightningMax", RangeOf: "retaliationSlowLightningMin")], Requires: ["retaliationSlowLightningMin", "retaliationSlowLightningDurationMin"]), // FUN_1009be30 @ 0x1009be30
		new(RollStore.Retaliation, 22, "RetaliationAttributeAbsMod_Lightning", 20f, RollScope.All, [new("retaliationLightningModifier")]), // FUN_1009bfb0 @ 0x1009bfb0
		new(RollStore.Retaliation, 23, "RetaliationAttributeDurMod_Lightning", 20f, RollScope.All, [new("retaliationSlowLightningModifier"), new("retaliationSlowLightningDurationModifier")]), // FUN_1009c170 @ 0x1009c170
		// second roll is (retaliationFireMax - retaliationFireMin) clamped >= 0, not retaliationFireMax; no draw when max <= min or absent; no instance when retaliationFireMin == 0.
		new(RollStore.Retaliation, 24, "RetaliationAttributeAbs_Fire", 20f, RollScope.All, [new("retaliationFireMin"), new("retaliationFireMax", RangeOf: "retaliationFireMin")], Requires: ["retaliationFireMin"]), // FUN_1009c2f0 @ 0x1009c2f0
		// second roll is (retaliationSlowFireMax - retaliationSlowFireMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowFireDurationMin == 0.
		new(RollStore.Retaliation, 25, "RetaliationAttributeDur_Fire", 20f, RollScope.All, [new("retaliationSlowFireMin"), new("retaliationSlowFireMax", RangeOf: "retaliationSlowFireMin")], Requires: ["retaliationSlowFireMin", "retaliationSlowFireDurationMin"]), // FUN_1009c460 @ 0x1009c460
		new(RollStore.Retaliation, 26, "RetaliationAttributeAbsMod_Fire", 20f, RollScope.All, [new("retaliationFireModifier")]), // FUN_1009c5e0 @ 0x1009c5e0
		new(RollStore.Retaliation, 27, "RetaliationAttributeDurMod_Fire", 20f, RollScope.All, [new("retaliationSlowFireModifier"), new("retaliationSlowFireDurationModifier")]), // FUN_1009c7a0 @ 0x1009c7a0
		// second roll is (retaliationPercentCurrentLifeMax - retaliationPercentCurrentLifeMin) clamped >= 0, not retaliationPercentCurrentLifeMax; no draw when max <= min or absent; no instance when retaliationPercentCurrentLifeMin == 0.
		new(RollStore.Retaliation, 28, "RetaliationAttributeAbs_PercentCurrentLife", 20f, RollScope.All, [new("retaliationPercentCurrentLifeMin"), new("retaliationPercentCurrentLifeMax", RangeOf: "retaliationPercentCurrentLifeMin")], Requires: ["retaliationPercentCurrentLifeMin"]), // FUN_1009c920 @ 0x1009c920
		// second roll is (retaliationLifeMax - retaliationLifeMin) clamped >= 0, not retaliationLifeMax; no draw when max <= min or absent; no instance when retaliationLifeMin == 0.
		new(RollStore.Retaliation, 29, "RetaliationAttributeAbs_Life", 20f, RollScope.All, [new("retaliationLifeMin"), new("retaliationLifeMax", RangeOf: "retaliationLifeMin")], Requires: ["retaliationLifeMin"]), // FUN_1009ca90 @ 0x1009ca90
		// second roll is (retaliationSlowLifeMax - retaliationSlowLifeMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLifeDurationMin == 0.
		new(RollStore.Retaliation, 30, "RetaliationAttributeDur_Life", 20f, RollScope.All, [new("retaliationSlowLifeMin"), new("retaliationSlowLifeMax", RangeOf: "retaliationSlowLifeMin")], Requires: ["retaliationSlowLifeMin", "retaliationSlowLifeDurationMin"]), // FUN_1009cc00 @ 0x1009cc00
		new(RollStore.Retaliation, 31, "RetaliationAttributeAbsMod_Life", 20f, RollScope.All, [new("retaliationLifeModifier")]), // FUN_1009cd80 @ 0x1009cd80
		new(RollStore.Retaliation, 32, "RetaliationAttributeDurMod_Life", 20f, RollScope.All, [new("retaliationSlowLifeModifier"), new("retaliationSlowLifeDurationModifier")]), // FUN_1009cf40 @ 0x1009cf40
		// second roll is (retaliationSlowLifeLeachMax - retaliationSlowLifeLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowLifeLeachDurationMin == 0.
		new(RollStore.Retaliation, 33, "RetaliationAttributeDur_LifeLeach", 20f, RollScope.All, [new("retaliationSlowLifeLeachMin"), new("retaliationSlowLifeLeachMax", RangeOf: "retaliationSlowLifeLeachMin")], Requires: ["retaliationSlowLifeLeachMin", "retaliationSlowLifeLeachDurationMin"]), // FUN_1009d0c0 @ 0x1009d0c0
		new(RollStore.Retaliation, 34, "RetaliationAttributeDurMod_LifeLeach", 20f, RollScope.All, [new("retaliationSlowLifeLeachModifier"), new("retaliationSlowLifeLeachDurationModifier")]), // FUN_1009d240 @ 0x1009d240
		// second roll is (retaliationSlowManaLeachMax - retaliationSlowManaLeachMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowManaLeachDurationMin == 0.
		new(RollStore.Retaliation, 35, "RetaliationAttributeDur_ManaLeach", 20f, RollScope.All, [new("retaliationSlowManaLeachMin"), new("retaliationSlowManaLeachMax", RangeOf: "retaliationSlowManaLeachMin")], Requires: ["retaliationSlowManaLeachMin", "retaliationSlowManaLeachDurationMin"]), // FUN_1009d3c0 @ 0x1009d3c0
		new(RollStore.Retaliation, 36, "RetaliationAttributeDurMod_ManaLeach", 20f, RollScope.All, [new("retaliationSlowManaLeachModifier"), new("retaliationSlowManaLeachDurationModifier")]), // FUN_1009d540 @ 0x1009d540
		// second roll is (retaliationSlowOffensiveAbilityMax - retaliationSlowOffensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowOffensiveAbilityDurationMin == 0.
		new(RollStore.Retaliation, 37, "RetaliationAttributeDur_OffensiveAbility", 20f, RollScope.All, [new("retaliationSlowOffensiveAbilityMin"), new("retaliationSlowOffensiveAbilityMax", RangeOf: "retaliationSlowOffensiveAbilityMin")], Requires: ["retaliationSlowOffensiveAbilityMin", "retaliationSlowOffensiveAbilityDurationMin"]), // FUN_1009d6c0 @ 0x1009d6c0
		new(RollStore.Retaliation, 38, "RetaliationAttributeDurMod_OffensiveAbility", 20f, RollScope.All, [new("retaliationSlowOffensiveAbilityModifier"), new("retaliationSlowOffensiveAbilityDurationModifier")]), // FUN_1009d840 @ 0x1009d840
		// second roll is (retaliationSlowDefensiveAbilityMax - retaliationSlowDefensiveAbilityMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowDefensiveAbilityDurationMin == 0.
		new(RollStore.Retaliation, 39, "RetaliationAttributeDur_DefensiveAbility", 20f, RollScope.All, [new("retaliationSlowDefensiveAbilityMin"), new("retaliationSlowDefensiveAbilityMax", RangeOf: "retaliationSlowDefensiveAbilityMin")], Requires: ["retaliationSlowDefensiveAbilityMin", "retaliationSlowDefensiveAbilityDurationMin"]), // FUN_1009d9c0 @ 0x1009d9c0
		new(RollStore.Retaliation, 40, "RetaliationAttributeDurMod_DefensiveAbility", 20f, RollScope.All, [new("retaliationSlowDefensiveAbilityModifier"), new("retaliationSlowDefensiveAbilityDurationModifier")]), // FUN_1009db40 @ 0x1009db40
		// second roll is (retaliationSlowOffensiveReductionMax - retaliationSlowOffensiveReductionMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowOffensiveReductionDurationMin == 0.
		new(RollStore.Retaliation, 41, "RetaliationAttributeDur_OffensiveReduction", 20f, RollScope.All, [new("retaliationSlowOffensiveReductionMin"), new("retaliationSlowOffensiveReductionMax", RangeOf: "retaliationSlowOffensiveReductionMin")], Requires: ["retaliationSlowOffensiveReductionMin", "retaliationSlowOffensiveReductionDurationMin"]), // FUN_1009dcc0 @ 0x1009dcc0
		new(RollStore.Retaliation, 42, "RetaliationAttributeDurMod_OffensiveReduction", 20f, RollScope.All, [new("retaliationSlowOffensiveReductionModifier"), new("retaliationSlowOffensiveReductionDurationModifier")]), // FUN_1009de40 @ 0x1009de40
		// second roll is (retaliationStunMax - retaliationStunMin) clamped >= 0, not retaliationStunMax; no draw when max <= min or absent; no instance when retaliationStunMin == 0.
		new(RollStore.Retaliation, 43, "RetaliationAttributeReflex_Stun", 20f, RollScope.All, [new("retaliationStunMin"), new("retaliationStunMax", RangeOf: "retaliationStunMin")], Requires: ["retaliationStunMin"]), // FUN_1009dfc0 @ 0x1009dfc0
		new(RollStore.Retaliation, 44, "RetaliationAttributeAbsMod_Stun", 20f, RollScope.All, [new("retaliationStunModifier")]), // FUN_1009e130 @ 0x1009e130
		// second roll is (retaliationSlowAttackSpeedMax - retaliationSlowAttackSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowAttackSpeedDurationMin == 0.
		new(RollStore.Retaliation, 45, "RetaliationAttributeDur_AttackSpeed", 20f, RollScope.All, [new("retaliationSlowAttackSpeedMin"), new("retaliationSlowAttackSpeedMax", RangeOf: "retaliationSlowAttackSpeedMin")], Requires: ["retaliationSlowAttackSpeedMin", "retaliationSlowAttackSpeedDurationMin"]), // FUN_1009e2f0 @ 0x1009e2f0
		new(RollStore.Retaliation, 46, "RetaliationAttributeDurMod_AttackSpeed", 20f, RollScope.All, [new("retaliationSlowAttackSpeedModifier"), new("retaliationSlowAttackSpeedDurationModifier")]), // FUN_1009e470 @ 0x1009e470
		// second roll is (retaliationSlowSpellCastSpeedMax - retaliationSlowSpellCastSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowSpellCastSpeedDurationMin == 0.
		new(RollStore.Retaliation, 47, "RetaliationAttributeDur_SpellCastSpeed", 20f, RollScope.All, [new("retaliationSlowSpellCastSpeedMin"), new("retaliationSlowSpellCastSpeedMax", RangeOf: "retaliationSlowSpellCastSpeedMin")], Requires: ["retaliationSlowSpellCastSpeedMin", "retaliationSlowSpellCastSpeedDurationMin"]), // FUN_1009e5f0 @ 0x1009e5f0
		new(RollStore.Retaliation, 48, "RetaliationAttributeDurMod_SpellCastSpeed", 20f, RollScope.All, [new("retaliationSlowSpellCastSpeedModifier"), new("retaliationSlowSpellCastSpeedDurationModifier")]), // FUN_1009e770 @ 0x1009e770
		// second roll is (retaliationSlowRunSpeedMax - retaliationSlowRunSpeedMin) clamped >= 0; durations are never rolled; whole instance (no draws) is skipped when retaliationSlowRunSpeedDurationMin == 0.
		new(RollStore.Retaliation, 49, "RetaliationAttributeDur_RunSpeed", 20f, RollScope.All, [new("retaliationSlowRunSpeedMin"), new("retaliationSlowRunSpeedMax", RangeOf: "retaliationSlowRunSpeedMin")], Requires: ["retaliationSlowRunSpeedMin", "retaliationSlowRunSpeedDurationMin"]), // FUN_1009e8f0 @ 0x1009e8f0
		new(RollStore.Retaliation, 50, "RetaliationAttributeDurMod_RunSpeed", 20f, RollScope.All, [new("retaliationSlowRunSpeedModifier"), new("retaliationSlowRunSpeedDurationModifier")]), // FUN_1009ea70 @ 0x1009ea70
		new(RollStore.Defense, 1, "DefenseAttribute_BaseProtectionAbsorption", 20f, RollScope.AffixOnlyArmor, [new("defensiveProtection")]), // DefenseAttribute_BaseProtectionAbsorption::Create_BaseAbsorptionProtection @ 0x10169bd0
		new(RollStore.Defense, 2, "DefenseAttributeAbsMod_Protection", 20f, RollScope.All, [new("defensiveProtectionModifier")]), // FUN_100639f0 @ 0x100639f0
		new(RollStore.Defense, 3, "DefenseAttributeAbsMod_Absorption", 20f, RollScope.All, [new("defensiveAbsorptionModifier")]), // FUN_10063b30 @ 0x10063b30
		new(RollStore.Defense, 4, "DefenseAttributeAbs_Physical", 20f, RollScope.All, [new("defensivePhysical")]), // FUN_10063c70 @ 0x10063c70
		new(RollStore.Defense, 5, "DefenseAttributeAbsMod_Physical", 20f, RollScope.All, [new("defensivePhysicalModifier")]), // FUN_10063db0 @ 0x10063db0
		new(RollStore.Defense, 6, "DefenseAttributeDur_Physical", 20f, RollScope.All, [new("defensivePhysicalDuration")]), // FUN_10063ef0 @ 0x10063ef0
		new(RollStore.Defense, 7, "DefenseAttributeDurMod_Physical", 20f, RollScope.All, [new("defensivePhysicalDurationModifier")]), // FUN_10064030 @ 0x10064030
		new(RollStore.Defense, 8, "DefenseAttributeAbs_Pierce", 20f, RollScope.All, [new("defensivePierce")]), // FUN_10064170 @ 0x10064170
		new(RollStore.Defense, 9, "DefenseAttributeAbsMod_Pierce", 20f, RollScope.All, [new("defensivePierceModifier")]), // FUN_100642b0 @ 0x100642b0
		new(RollStore.Defense, 10, "DefenseAttributeAbs_Bleeding", 20f, RollScope.All, [new("defensiveBleeding")]), // FUN_100643f0 @ 0x100643f0
		new(RollStore.Defense, 11, "DefenseAttributeDur_Bleeding", 20f, RollScope.All, [new("defensiveBleedingDuration")]), // FUN_10064530 @ 0x10064530
		new(RollStore.Defense, 12, "DefenseAttributeAbsMod_Bleeding", 20f, RollScope.All, [new("defensiveBleedingModifier")]), // FUN_10064670 @ 0x10064670
		new(RollStore.Defense, 13, "DefenseAttributeDurMod_Bleeding", 20f, RollScope.All, [new("defensiveBleedingDurationModifier")]), // FUN_100647b0 @ 0x100647b0
		new(RollStore.Defense, 14, "DefenseAttributeAbs_ElementalResistance", 20f, RollScope.All, [new("defensiveElementalResistance")]), // FUN_100648f0 @ 0x100648f0
		new(RollStore.Defense, 15, "DefenseAttributeAbs_Fire", 20f, RollScope.All, [new("defensiveFire")]), // FUN_10064a30 @ 0x10064a30
		new(RollStore.Defense, 16, "DefenseAttributeDur_Fire", 20f, RollScope.All, [new("defensiveFireDuration")]), // FUN_10064b70 @ 0x10064b70
		new(RollStore.Defense, 17, "DefenseAttributeAbsMod_Fire", 20f, RollScope.All, [new("defensiveFireModifier")]), // FUN_10064cb0 @ 0x10064cb0
		new(RollStore.Defense, 18, "DefenseAttributeDurMod_Fire", 20f, RollScope.All, [new("defensiveFireDurationModifier")]), // FUN_10064df0 @ 0x10064df0
		new(RollStore.Defense, 19, "DefenseAttributeAbs_Cold", 20f, RollScope.All, [new("defensiveCold")]), // FUN_10064f30 @ 0x10064f30
		new(RollStore.Defense, 20, "DefenseAttributeDur_Cold", 20f, RollScope.All, [new("defensiveColdDuration")]), // FUN_10065070 @ 0x10065070
		new(RollStore.Defense, 21, "DefenseAttributeAbsMod_Cold", 20f, RollScope.All, [new("defensiveColdModifier")]), // FUN_100651b0 @ 0x100651b0
		new(RollStore.Defense, 22, "DefenseAttributeDurMod_Cold", 20f, RollScope.All, [new("defensiveColdDurationModifier")]), // FUN_100652f0 @ 0x100652f0
		new(RollStore.Defense, 23, "DefenseAttributeAbs_Lightning", 20f, RollScope.All, [new("defensiveLightning")]), // FUN_10065430 @ 0x10065430
		new(RollStore.Defense, 24, "DefenseAttributeDur_Lightning", 20f, RollScope.All, [new("defensiveLightningDuration")]), // FUN_10065570 @ 0x10065570
		new(RollStore.Defense, 25, "DefenseAttributeAbsMod_Lightning", 20f, RollScope.All, [new("defensiveLightningModifier")]), // FUN_100656b0 @ 0x100656b0
		new(RollStore.Defense, 26, "DefenseAttributeDurMod_Lightning", 20f, RollScope.All, [new("defensiveLightningDurationModifier")]), // FUN_100657f0 @ 0x100657f0
		new(RollStore.Defense, 27, "DefenseAttributeAbs_Poison", 20f, RollScope.All, [new("defensivePoison")]), // FUN_10065930 @ 0x10065930
		new(RollStore.Defense, 28, "DefenseAttributeDur_Poison", 20f, RollScope.All, [new("defensivePoisonDuration")]), // FUN_10065a70 @ 0x10065a70
		new(RollStore.Defense, 29, "DefenseAttributeAbsMod_Poison", 20f, RollScope.All, [new("defensivePoisonModifier")]), // FUN_10065bb0 @ 0x10065bb0
		new(RollStore.Defense, 30, "DefenseAttributeDurMod_Poison", 20f, RollScope.All, [new("defensivePoisonDurationModifier")]), // FUN_10065cf0 @ 0x10065cf0
		new(RollStore.Defense, 31, "DefenseAttributeAbs_Life", 20f, RollScope.All, [new("defensiveLife")]), // FUN_10065e30 @ 0x10065e30
		new(RollStore.Defense, 32, "DefenseAttributeDur_Life", 20f, RollScope.All, [new("defensiveLifeDuration")]), // FUN_10065f70 @ 0x10065f70
		new(RollStore.Defense, 33, "DefenseAttributeAbsMod_Life", 20f, RollScope.All, [new("defensiveLifeModifier")]), // FUN_100660b0 @ 0x100660b0
		new(RollStore.Defense, 34, "DefenseAttributeDurMod_Life", 20f, RollScope.All, [new("defensiveLifeDurationModifier")]), // FUN_100661f0 @ 0x100661f0
		new(RollStore.Defense, 35, "DefenseAttributeAbs_LifeLeach", 20f, RollScope.All, [new("defensiveSlowLifeLeach")]), // FUN_10066330 @ 0x10066330
		new(RollStore.Defense, 36, "DefenseAttributeDur_LifeLeach", 20f, RollScope.All, [new("defensiveSlowLifeLeachDuration")]), // FUN_10066470 @ 0x10066470
		new(RollStore.Defense, 37, "DefenseAttributeAbsMod_LifeLeach", 20f, RollScope.All, [new("defensiveSlowLifeLeachModifier")]), // FUN_100665b0 @ 0x100665b0
		new(RollStore.Defense, 38, "DefenseAttributeDurMod_LifeLeach", 20f, RollScope.All, [new("defensiveSlowLifeLeachDurationModifier")]), // FUN_100666f0 @ 0x100666f0
		new(RollStore.Defense, 39, "DefenseAttributeAbs_ManaLeach", 20f, RollScope.All, [new("defensiveSlowManaLeach")]), // FUN_10066830 @ 0x10066830
		new(RollStore.Defense, 40, "DefenseAttributeDur_ManaLeach", 20f, RollScope.All, [new("defensiveSlowManaLeachDuration")]), // FUN_10066970 @ 0x10066970
		new(RollStore.Defense, 41, "DefenseAttributeAbsMod_ManaLeach", 20f, RollScope.All, [new("defensiveSlowManaLeachModifier")]), // FUN_10066ab0 @ 0x10066ab0
		new(RollStore.Defense, 42, "DefenseAttributeDurMod_ManaLeach", 20f, RollScope.All, [new("defensiveSlowManaLeachDurationModifier")]), // FUN_10066bf0 @ 0x10066bf0
		new(RollStore.Defense, 43, "DefenseAttributeAbs_Sleep", 20f, RollScope.All, [new("defensiveSleep")]), // FUN_10066d30 @ 0x10066d30
		new(RollStore.Defense, 44, "DefenseAttributeAbsMod_Sleep", 20f, RollScope.All, [new("defensiveSleepModifier")]), // FUN_10066e70 @ 0x10066e70
		new(RollStore.Defense, 45, "DefenseAttributeAbs_Stun", 20f, RollScope.All, [new("defensiveStun")]), // FUN_10066fb0 @ 0x10066fb0
		new(RollStore.Defense, 46, "DefenseAttributeAbsMod_Stun", 20f, RollScope.All, [new("defensiveStunModifier")]), // FUN_100670f0 @ 0x100670f0
		new(RollStore.Defense, 47, "DefenseAttributeAbs_Trap", 20f, RollScope.All, [new("defensiveTrap")]), // FUN_10067230 @ 0x10067230
		new(RollStore.Defense, 48, "DefenseAttributeAbs_Freeze", 20f, RollScope.All, [new("defensiveFreeze")]), // FUN_10067370 @ 0x10067370
		new(RollStore.Defense, 49, "DefenseAttributeAbs_Petrify", 20f, RollScope.All, [new("defensivePetrify")]), // FUN_100674b0 @ 0x100674b0
		new(RollStore.Defense, 50, "DefenseAttributeAbs_Disruption", 20f, RollScope.All, [new("defensiveDisruption")]), // FUN_100675f0 @ 0x100675f0
		new(RollStore.Defense, 51, "DefenseAttributeAbs_Taunt", 20f, RollScope.All, [new("defensiveTaunt")]), // FUN_10067730 @ 0x10067730
		new(RollStore.Defense, 52, "DefenseAttributeAbs_Fear", 20f, RollScope.All, [new("defensiveFear")]), // FUN_10067870 @ 0x10067870
		new(RollStore.Defense, 53, "DefenseAttributeAbs_Confusion", 20f, RollScope.All, [new("defensiveConfusion")]), // FUN_100679b0 @ 0x100679b0
		new(RollStore.Defense, 54, "DefenseAttributeMisc_BlockModifier", 20f, RollScope.All, [new("defensiveBlockModifier")]), // FUN_10067af0 @ 0x10067af0
		new(RollStore.Defense, 55, "DefenseAttributeMisc_Reflect", 20f, RollScope.All, [new("defensiveReflect")]), // FUN_10067c30 @ 0x10067c30
		new(RollStore.Defense, 56, "DefenseAttributeAbs_TotalSpeed", 20f, RollScope.All, [new("defensiveTotalSpeedResistance")]), // FUN_10067d70 @ 0x10067d70
	];
}
