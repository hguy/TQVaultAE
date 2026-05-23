using System.Text.Json.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.DTOs;

public class TabExportDTO
{
	[JsonPropertyName("sackNumber")]
	public int SackNumber { get; set; }

	[JsonPropertyName("sackType")]
	public string SackType { get; set; }

	[JsonPropertyName("iconInfo")]
	public BagButtonIconInfo IconInfo { get; set; }

	[JsonPropertyName("items")]
	public IReadOnlyList<ItemExportDTO> Items { get; set; }

	public static TabExportDTO FromSackCollection(SackCollection sack, int sackNumber)
	{
		var items = new List<ItemExportDTO>();
		foreach (var item in sack)
			items.Add(ItemExportDTO.FromItem(item));

		return new TabExportDTO
		{
			SackNumber = sackNumber,
			SackType = sack.SackType.ToString(),
			IconInfo = sack.BagButtonIconInfo,
			Items = items
		};
	}
}