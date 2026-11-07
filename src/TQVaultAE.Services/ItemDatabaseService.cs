using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Services;

/// <summary>
/// Service for managing the global searchable item database.
/// </summary>
public class ItemDatabaseService : IItemDatabaseService
{
    private readonly ILogger<ItemDatabaseService> _log;
    private readonly IItemProvider _itemProvider;
    private readonly SessionContext _sessionContext;

    /// <summary>
    /// The searchable items and, by reference, the items already in it. Replaced as a whole, so both always match.
    /// </summary>
    /// <remarks>
    /// Enumerating a <see cref="ConcurrentBag{T}"/> locks it and copies it: checking for duplicates that way made
    /// loading the saves quadratic, with every loading thread waiting on that lock.
    /// </remarks>
    private sealed record Database(ConcurrentBag<SearchResult> Items, ConcurrentDictionary<Item, byte> Indexed)
    {
        public static Database Of(IEnumerable<SearchResult> results)
        {
            var list = results.ToList();
            var indexed = new ConcurrentDictionary<Item, byte>(ReferenceEqualityComparer.Instance);
            foreach (var r in list)
                indexed.TryAdd(r.Item, 0);
            return new Database(new ConcurrentBag<SearchResult>(list), indexed);
        }
    }

    private Database _database = Database.Of([]);

    public ConcurrentBag<SearchResult> ItemDatabase => this._database.Items;

    public ItemDatabaseService(
        ILogger<ItemDatabaseService> log,
        IItemProvider itemProvider,
        SessionContext sessionContext)
    {
        this._log = log;
        this._itemProvider = itemProvider;
        this._sessionContext = sessionContext;
    }

    /// <inheritdoc/>
    public bool TryAddItemToDatabase(Item item)
    {
        if (item == null)
            return false;

        // Already in the database (by reference)
        var database = this._database;
        if (!database.Indexed.TryAdd(item, 0))
            return false;

        var result = new SearchResult(
            item,
            new Lazy<ToFriendlyNameResult>(
                () => this._itemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay),
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );
        database.Items.Add(result);
        return true;
    }

    /// <inheritdoc/>
    public void AddItemToDatabase(Item item, string containerPath, string containerName, int sackNumber, SackType sackType, StashType? stashType = null)
    {
        if (item == null)
            return;

        item.Place.Path = containerPath;
        item.Place.Name = containerName;
        item.Place.SackNumber = sackNumber;
        item.Place.SackType = sackType;
        item.Place.StashType = stashType;

        this.TryAddItemToDatabase(item);
    }

    /// <inheritdoc/>
    public void RemoveItemFromDatabase(Item item)
    {
        if (item == null)
            return;

        this._database = Database.Of(this.ItemDatabase.Where(r => r.Item != item));
    }

    /// <inheritdoc/>
    public void ClearItemDatabase()
        => this._database = Database.Of([]);

    /// <inheritdoc/>
    public void RebuildItemDatabase()
    {
        this.ClearItemDatabase();

        // Add vault items
        foreach (var kvp in this._sessionContext.Vaults)
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
        foreach (var kvp in this._sessionContext.Players)
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
        foreach (var kvp in this._sessionContext.Stashes)
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

        this._log.LogInformation("Item database rebuilt with {Count} items", this.ItemDatabase.Count);
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<RecordId, int> CountItemsByRecord(IEnumerable<RecordId> recordIds)
    {
        var counts = new Dictionary<RecordId, int>();
        foreach (var id in recordIds)
            counts.TryAdd(id, 0);

        if (counts.Count == 0)
            return counts;

        foreach (var result in this.ItemDatabase)
        {
            if (result.Item.BaseItemId is { } id && counts.TryGetValue(id, out var count))
                counts[id] = count + 1;
        }

        return counts;
    }

    #region Search Methods

    /// <inheritdoc/>
    public IReadOnlyList<SearchResult> FullTextSearch(string searchText, bool isRegex = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Array.Empty<SearchResult>();

        var itemDatabase = this.ItemDatabase;
        if (itemDatabase == null || itemDatabase.IsEmpty)
            return Array.Empty<SearchResult>();

        return this.ApplyTextSearch(itemDatabase, searchText, isRegex).ToList();
    }

    public void ResetAllItemDatabase(List<SearchResult> validItems)
    {
	    this._database = Database.Of(validItems);
	}

	private readonly object _warmUpLock = new();
	private Task? _warmUp;

	/// <inheritdoc/>
	public Task WarmUpAsync()
	{
		lock (this._warmUpLock)
		{
			if (this._warmUp is { IsCompleted: false })
				return this._warmUp;

			var items = this.ItemDatabase.ToArray();
			// LongRunning gives a dedicated thread, so its priority can be lowered without touching the thread pool.
			this._warmUp = Task.Factory.StartNew(() => this.WarmUp(items), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			return this._warmUp;
		}
	}

	private void WarmUp(SearchResult[] items)
	{
		Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		long allocated = GC.GetTotalAllocatedBytes();
		int collections = GC.CollectionCount(0);
		int failed = 0;

		// Half the cores, so the main window stays responsive while this runs.
		var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };
		Parallel.ForEach(items, options, item =>
		{
			try
			{
				item.LazyLoad();
			}
			catch (Exception ex)
			{
				// The search dialog drops items without a name; one broken item must not stop the others.
				if (Interlocked.Increment(ref failed) == 1)
					this._log.LogWarning(ex, "Search warm-up failed for {Item}", item.Item?.BaseItemId);
			}
		});

		// Allocations and collections cover the whole process while this runs, as other startup work goes on too.
		this._log.LogInformation("Search data warmed up in {Elapsed} ms ({Count} items, {Failed} failed, {Allocated} MB allocated, {Collections} gen0 collections)"
			, watch.ElapsedMilliseconds, items.Length, failed, (GC.GetTotalAllocatedBytes() - allocated) >> 20, GC.CollectionCount(0) - collections);
	}

	/// <inheritdoc/>
	public IReadOnlyList<SearchResult> ExecuteAdvancedSearch(AdvancedSearchRequest request)
    {
        if (request.InitialResults == null)
            return Array.Empty<SearchResult>();

        // Start with all initial results
        var results = request.InitialResults.ToList();

        // Apply quick filters
        if (request.HasPrefix)
            results = results.Where(i => i.FriendlyNames?.Item?.HasPrefix == true).ToList();

        if (request.HasSuffix)
            results = results.Where(i => i.FriendlyNames?.Item?.HasSuffix == true).ToList();

        if (request.HasRelic)
            results = results.Where(i => i.FriendlyNames?.Item?.HasRelic == true).ToList();

        if (request.HasCharm)
            results = results.Where(i => i.FriendlyNames?.Item?.HasCharm == true).ToList();

        if (request.IsSetItem)
            results = results.Where(i => i.FriendlyNames?.ItemSet != null).ToList();

        if (request.HasSkill)
            results = results.Where(i => this._itemProvider.HasGrantedSkill(i.Item)).ToList();

        // Apply minimum requirements
        if (request.MinLevel > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Lvl.HasValue || req.Lvl.Value >= request.MinLevel;
            }).ToList();

        if (request.MinStrength > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Str.HasValue || req.Str.Value >= request.MinStrength;
            }).ToList();

        if (request.MinDexterity > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Dex.HasValue || req.Dex.Value >= request.MinDexterity;
            }).ToList();

        if (request.MinIntelligence > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Int.HasValue || req.Int.Value >= request.MinIntelligence;
            }).ToList();

        // Apply maximum requirements
        if (request.MaxLevel > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Lvl.HasValue || req.Lvl.Value <= request.MaxLevel;
            }).ToList();

        if (request.MaxStrength > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Str.HasValue || req.Str.Value <= request.MaxStrength;
            }).ToList();

        if (request.MaxDexterity > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Dex.HasValue || req.Dex.Value <= request.MaxDexterity;
            }).ToList();

        if (request.MaxIntelligence > 0)
            results = results.Where(x =>
            {
                var req = x.FriendlyNames?.RequirementInfo;
                if (req == null) return true;
                return !req.Int.HasValue || req.Int.Value <= request.MaxIntelligence;
            }).ToList();

        this._log.LogDebug("Advanced search executed: {Count} results found", results.Count);
        return results;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Applies text search to results.
    /// </summary>
    private IEnumerable<SearchResult> ApplyTextSearch(IEnumerable<SearchResult> results, string searchText, bool isRegex)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return results;

        if (isRegex)
        {
            try
            {
                var regex = new Regex(searchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return results.Where(r => r.FriendlyNames != null &&
                    regex.IsMatch(r.FriendlyNames.FullNameClean ?? string.Empty));
            }
            catch (ArgumentException)
            {
                // Invalid regex, fall back to contains
                return results.Where(r => r.FriendlyNames != null &&
                    (r.FriendlyNames.FullNameClean ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }
        }
        else
        {
            return results.Where(r => r.FriendlyNames != null &&
                r.FriendlyNames.FulltextIsMatchIndexOf(searchText));
        }
    }

    #endregion
}
