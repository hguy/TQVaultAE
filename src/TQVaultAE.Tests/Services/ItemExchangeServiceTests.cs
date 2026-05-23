using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text.Json;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class ItemExchangeServiceTests
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceTests()
	{
		_jsonOptions = new JsonSerializerOptions
		{
			IncludeFields = true,
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		_service = new ItemExchangeService(_jsonOptions);
	}

	[Fact]
	public void SerializeItem_ShouldProduceValidExportFormat()
	{
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			prefixID = "records/affixes/prefix/test_prefix.dbr",
			suffixID = "records/affixes/suffix/test_suffix.dbr",
			Seed = 12345,
			PositionX = 5,
			PositionY = 10,
			StackSize = 1,
			Width = 2,
			Height = 3,
			Var1 = 0,
			Var2 = 2035248,
			itemScalePercent = 1.0F
		};

		var json = _service.SerializeItem(item);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Item");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("baseName").GetString().Should().Be("records/gear/armor/test.dbr");
		data.GetProperty("seed").GetInt32().Should().Be(12345);
		data.GetProperty("pointX").GetInt32().Should().Be(5);
		data.GetProperty("pointY").GetInt32().Should().Be(10);
	}

	[Fact]
	public void EncodeToClipboard_ShouldProduceBase64String()
	{
		var json = "{\"test\":\"value\"}";

		var encoded = _service.EncodeToClipboardPayload(json);

		encoded.Should().NotBeNullOrEmpty();
		var decoded = _service.DecodeFromClipboardPayload(encoded);
		decoded.Should().Be(json);
	}

	[Fact]
	public void ImportFromJson_WithValidItemJson_ShouldReturnImportResult()
	{
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			prefixID = "records/affixes/prefix/test_prefix.dbr",
			Seed = 12345,
			PositionX = 5,
			PositionY = 10,
			Width = 2,
			Height = 3
		};

		var json = _service.SerializeItem(item);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Scope.Should().Be(ExportScope.Item);
		result.Item.Should().NotBeNull();
		result.Item.BaseItemId.Should().Be(item.BaseItemId);
		result.Item.Seed.Should().Be(item.Seed);
	}

	[Fact]
	public void IsPasteBinUrl_WithValidUrl_ShouldReturnTrue()
	{
		_service.IsPasteBinUrl("https://pastebin.com/abc123").Should().BeTrue();
		_service.IsPasteBinUrl("https://pastebin.com/ABC123").Should().BeTrue();
	}

	[Fact]
	public void IsPasteBinUrl_WithInvalidUrl_ShouldReturnFalse()
	{
		_service.IsPasteBinUrl("some random text").Should().BeFalse();
		_service.IsPasteBinUrl("https://google.com").Should().BeFalse();
		_service.IsPasteBinUrl(string.Empty).Should().BeFalse();
	}

	[Fact]
	public void ImportFromJson_WithInvalidJson_ShouldReturnFailure()
	{
		var result = _service.ImportFromJson("not valid json");

		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
	}

	[Fact]
	public void SerializeSackCollection_ShouldProduceValidTabScopeExportFormat()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/armor/helm.dbr",
			Seed = 42,
			PositionX = 3,
			PositionY = 7,
			Width = 2,
			Height = 2
		});
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/weapon/sword.dbr",
			Seed = 99,
			PositionX = 10,
			PositionY = 15,
			Width = 1,
			Height = 3
		});

		var json = _service.SerializeSackCollection(sack, 2);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("Tab");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("sackNumber").GetInt32().Should().Be(2);

		var items = data.GetProperty("items");
		items.GetArrayLength().Should().Be(2);
		items[0].GetProperty("baseName").GetString().Should().Be("records/gear/armor/helm.dbr");
		items[1].GetProperty("baseName").GetString().Should().Be("records/gear/weapon/sword.dbr");
	}

	[Fact]
	public void ImportFromJson_WithValidTabJson_ShouldReturnMultipleItems()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/armor/helm.dbr",
			Seed = 42,
			PositionX = 3,
			PositionY = 7,
			Width = 2,
			Height = 2
		});
		sack.AddItem(new Item
		{
			BaseItemId = "records/gear/weapon/sword.dbr",
			Seed = 99,
			PositionX = 10,
			PositionY = 15,
			Width = 1,
			Height = 3
		});

		var json = _service.SerializeSackCollection(sack, 2);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().NotBeNull();
		result.Items.Should().HaveCount(2);
		result.ImportedCount.Should().Be(2);
		result.TotalCount.Should().Be(2);
		result.Items[0].BaseItemId.Should().Be("records/gear/armor/helm.dbr");
		result.Items[0].PositionX.Should().Be(3);
		result.Items[0].PositionY.Should().Be(7);
		result.Items[1].BaseItemId.Should().Be("records/gear/weapon/sword.dbr");
		result.Items[1].PositionX.Should().Be(10);
		result.Items[1].PositionY.Should().Be(15);
	}

[Fact]
	public void ImportResult_TabScope_ShouldHaveCorrectCounts()
	{
		var items = new List<Item>
		{
			new Item { BaseItemId = "a.dbr" },
			new Item { BaseItemId = "b.dbr" },
			new Item { BaseItemId = "c.dbr" }
		};

		var result = ImportResult.SucceededTab(items, 0);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().HaveCount(3);
		result.ImportedCount.Should().Be(3);
		result.TotalCount.Should().Be(3);
	}

	[Fact]
	public void ImportFromJson_WithEmptyTab_ShouldReturnNoItems()
	{
		var sack = new SackCollection();
		sack.SackType = SackType.Vault;

		var json = _service.SerializeSackCollection(sack, 0);
		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Scope.Should().Be(ExportScope.Tab);
		result.Items.Should().NotBeNull();
		result.Items.Should().BeEmpty();
	}

	[Fact]
	public void ImportFromJson_WithUnsupportedScope_ShouldReturnFailure()
	{
		var json = "{\"formatVersion\":1,\"scope\":\"unknown\",\"data\":{}}";

		var result = _service.ImportFromJson(json);

		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("Unsupported scope");
	}
}