//-----------------------------------------------------------------------
// <copyright file="EquipmentRollTable.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.Domain.Jitter;

/// <summary>
/// Equipment attribute stores, in the order <c>ItemEquipment::InitializeItem</c> (Game.dll 0x101bf4a0) loads them.
/// </summary>
public enum RollStore
{
	/// <summary><c>CharAttributeStore_Equipment</c>: affixes roll through <c>LoadPrefixTable</c> (always, uncapped).</summary>
	Character,

	/// <summary><c>SkillAttributeStore_Equipment</c>: same rules as <see cref="Character"/>.</summary>
	Skill,

	/// <summary><c>DamageAttributeStore_Equipment</c>: every instance rolls through <c>AddJitter</c>.</summary>
	Damage,

	/// <summary><c>RetaliationAttributeStore_Equipment</c>: same rules as <see cref="Damage"/>.</summary>
	Retaliation,

	/// <summary><c>DefenseAttributeStore_Equipment</c>: same rules as <see cref="Damage"/>.</summary>
	Defense,
}

/// <summary>
/// Which record instances of an attribute consume RNG draws.
/// </summary>
public enum RollScope
{
	/// <summary>Base item, prefix and suffix instances all roll (the normal case).</summary>
	All,

	/// <summary>Only prefix/suffix instances roll, at the attribute's base percent, and only on armor items
	/// (<c>DefenseAttribute_BaseProtectionAbsorption</c>: base armor is static).</summary>
	AffixOnlyArmor,

	/// <summary><c>AddJitter</c> is a no-op for this class (e.g. <c>DamageAttributeAbsBase</c>, level requirement reduction).</summary>
	None,
}

/// <summary>
/// Which items load an attribute (<c>byte [item+0x628]</c>, set by <c>Armor::Armor</c>).
/// </summary>
public enum RollItemFilter
{
	/// <summary>Every equipment item.</summary>
	Any,

	/// <summary>Only <c>Armor</c> items (helms, torsos, arms, legs).</summary>
	ArmorOnly,

	/// <summary>Every equipment item except <c>Armor</c> (weapons, shields, jewellery).</summary>
	NonArmorOnly,
}

/// <summary>
/// One value rolled by an attribute's jitter, in engine order.
/// </summary>
/// <param name="Variable">Database variable name (case-insensitive).</param>
/// <param name="DefaultFrom">When the variable is absent or zero, the loader copies this variable's value first.</param>
/// <param name="RoundToInt">The engine truncates the value to an integer before and after the roll (<c>DamageAttribute_Physical</c> range).</param>
/// <param name="RangeOf">
/// Damage pairs are stored as (min, range): this field is the max variable, but the engine rolls
/// <c>max(0, max - min)</c> with <paramref name="RangeOf"/> the min variable, and the in-game max is
/// <c>rolled min + rolled range</c>. No draw when max is absent or not above min (<c>GetValueMax</c> 0x10155260).
/// </param>
/// <param name="Scales">
/// <see langword="false"/> when the class's <c>ScaleAttribute</c> leaves this value alone
/// (e.g. <c>DamageAttribute_Physical</c> pierce ratio). For <paramref name="RangeOf"/> fields only the min is
/// scaled and the range is kept, so the max moves by the min's scaling.
/// </param>
public sealed record RollField(string Variable, string? DefaultFrom = null, bool RoundToInt = false, string? RangeOf = null, bool Scales = true);

/// <summary>
/// One attribute loader call inside a store's <c>Load</c>, in call order.
/// </summary>
/// <param name="Store">Owning store.</param>
/// <param name="Order">1-based call index inside the store's <c>Load</c>.</param>
/// <param name="EngineClass">Game.dll attribute class, for traceability.</param>
/// <param name="BaseJitterPercent">Percent the store passes to the base instance's <c>AddJitter</c> (0 = base never rolls).</param>
/// <param name="Scope">Which instances roll.</param>
/// <param name="Fields">Values rolled per instance, in order.</param>
/// <param name="Requires">Variables that must all be non-zero in a record for its instance to exist
/// (<c>LoadFromTable</c> returns false otherwise and nothing rolls), e.g. the min of a damage pair.</param>
/// <param name="Items">Which items load the attribute at all.</param>
/// <param name="Scales"><see langword="false"/> when the class's <c>ScaleAttribute</c> is a no-op
/// (<c>CharAttributeValSpeed</c>, <c>CharAttributeVal_LevelReqReduction</c>, <c>CharAttributeVal_BaseAttackSpeed</c>):
/// the base item's <c>attributeScalePercent</c> does not apply.</param>
public sealed record RollAttribute(RollStore Store, int Order, string EngineClass, float BaseJitterPercent, RollScope Scope, RollField[] Fields, string[]? Requires = null, RollItemFilter Items = RollItemFilter.Any, bool Scales = true);

/// <summary>
/// Roll order of every equipment attribute, extracted from Game.dll
/// (SHA-256 754907eacf552656945ff9eaf1763630e138506517e91b698ab28a0c3186aa86).
/// The data lives in <c>EquipmentRollTable.Data.cs</c>.
/// </summary>
public static partial class EquipmentRollTable
{
	/// <summary>
	/// All attributes, ordered by <see cref="RollStore"/> then <see cref="RollAttribute.Order"/>.
	/// </summary>
	public static IReadOnlyList<RollAttribute> Attributes { get; } = BuildOrdered();

	private static RollAttribute[] BuildOrdered()
		=> CreateAttributes().OrderBy(a => a.Store).ThenBy(a => a.Order).ToArray();

	private static partial RollAttribute[] CreateAttributes();
}
