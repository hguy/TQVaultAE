using System.Text.Json.Serialization;

namespace TQVaultAE.Domain.Entities;

public class BagButtonIconInfo
{
	/// <summary>
	/// BagButton display mode
	/// </summary>
	[JsonPropertyName("mode")]
	[JsonConverter(typeof(JsonStringEnumConverter<BagButtonDisplayMode>))]
	public BagButtonDisplayMode DisplayMode;
	/// <summary>
	/// BagButton Custom label 
	/// </summary>
	[JsonPropertyName("label")]
	public string Label = string.Empty;

	/// <summary>
	/// BagButton On icon
	/// </summary>
	[JsonPropertyName("on")]
	public string OnStr = string.Empty;

	[JsonIgnore]
	public RecordId On
	{
		get => OnStr;
		set => OnStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// BagButton Off icon
	/// </summary>
	[JsonPropertyName("off")]
	public string OffStr = string.Empty;

	[JsonIgnore]
	public RecordId Off
	{
		get => OffStr;
		set => OffStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// BagButton Over icon
	/// </summary>
	[JsonPropertyName("over")]
	public string OverStr = string.Empty;

	[JsonIgnore]
	public RecordId Over
	{
		get => OverStr;
		set => OverStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// Returns a copy that shares nothing with this instance, so a pasted icon is not changed with its source.
	/// </summary>
	public BagButtonIconInfo Clone()
		=> (BagButtonIconInfo)this.MemberwiseClone();
}