using System.Text.Json.Serialization;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SearchOperator
	{
		/// <summary>
		/// Find items having all filters
		/// </summary>
		And,
		/// <summary>
		/// Find items having one or more filter
		/// </summary>
		Or
	}
}
