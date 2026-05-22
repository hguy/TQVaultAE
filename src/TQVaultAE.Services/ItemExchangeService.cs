using System.Text;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public class ItemExchangeService : IItemExchangeService
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly IVaultService _vaultService;
	private readonly IUIService _uiService;
	private readonly IPasteBinService _pasteBinService;
	private readonly UserSettings _userSettings;

	public ItemExchangeService(
		JsonSerializerOptions jsonOptions,
		IVaultService vaultService = null,
		IUIService uiService = null,
		IPasteBinService pasteBinService = null,
		UserSettings userSettings = null)
	{
		_jsonOptions = jsonOptions;
		_vaultService = vaultService;
		_uiService = uiService;
		_pasteBinService = pasteBinService;
		_userSettings = userSettings;
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

	public string SerializeSackCollection(SackCollection sack, int sackNumber)
	{
		var dto = TabExportDTO.FromSackCollection(sack, sackNumber);
		var envelope = new ExportFormat
		{
			FormatVersion = 1,
			Scope = "tab",
			Data = dto
		};

		return JsonSerializer.Serialize(envelope, _jsonOptions);
	}

	public string SerializePlayerCollection(PlayerCollection vault)
	{
		var dto = VaultExportDTO.FromPlayerCollection(vault);
		var envelope = new ExportFormat
		{
			FormatVersion = 1,
			Scope = "vault",
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

			if (scopeValue == "tab")
			{
				if (!root.TryGetProperty("data", out var dataElement))
					return ImportResult.Failed("Missing data in export format.");

				var dto = JsonSerializer.Deserialize<TabExportDTO>(
					dataElement.GetRawText(),
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true });

				if (dto == null)
					return ImportResult.Failed("Failed to deserialize tab data.");

				var items = new List<Item>();
				foreach (var itemDto in dto.Items)
					items.Add(itemDto.ToItem());

				return ImportResult.SucceededTab(items, dto.SackNumber, dto.SackType);
			}

			if (scopeValue == "vault")
			{
				if (!root.TryGetProperty("data", out var dataElement))
					return ImportResult.Failed("Missing data in export format.");

				var dto = JsonSerializer.Deserialize<VaultExportDTO>(
					dataElement.GetRawText(),
					new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IncludeFields = true });

				if (dto == null)
					return ImportResult.Failed("Failed to deserialize vault data.");

				var sackItems = new Dictionary<int, List<Item>>();
				foreach (var sackDto in dto.Sacks)
				{
					var itemList = new List<Item>();
					foreach (var itemDto in sackDto.Items)
						itemList.Add(itemDto.ToItem());

					sackItems[sackDto.SackNumber] = itemList;
				}

				return ImportResult.SucceededVault(dto.Name, sackItems);
			}

			return ImportResult.Failed($"Unsupported scope: {scopeValue}");
		}
		catch (JsonException ex)
		{
			return ImportResult.Failed($"Invalid JSON format: {ex.Message}");
		}
	}

	public string DetectScope(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
			return null;

		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (root.TryGetProperty("scope", out var scope))
				return scope.GetString();
			return null;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public void ImportVaultInto(PlayerCollection vault, ImportResult importData)
	{
		if (vault == null || importData?.SackItems == null)
			return;

		// Clear all tabs
		for (int i = 0; i < vault.NumberOfSacks; i++)
		{
			var sack = vault.GetSack(i);
			sack?.items?.Clear();
			if (sack != null)
				sack.IsModified = true;
		}

		// Import items into the correct sacks
		foreach (var kvp in importData.SackItems)
		{
			int sackNumber = kvp.Key;
			if (sackNumber < 0 || sackNumber >= vault.NumberOfSacks)
				continue;

			var sack = vault.GetSack(sackNumber);
			if (sack == null)
				continue;

			foreach (var item in kvp.Value)
			{
				sack.AddItem(item);
			}
		}
	}

	public bool IsPasteBinUrl(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return false;

		return text.StartsWith("https://pastebin.com/", StringComparison.OrdinalIgnoreCase);
	}

	public bool HasPasteBinApiKey
		=> !string.IsNullOrWhiteSpace(_userSettings?.PasteBinApiKey);

	public async Task<string> ExportToPasteBinAsync(string json)
	{
		if (_pasteBinService == null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		var payload = EncodeToClipboardPayload(json);
		return await _pasteBinService.UploadAsync(payload);
	}

	public async Task<string> ImportFromPasteBinAsync(string pasteUrl)
	{
		if (_pasteBinService == null)
			throw new InvalidOperationException("PasteBin service is not configured.");

		var payload = await _pasteBinService.FetchPasteAsync(pasteUrl);
		return DecodeFromClipboardPayload(payload);
	}
}
