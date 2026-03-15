using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TQVaultAE.Application.Contracts;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Search;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Application.Services;

/// <summary>
/// Service for managing application state and searching items.
/// Provides access to loaded players, vaults, stashes, and the item database.
/// </summary>
public class ApplicationStateService : IApplicationStateService
{
    private readonly ILogger<ApplicationStateService> _log;
    private readonly SessionContext _sessionContext;

    public ApplicationStateService(
        ILogger<ApplicationStateService> log,
        SessionContext sessionContext)
    {
        this._log = log;
        this._sessionContext = sessionContext;
    }

    /// <summary>
    /// Gets the currently selected player.
    /// </summary>
    public PlayerCollection CurrentPlayer
        => this._sessionContext.CurrentPlayer;

    /// <summary>
    /// Gets all loaded players.
    /// </summary>
    public IReadOnlyDictionary<string, PlayerCollection> LoadedPlayers
        => this._sessionContext.Players.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Value
        );

    /// <summary>
    /// Gets all loaded vaults.
    /// </summary>
    public IReadOnlyDictionary<string, PlayerCollection> LoadedVaults
        => this._sessionContext.Vaults.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Value
        );

    /// <summary>
    /// Gets all loaded stashes.
    /// </summary>
    public IReadOnlyDictionary<string, Stash> LoadedStashes
        => this._sessionContext.Stashes.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Value
        );

    /// <summary>
    /// Gets the global searchable item database containing all items across all loaded containers.
    /// </summary>
    public IReadOnlyList<Result> ItemDatabase
        => this._sessionContext.ItemDatabase.ToList();

    /// <summary>
    /// Sets the current player.
    /// </summary>
    public void SetCurrentPlayer(PlayerCollection player)
    {
        this._sessionContext.CurrentPlayer = player;
        this._log.LogInformation("Current player set to {PlayerName}", player?.PlayerName);
    }

    /// <summary>
    /// Rebuilds the item database from all loaded containers.
    /// </summary>
    public void RebuildItemDatabase()
    {
        this._sessionContext.RebuildItemDatabase();
        this._log.LogInformation("Item database rebuilt with {Count} items", this._sessionContext.ItemDatabase.Count);
    }

    /// <summary>
    /// Clears all application state including loaded players, vaults, stashes, and the item database.
    /// </summary>
    public void ClearAll()
    {
        this._sessionContext.CurrentPlayer = null;
        this._sessionContext.Players.Clear();
        this._sessionContext.Vaults.Clear();
        this._sessionContext.Stashes.Clear();
        this._sessionContext.ClearItemDatabase();
        this._sessionContext.ResetHighlight();
        this._log.LogInformation("All application state cleared");
    }

    #region Search Methods

    /// <summary>
    /// Executes a search query and returns matching results.
    /// </summary>
    public IReadOnlyList<Result> ExecuteSearch(SearchQueryDto query)
    {
        if (query == null)
            return Array.Empty<Result>();

        var itemDatabase = _sessionContext.ItemDatabase;
        if (itemDatabase == null || itemDatabase.IsEmpty)
            return Array.Empty<Result>();

        // Start with all items
        var results = itemDatabase.AsQueryable();

        // Apply text search if provided
        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            results = ApplyTextSearch(results, query.SearchText, query.IsRegex);
        }

        // Apply filters
        if (query.Filters != null && query.Filters.Count > 0)
        {
            results = ApplyFilters(results, query.Filters, query.Operator);
        }

        // Limit results
        var finalResults = results.Take(query.MaxResultsPerCategory * 10).ToList();

        _log.LogDebug("Search executed: {Count} results found", finalResults.Count);
        return finalResults;
    }

    /// <summary>
    /// Filters results based on criteria.
    /// </summary>
    public IReadOnlyList<Result> FilterResults(IEnumerable<Result> results, SearchFilterDto filter)
    {
        if (results == null || filter == null)
            return Array.Empty<Result>();

        return ApplyFilter(results.AsQueryable(), filter).ToList();
    }

    /// <summary>
    /// Performs a full-text search on items.
    /// </summary>
    public IReadOnlyList<Result> FullTextSearch(string searchText, bool isRegex = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Array.Empty<Result>();

        var itemDatabase = _sessionContext.ItemDatabase;
        if (itemDatabase == null || itemDatabase.IsEmpty)
            return Array.Empty<Result>();

        return ApplyTextSearch(itemDatabase.AsQueryable(), searchText, isRegex).ToList();
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Applies text search to results.
    /// </summary>
    private IQueryable<Result> ApplyTextSearch(IQueryable<Result> results, string searchText, bool isRegex)
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
                    (r.FriendlyNames.FullNameClean ?? string.Empty).ContainsIgnoreCase(searchText));
            }
        }
        else
        {
            return results.Where(r => r.FriendlyNames != null && 
                r.FriendlyNames.FulltextIsMatchIndexOf(searchText));
        }
    }

    /// <summary>
    /// Applies multiple filters to results.
    /// </summary>
    private IQueryable<Result> ApplyFilters(IQueryable<Result> results, List<SearchFilterDto> filters, SearchOperator op)
    {
        if (filters == null || filters.Count == 0)
            return results;

        if (op == SearchOperator.And)
        {
            // AND operator: item must match all filters
            foreach (var filter in filters)
            {
                results = ApplyFilter(results, filter);
            }
            return results;
        }
        else
        {
            // OR operator: item must match any filter
            var allMatches = new List<Result>();
            foreach (var filter in filters)
            {
                var matches = ApplyFilter(results, filter).ToList();
                allMatches.AddRange(matches);
            }
            return allMatches.Distinct().AsQueryable();
        }
    }

    /// <summary>
    /// Applies a single filter to results.
    /// </summary>
    private IQueryable<Result> ApplyFilter(IQueryable<Result> results, SearchFilterDto filter)
    {
        if (filter?.Value == null)
            return results;

        switch (filter.FilterType)
        {
            case SearchFilterType.HasPrefix:
                if (filter.Value is bool hasPrefix && hasPrefix)
                    return results.Where(r => r.Item != null && r.Item.HasPrefix);
                break;

            case SearchFilterType.HasSuffix:
                if (filter.Value is bool hasSuffix && hasSuffix)
                    return results.Where(r => r.Item != null && r.Item.HasSuffix);
                break;

            case SearchFilterType.HasRelic:
                if (filter.Value is bool hasRelic && hasRelic)
                    return results.Where(r => r.Item != null && r.Item.HasRelic);
                break;

            case SearchFilterType.HasCharm:
                if (filter.Value is bool hasCharm && hasCharm)
                    return results.Where(r => r.Item != null && r.Item.HasCharm);
                break;

            case SearchFilterType.SetItem:
                if (filter.Value is bool isSetItem && isSetItem)
                    return results.Where(r => r.FriendlyNames != null && r.FriendlyNames.ItemSet != null);
                break;

            case SearchFilterType.MinLevel:
                if (filter.Value is int minLevel && minLevel > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Lvl.HasValue || 
                         r.FriendlyNames.RequirementInfo.Lvl >= minLevel));
                break;

            case SearchFilterType.MaxLevel:
                if (filter.Value is int maxLevel && maxLevel > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Lvl.HasValue || 
                         r.FriendlyNames.RequirementInfo.Lvl <= maxLevel));
                break;

            case SearchFilterType.MinStrength:
                if (filter.Value is int minStr && minStr > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Str.HasValue || 
                         r.FriendlyNames.RequirementInfo.Str >= minStr));
                break;

            case SearchFilterType.MaxStrength:
                if (filter.Value is int maxStr && maxStr > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Str.HasValue || 
                         r.FriendlyNames.RequirementInfo.Str <= maxStr));
                break;

            case SearchFilterType.MinDexterity:
                if (filter.Value is int minDex && minDex > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Dex.HasValue || 
                         r.FriendlyNames.RequirementInfo.Dex >= minDex));
                break;

            case SearchFilterType.MaxDexterity:
                if (filter.Value is int maxDex && maxDex > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Dex.HasValue || 
                         r.FriendlyNames.RequirementInfo.Dex <= maxDex));
                break;

            case SearchFilterType.MinIntelligence:
                if (filter.Value is int minInt && minInt > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Int.HasValue || 
                         r.FriendlyNames.RequirementInfo.Int >= minInt));
                break;

            case SearchFilterType.MaxIntelligence:
                if (filter.Value is int maxInt && maxInt > 0)
                    return results.Where(r => r.FriendlyNames != null && 
                        (!r.FriendlyNames.RequirementInfo.Int.HasValue || 
                         r.FriendlyNames.RequirementInfo.Int <= maxInt));
                break;

            case SearchFilterType.Rarity:
                if (filter.Value is ItemStyle rarity)
                    return results.Where(r => r.Item != null && r.Item.ItemStyle == rarity);
                break;

            case SearchFilterType.Origin:
                if (filter.Value is GameDlc origin)
                    return results.Where(r => r.Item != null && r.Item.GameDlc == origin);
                break;

            case SearchFilterType.Container:
                if (filter.Value is string containerName && !string.IsNullOrEmpty(containerName))
                    return results.Where(r => r.ContainerName != null && 
                        r.ContainerName.Equals(containerName, StringComparison.OrdinalIgnoreCase));
                break;
        }

        return results;
    }

    #endregion
}
