namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Data transfer object for search queries.
/// </summary>
public class SearchQueryRequest
{
    /// <summary>
    /// The search text to match against item names and attributes.
    /// </summary>
    public string SearchText { get; set; }
    
    /// <summary>
    /// Whether the search text is a regular expression.
    /// </summary>
    public bool IsRegex { get; set; }
    
    /// <summary>
    /// The logical operator to use when combining filters.
    /// </summary>
    public SearchOperator Operator { get; set; }
    
    /// <summary>
    /// List of filters to apply to the search.
    /// </summary>
    public List<SearchFilterDto> Filters { get; set; } = new List<SearchFilterDto>();
    
    /// <summary>
    /// Maximum number of results per category.
    /// </summary>
    public int MaxResultsPerCategory { get; set; } = 100;
}

/// <summary>
/// Data transfer object for search filters.
/// </summary>
public class SearchFilterDto
{
    /// <summary>
    /// The type of filter to apply.
    /// </summary>
    public SearchFilterType FilterType { get; set; }
    
    /// <summary>
    /// The value to filter by.
    /// </summary>
    public object Value { get; set; }
}

/// <summary>
/// Logical operators for combining search filters.
/// </summary>
public enum SearchOperator
{
    /// <summary>
    /// All filters must match (AND).
    /// </summary>
    And,
    
    /// <summary>
    /// Any filter can match (OR).
    /// </summary>
    Or
}

/// <summary>
/// Types of search filters available.
/// </summary>
public enum SearchFilterType
{
    /// <summary>
    /// Filter by item rarity.
    /// </summary>
    Rarity,
    
    /// <summary>
    /// Filter by item type/class.
    /// </summary>
    ItemType,
    
    /// <summary>
    /// Filter by minimum level requirement.
    /// </summary>
    MinLevel,
    
    /// <summary>
    /// Filter by maximum level requirement.
    /// </summary>
    MaxLevel,
    
    /// <summary>
    /// Filter by set item membership.
    /// </summary>
    SetItem,
    
    /// <summary>
    /// Filter by having a prefix.
    /// </summary>
    HasPrefix,
    
    /// <summary>
    /// Filter by having a suffix.
    /// </summary>
    HasSuffix,
    
    /// <summary>
    /// Filter by having a relic.
    /// </summary>
    HasRelic,
    
    /// <summary>
    /// Filter by having a charm.
    /// </summary>
    HasCharm,
    
    /// <summary>
    /// Filter by container (vault/player name).
    /// </summary>
    Container,
    
    /// <summary>
    /// Filter by quality.
    /// </summary>
    Quality,
    
    /// <summary>
    /// Filter by game origin (DLC).
    /// </summary>
    Origin,
    
    /// <summary>
    /// Filter by minimum strength requirement.
    /// </summary>
    MinStrength,
    
    /// <summary>
    /// Filter by maximum strength requirement.
    /// </summary>
    MaxStrength,
    
    /// <summary>
    /// Filter by minimum dexterity requirement.
    /// </summary>
    MinDexterity,
    
    /// <summary>
    /// Filter by maximum dexterity requirement.
    /// </summary>
    MaxDexterity,
    
    /// <summary>
    /// Filter by minimum intelligence requirement.
    /// </summary>
    MinIntelligence,
    
    /// <summary>
    /// Filter by maximum intelligence requirement.
    /// </summary>
    MaxIntelligence
}
