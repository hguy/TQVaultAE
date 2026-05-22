using System.Text;
using System.Text.Json;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class ItemExchangeService : IItemExchangeService
{
	private readonly JsonSerializerOptions _jsonOptions;

	public ItemExchangeService(JsonSerializerOptions jsonOptions)
	{
		_jsonOptions = jsonOptions;
	}

	public string SerializeItem(Item item)
	{
		var dto = ItemExportDTO.FromItem(item);
		var envelope = new ExportFormat
		{
			FormatVersion = 1,
			Scope = "item",
			Data = dto
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string EncodeToClipboardPayload(string json)
	{
		var bytes = Encoding.UTF8.GetBytes(json);
		return Convert.ToBase64String(bytes);
	}

	public string DecodeFromClipboardPayload(string base64)
	{
		var bytes = Convert.FromBase64String(base64);
		return Encoding.UTF8.GetString(bytes);
	}

	public ImportResult ImportFromJson(string json)
	{
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (!root.TryGetProperty("formatVersion", out var fv) || fv.GetInt32() != 1)
				return ImportResult.Failed("Invalid or unsupported export format.");

			if (!root.TryGetProperty("scope", out var scope))
				return ImportResult.Failed("Missing scope in export format.");

			var scopeValue = scope.GetString();

			if (scopeValue == "item")
			{
				if (!root.TryGetProperty("data", out var dataElement))
					return ImportResult.Failed("Missing data in export format.");

				var dto = JsonSerializer.Deserialize<ItemExportDTO>(
					dataElement.GetRawText(),
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true });

				if (dto == null)
					return ImportResult.Failed("Failed to deserialize item data.");

				return ImportResult.Succeeded(dto.ToItem());
			}

			return ImportResult.Failed($"Unsupported scope: {scopeValue}");
		}
		catch (JsonException ex)
		{
			return ImportResult.Failed($"Invalid JSON format: {ex.Message}");
		}
	}

	public bool IsPasteBinUrl(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return false;

		return text.StartsWith("https://pastebin.com/", StringComparison.OrdinalIgnoreCase);
	}
}