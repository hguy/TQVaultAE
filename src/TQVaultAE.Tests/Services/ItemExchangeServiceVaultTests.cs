using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Text.Json;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class ItemExchangeServiceVaultTests
{
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly Mock<IVaultService> _mockVaultService;
	private readonly Mock<IUIService> _mockUIService;
	private readonly ItemExchangeService _service;

	public ItemExchangeServiceVaultTests()
	{
		_jsonOptions = new JsonSerializerOptions
		{
			IncludeFields = true,
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		_mockVaultService = new Mock<IVaultService>();
		_mockUIService = new Mock<IUIService>();
		_service = new ItemExchangeService(_jsonOptions, _mockVaultService.Object, _mockUIService.Object);
	}

	[Fact]
	public void SerializePlayerCollection_ShouldProduceValidExportFormat()
	{
		var vault = new PlayerCollection("TestVault", "test.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 12345,
			PositionX = 2,
			PositionY = 3,
			Width = 2,
			Height = 3
		};
		vault.Sacks[0].AddItem(item);

		var json = _service.SerializePlayerCollection(vault);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("vault");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("name").GetString().Should().Be("TestVault");

		var sacks = data.GetProperty("sacks");
		sacks.GetArrayLength().Should().Be(1);
		sacks[0].GetProperty("sackNumber").GetInt32().Should().Be(0);
		sacks[0].GetProperty("items").GetArrayLength().Should().Be(1);
	}

	[Fact]
	public void SerializeSackCollection_ShouldProduceValidExportFormat()
	{
		var sack = new SackCollection { SackType = SackType.Vault };
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 54321,
			PositionX = 5,
			PositionY = 7,
			Width = 2,
			Height = 3
		};
		sack.AddItem(item);

		var json = _service.SerializeSackCollection(sack, 3);

		json.Should().NotBeNullOrEmpty();

		var doc = JsonDocument.Parse(json);
		doc.RootElement.GetProperty("formatVersion").GetInt32().Should().Be(1);
		doc.RootElement.GetProperty("scope").GetString().Should().Be("tab");

		var data = doc.RootElement.GetProperty("data");
		data.GetProperty("sackNumber").GetInt32().Should().Be(3);
		data.GetProperty("sackType").GetString().Should().Be("Vault");
		data.GetProperty("items").GetArrayLength().Should().Be(1);
	}

	[Fact]
	public void ImportFromJson_WithVaultScope_ShouldReturnVaultImportResult()
	{
		var vault = new PlayerCollection("ImportedVault", "imported.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 12345,
			PositionX = 2,
			PositionY = 3,
			Width = 2,
			Height = 3
		};
		vault.Sacks[0].AddItem(item);

		var json = _service.SerializePlayerCollection(vault);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be("vault");
		result.VaultName.Should().Be("ImportedVault");
		result.SackItems.Should().NotBeNull();
		result.SackItems.Should().ContainKey(0);
		result.SackItems[0].Should().HaveCount(1);
		result.SackItems[0][0].BaseItemId.Should().Be(item.BaseItemId);
	}

	[Fact]
	public void ImportFromJson_WithTabScope_ShouldReturnTabImportResult()
	{
		var sack = new SackCollection { SackType = SackType.Vault };
		var item = new Item
		{
			BaseItemId = "records/gear/armor/test.dbr",
			Seed = 54321,
			PositionX = 5,
			PositionY = 7,
			Width = 2,
			Height = 3
		};
		sack.AddItem(item);

		var json = _service.SerializeSackCollection(sack, 3);
		var result = _service.ImportFromJson(json);

		result.Success.Should().BeTrue();
		result.Scope.Should().Be("tab");
		result.SackNumber.Should().Be(3);
		result.Items.Should().HaveCount(1);
		result.Items[0].BaseItemId.Should().Be(item.BaseItemId);
	}

	[Fact]
	public void DetectScope_WithItemJson_ShouldReturnItem()
	{
		var item = new Item { BaseItemId = "records/gear/armor/test.dbr" };
		var json = _service.SerializeItem(item);
		var scope = _service.DetectScope(json);
		scope.Should().Be("item");
	}

	[Fact]
	public void DetectScope_WithVaultJson_ShouldReturnVault()
	{
		var vault = new PlayerCollection("TestVault", "test.vault") { IsVault = true };
		vault.CreateEmptySacks(12);
		var json = _service.SerializePlayerCollection(vault);
		var scope = _service.DetectScope(json);
		scope.Should().Be("vault");
	}

	[Fact]
	public void DetectScope_WithTabJson_ShouldReturnTab()
	{
		var sack = new SackCollection { SackType = SackType.Vault };
		sack.AddItem(new Item { BaseItemId = "records/gear/armor/test.dbr" });
		var json = _service.SerializeSackCollection(sack, 0);
		var scope = _service.DetectScope(json);
		scope.Should().Be("tab");
	}

	[Fact]
	public void DetectScope_WithInvalidJson_ShouldReturnNull()
	{
		var scope = _service.DetectScope("not valid json");
		scope.Should().BeNull();
	}

	[Fact]
	public void ImportVaultInto_ShouldClearAllTabsAndImportItems()
	{
		var vault = new PlayerCollection("TargetVault", "target.vault") { IsVault = true };
		vault.CreateEmptySacks(12);

		var existingItem = new Item { BaseItemId = "records/old.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 };
		vault.Sacks[0].AddItem(existingItem);
		vault.Sacks[0].Count.Should().Be(1);// existing

		var importedItems = new Dictionary<int, List<Item>>
		{
			[0] = new()
			{
				new Item { BaseItemId = "records/gear/new1.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 },
				new Item { BaseItemId = "records/gear/new2.dbr", PositionX = 1, PositionY = 0, Width = 1, Height = 1 }
			},
			[1] = new()
			{
				new Item { BaseItemId = "records/gear/new3.dbr", PositionX = 0, PositionY = 0, Width = 1, Height = 1 }
			}
		};

		var importResult = ImportResult.SucceededVault("Imported", importedItems);

		_service.ImportVaultInto(vault, importResult);

		vault.Sacks[0].Count.Should().Be(2);// old cleared, new ones imported
		vault.Sacks[0].items[0].BaseItemId.Should().Be("records/gear/new1.dbr");
		vault.Sacks[0].items[1].BaseItemId.Should().Be("records/gear/new2.dbr");
		vault.Sacks[1].Count.Should().Be(1);
		vault.Sacks[1].items[0].BaseItemId.Should().Be("records/gear/new3.dbr");

		// Other sacks should be empty
		for (int i = 2; i < 12; i++)
			vault.Sacks[i].IsEmpty.Should().BeTrue();
	}
}
