using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Forge: the rules with the item level requirements, the parts offered in God mode, and the forge itself.
/// </summary>
public interface IForgeService
{
	/// <summary>Creates the forge rules, the level requirements read from the item records.</summary>
	ForgeRules CreateRules();

	/// <summary>
	/// The parts of <paramref name="item"/> usable in God mode: the base item first, then the prefix, suffix and
	/// relics it has.
	/// </summary>
	/// <param name="item">A material item.</param>
	IReadOnlyList<ForgePart> GodModeParts(Item item);

	/// <summary>
	/// Builds the item the forge would make, for its tooltip: a copy of the base item with the parts of the material
	/// items merged in, its record data read again. The selection is left as it is.
	/// </summary>
	/// <param name="selection">The items placed in the slots.</param>
	/// <param name="godMode">God mode: the parts are taken from <paramref name="godPart"/>.</param>
	/// <param name="godPart">God mode: the part of the material item of a slot.</param>
	/// <returns>The preview, a plain copy of the base item without material, or null without a base item.</returns>
	Item? Preview(ForgeSelection selection, bool godMode, Func<ForgeSlot, ForgePart?> godPart);

	/// <summary>
	/// Forges <paramref name="selection"/>: merges the material items into the base item, marks it and its sack
	/// modified, consumes the material items in hardcore mode, and replaces the search entries of the items changed.
	/// </summary>
	/// <param name="rules">Rules checking that the selection can be forged.</param>
	/// <param name="selection">The items placed in the slots.</param>
	/// <param name="sacks">The sacks the items of each slot come from.</param>
	/// <param name="godMode">God mode: the parts are taken from <paramref name="godPart"/>.</param>
	/// <param name="godPart">God mode: the part of the material item of a slot.</param>
	/// <param name="hardcore">The material items are removed from their sacks.</param>
	/// <returns>The refusal, or the forged and consumed items, whose caches must be refreshed.</returns>
	ForgeResult Forge(
		ForgeRules rules
		, ForgeSelection selection
		, IReadOnlyDictionary<ForgeSlot, SackCollection?> sacks
		, bool godMode
		, Func<ForgeSlot, ForgePart?> godPart
		, bool hardcore);
}
