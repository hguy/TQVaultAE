using System.Text.Json.Serialization;

namespace TQVaultAE.Application.DTOs;

public class ExportFormat
{
	[JsonPropertyName("formatVersion")]
	public int FormatVersion { get; set; }

	[JsonPropertyName("scope")]
	public string Scope { get; set; }

	[JsonPropertyName("data")]
	public object Data { get; set; }
}