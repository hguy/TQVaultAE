using System.Text.Json.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Minimal dataset for item interchange. Matches scope of <see cref="Data.Dto.ItemDto"/>.
/// Hydration (width/height, info objects) deferred to <see cref="Contracts.Providers.IItemProvider.GetDBData"/>.
/// </summary>
public class ItemExportDTO
{
	[JsonPropertyName("stackSize")]
	public int StackSize { get; set; }

	[JsonPropertyName("seed")]
	public int Seed { get; set; }

	[JsonPropertyName("baseItemId")]
	public string BaseItemId { get; set; }

	[JsonPropertyName("prefixId")]
	public string PrefixId { get; set; }

	[JsonPropertyName("suffixId")]
	public string SuffixId { get; set; }

	[JsonPropertyName("relicId")]
	public string RelicId { get; set; }

	[JsonPropertyName("relicBonusId")]
	public string RelicBonusId { get; set; }

	[JsonPropertyName("var1")]
	public int Var1 { get; set; }

	[JsonPropertyName("relic2Id")]
	public string Relic2Id { get; set; }

	[JsonPropertyName("relicBonus2Id")]
	public string RelicBonus2Id { get; set; }

	[JsonPropertyName("var2")]
	public int Var2 { get; set; }

	[JsonPropertyName("positionX")]
	public int PositionX { get; set; }

	[JsonPropertyName("positionY")]
	public int PositionY { get; set; }

	public static ItemExportDTO FromItem(Item item)
		=> new()
		{
			StackSize = item.StackSize,
			Seed = item.Seed,
			BaseItemId = item.BaseItemId?.Raw,
			PrefixId = item.prefixID?.Raw,
			SuffixId = item.suffixID?.Raw,
			RelicId = item.relicID?.Raw,
			RelicBonusId = item.RelicBonusId?.Raw,
			Var1 = item.Var1,
			Relic2Id = item.relic2ID?.Raw,
			RelicBonus2Id = item.RelicBonus2Id?.Raw,
			Var2 = item.Var2,
			PositionX = item.PositionX,
			PositionY = item.PositionY
		};

	public Item ToItem()
	{
		var item = new Item
		{
			StackSize = this.StackSize,
			Seed = this.Seed,
			BaseItemId = this.BaseItemId,
			prefixID = this.PrefixId,
			suffixID = this.SuffixId,
			relicID = this.RelicId,
			RelicBonusId = this.RelicBonusId,
			Var1 = this.Var1,
			PositionX = this.PositionX,
			PositionY = this.PositionY,
			endBlockCrap2 = 0,
			endBlockCrap1 = 0
		};

		if (!string.IsNullOrWhiteSpace(this.Relic2Id))
		{
			item.atlantis = true;
			item.relic2ID = this.Relic2Id;
			item.RelicBonus2Id = this.RelicBonus2Id;
			item.Var2 = this.Var2;
		}
		else
		{
			item.relic2ID = RecordId.Empty;
			item.RelicBonus2Id = RecordId.Empty;
			item.Var2 = Item.var2Default;
		}

		return item;
	}
}