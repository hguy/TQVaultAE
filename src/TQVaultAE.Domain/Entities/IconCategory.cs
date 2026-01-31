using System.Text.Json.Serialization;

namespace TQVaultAE.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IconCategory
{
	Misc,
	Artifacts,
	Relics,
	Jewellery,
	Potions,
	Scrolls,
	Skills,
	Buttons,
	Helmets,
	Shields,
	Armbands,
	Greaves,
}