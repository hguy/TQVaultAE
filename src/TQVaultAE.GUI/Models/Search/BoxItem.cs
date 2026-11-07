using System.Text.Json.Serialization;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Models.Search;

public class BoxItem
{
	VirtualCheckedListBox? _CheckedList;
	[JsonIgnore]
	public VirtualCheckedListBox? CheckedList
	{
		get => _CheckedList;
		set
		{
			CheckedListName = value?.Name ?? string.Empty;
			_CheckedList = value;
		}
	}

	ScalingLabel? _Category;
	[JsonIgnore]
	public ScalingLabel? Category
	{
		get => _Category; 
		set
		{
			CategoryName = value?.Name ?? string.Empty;
			_Category = value;
		}
	}

	[JsonIgnore]
	public IEnumerable<SearchResult> MatchingResults { get; set; } = [];

	public string CheckedListName { get; set; } = string.Empty;
	public string CategoryName { get; set; } = string.Empty;
	public string DisplayValue { get; set; } = string.Empty;
	public override string ToString()
		=> this.DisplayValue;
}