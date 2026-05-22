using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TQVaultAE.Application.Contracts.Services;
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
		doc.RootElement.GetProperty("scope").GetString().Should().Be("item");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("baseItemId").GetString().Should().Be("records/gear/armor/test.dbr");
		data.GetProperty("seed").GetInt32().Should().Be(12345);
		data.GetProperty("positionX").GetInt32().Should().Be(5);
		data.GetProperty("positionY").GetInt32().Should().Be(10);
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
		result.Scope.Should().Be("item");
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
}