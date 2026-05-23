using System.Text.Json.Serialization;

namespace TQVaultAE.Application.DTOs;

public enum ExportScope
{
	Item,
	Tab,
	Vault
}

public class ExportFormat
{
	[JsonPropertyName("formatVersion")]
	public int FormatVersion { get; set; }

	[JsonPropertyName("scope")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public ExportScope Scope { get; set; }

	[JsonPropertyName("data")]
	public object Data { get; set; }
}