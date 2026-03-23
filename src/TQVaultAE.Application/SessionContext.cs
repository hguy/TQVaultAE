using System.Collections.Concurrent;
using System.Drawing;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Application;

/// <summary>
/// Shared data context for all services
/// </summary>
/// <remarks>must be agnostic so no Winform references. Only data</remarks>
public class SessionContext
{
	private readonly IItemProvider ItemProvider;

	public SessionContext(IItemProvider ItemProvider)
	{
		this.ItemProvider = ItemProvider;
	}

	/// <summary>
	/// Currently selected player
	/// </summary>
	public PlayerCollection CurrentPlayer { get; set; }

	private BagButtonIconInfo iconInfoCopy;
	/// <summary>
	/// Last icon info copied
	/// </summary>
	public BagButtonIconInfo IconInfoCopy
	{
		get => iconInfoCopy;
		set
		{
			iconInfoCopy = value;
			iconInfoCopied = true;
		}
	}

	private bool iconInfoCopied;
	/// <summary>
	/// Is there any IconInfo copied
	/// </summary>
	/// <remarks>this allow <see cref="IconInfoCopy"/> to have null relevant</remarks>
	public bool IconInfoCopied => iconInfoCopied;
	/// <summary>
	/// Dictionary of all loaded player files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, PlayerCollection> Players = new LazyConcurrentDictionary<string, PlayerCollection>();

	/// <summary>
	/// Dictionary of all loaded vault files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, PlayerCollection> Vaults = new LazyConcurrentDictionary<string, PlayerCollection>();

	/// <summary>
	/// Dictionary of all loaded player stash files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, Stash> Stashes = new LazyConcurrentDictionary<string, Stash>();

	#region HighlightSearchItem

	/// <summary>
	/// Gets or sets the background Color for item highlight.
	/// </summary>
	public Color HighlightSearchItemColor { get; set; } = TQColor.Indigo.Color();
	/// <summary>
	/// Gets or sets the border color for item highlight.
	/// </summary>
	public Color HighlightSearchItemBorderColor { get; set; } = TQColor.Red.Color();

	/// <summary>
	/// Hightlight search string
	/// </summary>
	public string HighlightSearch { get; set; }

	/// <summary>
	/// Hightlight search filters
	/// </summary>
	public HighlightFilterValues HighlightFilter { get; set; }

	/// <summary>
	/// Hightlight search items
	/// </summary>
	public readonly List<Item> HighlightedItems = new();

	/// <summary>
	/// Find items to highlight
	/// </summary>
	public void FindHighlight()
	{
		var hasSearch = !string.IsNullOrWhiteSpace(this.HighlightSearch);
		var hasFilter = this.HighlightFilter is not null;

		if (hasSearch || hasFilter)
		{
			this.HighlightedItems.Clear();

			// Check for players
			var sacksplayers = this.Players.Select(p => p.Value.Value)
				.SelectMany(p =>
				{
					var retval = new List<SackCollection>();

					if (p.EquipmentSack is not null)
						retval.Add(p.EquipmentSack);

					if (p.Sacks is not null)
						retval.AddRange(p.Sacks);

					return retval;
				})
				.Where(s => s.Count > 0);

			// Check for Vaults
			var sacksVault = this.Vaults.Select(p => p.Value.Value)
				.SelectMany(p => p.Sacks)
				.Where(s => s is not null && s.Count > 0);

			// Check for Stash
			var sacksStash = this.Stashes.Select(p => p.Value.Value)
				.Select(p => p.Sack)
				.Where(s => s is not null && s.Count > 0);

			var availableItems = sacksplayers.Concat(sacksVault).Concat(sacksStash).SelectMany(i => i)
				.Select(i =>
				{
					var fnr = ItemProvider.GetFriendlyNames(i, FriendlyNamesExtraScopes.ItemFullDisplay);

					return new
					{
						Item = i,
						FriendlyNames = fnr,
						Info = fnr.RequirementInfo,
					};
				}).AsQueryable();

			if (hasSearch)
			{
				var (isRegex, _, regex, regexIsValid) = StringHelper.IsTQVaultSearchRegEx(this.HighlightSearch);

				availableItems = availableItems.Where(i =>
					isRegex && regexIsValid
						? i.FriendlyNames.FulltextIsMatchRegex(regex)
						: i.FriendlyNames.FulltextIsMatchIndexOf(this.HighlightSearch)
				);
			}

			if (hasFilter)
			{
				if (this.HighlightFilter.MinRequierement)
				{
					// Min Lvl
					if (this.HighlightFilter.MinLvl != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Lvl.HasValue // Item doesn't have requirement
							|| i.Info.Lvl >= this.HighlightFilter.MinLvl
						);
					}
					// Min Dex
					if (this.HighlightFilter.MinDex != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Dex.HasValue
							|| i.Info.Dex >= this.HighlightFilter.MinDex
						);
					}
					// Min Str
					if (this.HighlightFilter.MinStr != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Str.HasValue
							|| i.Info.Str >= this.HighlightFilter.MinStr
						);
					}
					// Min Int
					if (this.HighlightFilter.MinInt != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Int.HasValue
							|| i.Info.Int >= this.HighlightFilter.MinInt
						);
					}
				}

				if (this.HighlightFilter.MaxRequierement)
				{
					// Max Lvl
					if (this.HighlightFilter.MaxLvl != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Lvl.HasValue // Item doesn't have requirement
							|| i.Info.Lvl <= this.HighlightFilter.MaxLvl
						);
					}
					// Max Dex
					if (this.HighlightFilter.MaxDex != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Dex.HasValue
							|| i.Info.Dex <= this.HighlightFilter.MaxDex
						);
					}
					// Max Str
					if (this.HighlightFilter.MaxStr != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Str.HasValue
							|| i.Info.Str <= this.HighlightFilter.MaxStr
						);
					}
					// Max Int
					if (this.HighlightFilter.MaxInt != 0)
					{
						availableItems = availableItems.Where(i =>
							!i.Info.Int.HasValue
							|| i.Info.Int <= this.HighlightFilter.MaxInt
						);
					}
				}

				if (this.HighlightFilter.ClassItem.Any())
				{
					availableItems = availableItems.Where(i =>
						this.HighlightFilter.ClassItem
							.Any(ci => ci.Equals(i.Item.ItemClass, StringComparison.OrdinalIgnoreCase))
					);
				}

				if (this.HighlightFilter.Rarity.Any())
				{
					availableItems = availableItems.Where(i =>
						this.HighlightFilter.Rarity.Contains(i.Item.Rarity)
					);
				}

				if (this.HighlightFilter.Origin.Any())
				{
					availableItems = availableItems.Where(i =>
						this.HighlightFilter.Origin.Contains(i.Item.GameDlc)
					);
				}

				if (this.HighlightFilter.HavingPrefix)
					availableItems = availableItems.Where(i => i.Item.HasPrefix);

				if (this.HighlightFilter.HavingSuffix)
					availableItems = availableItems.Where(i => i.Item.HasSuffix);

				if (this.HighlightFilter.HavingRelic)
					availableItems = availableItems.Where(i => i.Item.HasRelic);

				if (this.HighlightFilter.HavingCharm)
					availableItems = availableItems.Where(i => i.Item.HasCharm);

				if (this.HighlightFilter.IsSetItem)
					availableItems = availableItems.Where(i => i.FriendlyNames.ItemSet != null);
			}

			this.HighlightedItems.AddRange(availableItems.Select(i => i.Item).ToList());
			return;
		}
		ResetHighlight();
	}

	/// <summary>
	/// Reset Hightlight search
	/// </summary>
	public void ResetHighlight()
	{
		this.HighlightedItems.Clear();
		this.HighlightSearch = null;
		this.HighlightFilter = null;
	}

	#endregion

	#region Search Item Database

	/// <summary>
	/// Global searchable item database containing all items across all loaded containers
	/// </summary>
	public ConcurrentBag<SearchResult> ItemDatabase { get; private set; } = new();

	/// <summary>
	/// Adds an item to the search database if it's not already present.
	/// Location properties should already be set on the item before calling this.
	/// </summary>
	/// <param name="item">The item to add</param>
	/// <returns>True if the item was added, false if it already exists</returns>
	public bool TryAddItemToDatabase(Item item)
	{
		if (item == null)
			return false;

		// Check if already exists (by reference) - ConcurrentBag doesn't have FirstOrDefault
		foreach (var existing in this.ItemDatabase)
		{
			if (existing.Item == item)
				return false; // Already exists
		}

		var result = new SearchResult(
			item,
			new Lazy<ToFriendlyNameResult>(
				() => this.ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay),
				LazyThreadSafetyMode.ExecutionAndPublication
			)
		);
		this.ItemDatabase.Add(result);
		return true;
	}

	/// <summary>
	/// Adds an item to the search database with its container information.
	/// Updates item location properties and adds to database if not already present.
	/// </summary>
	public void AddItemToDatabase(Item item, string containerPath, string containerName, int sackNumber, SackType sackType, StashType? stashType = null)
	{
		if (item == null)
			return;

		// Update item location properties
		item.Place.Path = containerPath;
		item.Place.Name = containerName;
		item.Place.SackNumber = sackNumber;
		item.Place.SackType = sackType;
		item.Place.StashType = stashType;

		// Add to database if not already present
		this.TryAddItemToDatabase(item);
	}

	/// <summary>
	/// Removes an item from the search database
	/// </summary>
	public void RemoveItemFromDatabase(Item item)
	{
		if (item == null)
			return;

		// ConcurrentBag doesn't support RemoveAll, so we rebuild without the item
		var itemsToKeep = this.ItemDatabase.Where(r => r.Item != item).ToList();

		ClearItemDatabase();

		foreach (var result in itemsToKeep)
		{
			this.ItemDatabase.Add(result);
		}
	}

	/// <summary>
	/// Clears all items from the search database
	/// </summary>
	public void ClearItemDatabase()
		=> this.ItemDatabase = new();

	/// <summary>
	/// Populates the item database from all loaded containers (Vaults, Players, Stashes)
	/// </summary>
	public void RebuildItemDatabase()
	{
		this.ClearItemDatabase();

		// Add vault items
		foreach (var kvp in this.Vaults)
		{
			var vault = kvp.Value.Value;
			if (vault == null)
				continue;

			int sackNumber = -1;
			foreach (var sack in vault)
			{
				sackNumber++;
				if (sack == null)
					continue;

				foreach (var item in sack)
					this.AddItemToDatabase(item, kvp.Key, vault.PlayerName, sackNumber, SackType.Vault);
			}
		}

		// Add player items
		foreach (var kvp in this.Players)
		{
			var player = kvp.Value.Value;
			if (player == null)
				continue;

			// Player sacks
			int sackNumber = -1;
			foreach (var sack in player)
			{
				sackNumber++;
				if (sack == null)
					continue;

				foreach (var item in sack)
					this.AddItemToDatabase(item, kvp.Key, player.PlayerName, sackNumber, SackType.Player);
			}

			// Equipment sack
			if (player.EquipmentSack != null)
			{
				foreach (var item in player.EquipmentSack)
					this.AddItemToDatabase(item, kvp.Key, player.PlayerName, BagIdConstants.BAGID_EQUIPMENTPANEL, SackType.Equipment);
			}
		}

		// Add stash items
		foreach (var kvp in this.Stashes)
		{
			var stash = kvp.Value.Value;
			if (stash?.Sack == null)
				continue;

			int sackNumber = BagIdConstants.BAGID_PLAYERSTASH;
			StashType stashType = StashType.PlayerStash;

			if (stash.Sack.StashType == StashType.TransferStash)
			{

				sackNumber = BagIdConstants.BAGID_TRANSFERSTASH;
				stashType = StashType.TransferStash;
			}
			else if (stash.Sack.StashType == StashType.RelicVaultStash)
			{
				sackNumber = BagIdConstants.BAGID_RELICVAULTSTASH;
				stashType = StashType.RelicVaultStash;
			}

			foreach (var item in stash.Sack)
				this.AddItemToDatabase(item, kvp.Key, stash.PlayerName, sackNumber, SackType.Stash, stashType);
		}
	}

	#endregion

}