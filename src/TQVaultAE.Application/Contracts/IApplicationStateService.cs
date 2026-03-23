using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts;

/// <summary>
/// Service for managing application state and searching items.
/// Provides access to loaded players, vaults, stashes, and the item database.
/// </summary>
public interface IApplicationStateService
{
    /// <summary>
    /// Gets the currently selected player.
    /// </summary>
    PlayerCollection CurrentPlayer { get; }
    
    /// <summary>
    /// Gets all loaded players.
    /// </summary>
    IReadOnlyDictionary<string, PlayerCollection> LoadedPlayers { get; }
    
    /// <summary>
    /// Gets all loaded vaults.
    /// </summary>
    IReadOnlyDictionary<string, PlayerCollection> LoadedVaults { get; }
    
    /// <summary>
    /// Gets all loaded stashes.
    /// </summary>
    IReadOnlyDictionary<string, Stash> LoadedStashes { get; }
    
    /// <summary>
    /// Gets the global searchable item database containing all items across all loaded containers.
    /// </summary>
    IReadOnlyList<SearchResult> ItemDatabase { get; }
    
    /// <summary>
    /// Sets the current player.
    /// </summary>
    /// <param name="player">The player to set as current.</param>
    void SetCurrentPlayer(PlayerCollection player);
    
    /// <summary>
    /// Rebuilds the item database from all loaded containers.
    /// Should be called when containers are loaded/changed.
    /// </summary>
    void RebuildItemDatabase();
    
    /// <summary>
    /// Clears all application state including loaded players, vaults, stashes, and the item database.
    /// </summary>
    void ClearAll();

    #region Search Methods

    /// <summary>
    /// Executes a search query and returns matching results.
    /// </summary>
    /// <param name="query">The search query parameters.</param>
    /// <returns>List of matching results.</returns>
    IReadOnlyList<SearchResult> ExecuteSearch(SearchQueryRequest query);
    
    /// <summary>
    /// Filters results based on criteria.
    /// </summary>
    /// <param name="results">The results to filter.</param>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>Filtered results.</returns>
    IReadOnlyList<SearchResult> FilterResults(IEnumerable<SearchResult> results, SearchFilterDto filter);
    
    /// <summary>
    /// Performs a full-text search on items.
    /// </summary>
    /// <param name="searchText">The search text.</param>
    /// <param name="isRegex">Whether the search text is a regex pattern.</param>
    /// <returns>Matching results.</returns>
    IReadOnlyList<SearchResult> FullTextSearch(string searchText, bool isRegex = false);

    #endregion
}
