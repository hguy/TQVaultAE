using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TQVaultAE.Application.DTOs;

public class VaultExportDTO
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("sacks")]
	public IReadOnlyList<SackExportDTO> Sacks { get; set; }

	public static VaultExportDTO FromPlayerCollection(PlayerCollection vault)
	{
		var sacks = new List<SackExportDTO>();
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			if (sack == null || sack.IsEmpty)
				continue;

			var items = new List<ItemExportDTO>();
			foreach (var item in sack)
				items.Add(ItemExportDTO.FromItem(item));

			sacks.Add(new SackExportDTO
			{
				SackNumber = i,
				Items = items
			});
		}

		return new VaultExportDTO
		{
			Name = vault.PlayerName,
			Sacks = sacks
		};
	}
}

public class SackExportDTO
{
	[JsonPropertyName("sackNumber")]
	public int SackNumber { get; set; }

	[JsonPropertyName("items")]
	public IReadOnlyList<ItemExportDTO> Items { get; set; }
}