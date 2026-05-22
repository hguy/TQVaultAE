using System.Text.Json.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.DTOs;

public class ItemExportDTO
{
	[JsonPropertyName("baseItemId")]
	public string BaseItemId { get; set; }

	[JsonPropertyName("prefixId")]
	public string PrefixId { get; set; }

	[JsonPropertyName("suffixId")]
	public string SuffixId { get; set; }

	[JsonPropertyName("relicId")]
	public string RelicId { get; set; }

	[JsonPropertyName("relic2Id")]
	public string Relic2Id { get; set; }

	[JsonPropertyName("relicBonusId")]
	public string RelicBonusId { get; set; }

	[JsonPropertyName("relicBonus2Id")]
	public string RelicBonus2Id { get; set; }

	[JsonPropertyName("seed")]
	public int Seed { get; set; }

	[JsonPropertyName("positionX")]
	public int PositionX { get; set; }

	[JsonPropertyName("positionY")]
	public int PositionY { get; set; }

	[JsonPropertyName("stackSize")]
	public int StackSize { get; set; }

	[JsonPropertyName("width")]
	public int Width { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }

	[JsonPropertyName("var1")]
	public int Var1 { get; set; }

	[JsonPropertyName("var2")]
	public int Var2 { get; set; }

	[JsonPropertyName("itemScalePercent")]
	public float ItemScalePercent { get; set; }

	[JsonPropertyName("atlantis")]
	public bool Atlantis { get; set; }

	[JsonPropertyName("beginBlockCrap1")]
	public int BeginBlockCrap1 { get; set; }

	[JsonPropertyName("endBlockCrap1")]
	public int EndBlockCrap1 { get; set; }

	[JsonPropertyName("beginBlockCrap2")]
	public int BeginBlockCrap2 { get; set; }

	[JsonPropertyName("endBlockCrap2")]
	public int EndBlockCrap2 { get; set; }

	[JsonPropertyName("attributeCount")]
	public int AttributeCount { get; set; }

	public static ItemExportDTO FromItem(Item item)
		=> new()
		{
			BaseItemId = item.BaseItemId?.Raw,
			PrefixId = item.prefixID?.Raw,
			SuffixId = item.suffixID?.Raw,
			RelicId = item.relicID?.Raw,
			Relic2Id = item.relic2ID?.Raw,
			RelicBonusId = item.RelicBonusId?.Raw,
			RelicBonus2Id = item.RelicBonus2Id?.Raw,
			Seed = item.Seed,
			PositionX = item.PositionX,
			PositionY = item.PositionY,
			StackSize = item.StackSize,
			Width = item.Width,
			Height = item.Height,
			Var1 = item.Var1,
			Var2 = item.Var2,
			ItemScalePercent = item.itemScalePercent,
			Atlantis = item.atlantis,
			BeginBlockCrap1 = item.beginBlockCrap1,
			EndBlockCrap1 = item.endBlockCrap1,
			BeginBlockCrap2 = item.beginBlockCrap2,
			EndBlockCrap2 = item.endBlockCrap2,
			AttributeCount = item.attributeCount
		};

	public Item ToItem()
		=> new()
		{
			BaseItemId = this.BaseItemId,
			prefixID = this.PrefixId,
			suffixID = this.SuffixId,
			relicID = this.RelicId,
			relic2ID = this.Relic2Id,
			RelicBonusId = this.RelicBonusId,
			RelicBonus2Id = this.RelicBonus2Id,
			Seed = this.Seed,
			PositionX = this.PositionX,
			PositionY = this.PositionY,
			StackSize = this.StackSize,
			Width = this.Width,
			Height = this.Height,
			Var1 = this.Var1,
			Var2 = this.Var2,
			itemScalePercent = this.ItemScalePercent,
			atlantis = this.Atlantis,
			beginBlockCrap1 = this.BeginBlockCrap1,
			endBlockCrap1 = this.EndBlockCrap1,
			beginBlockCrap2 = this.BeginBlockCrap2,
			endBlockCrap2 = this.EndBlockCrap2,
			attributeCount = this.AttributeCount
		};
}