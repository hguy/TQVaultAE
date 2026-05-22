using System.Collections.Generic;
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
	public BagButtonIconInfoDTO IconInfo { get; set; }

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
			IconInfo = BagButtonIconInfoDTO.FromIconInfo(sack.BagButtonIconInfo),
			Items = items
		};
	}
}

public class BagButtonIconInfoDTO
{
	[JsonPropertyName("mode")]
	public string DisplayMode { get; set; }

	[JsonPropertyName("label")]
	public string Label { get; set; }

	[JsonPropertyName("on")]
	public string On { get; set; }

	[JsonPropertyName("off")]
	public string Off { get; set; }

	[JsonPropertyName("over")]
	public string Over { get; set; }

	public static BagButtonIconInfoDTO FromIconInfo(BagButtonIconInfo info)
	{
		if (info is null)
			return null;

		return new BagButtonIconInfoDTO
		{
			DisplayMode = info.DisplayMode.ToString(),
			Label = info.Label,
			On = info.OnStr,
			Off = info.OffStr,
			Over = info.OverStr
		};
	}
}