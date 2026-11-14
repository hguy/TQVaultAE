namespace TQVaultAE.Domain.Entities;

public class HighlightFilterValues
{
	public int MaxStr;
	public int MaxDex;
	public int MaxInt;
	public int MinStr;
	public int MinDex;
	public int MinInt;
	public bool MinRequierement;
	public bool MaxRequierement;
	public List<string> ClassItem = [];
	public List<Rarity> Rarity = [];
	public int MaxLvl;
	public int MinLvl;
	public List<GameDlc> Origin = [];
	public bool HavingPrefix;
	public bool HavingSuffix;
	public bool HavingRelic;
	public bool HavingCharm;
	public bool IsSetItem;
	/// <summary>Only the items that grant a skill.</summary>
	public bool HavingSkill;

	/// <summary>
	/// <c>true</c> when no filter is active: no requirement range, no type, rarity or origin, and no "having" option.
	/// The level and attribute values alone do not count without their minimum or maximum check box.
	/// </summary>
	public bool IsEmpty
		=> !(this.MaxRequierement || this.MinRequierement
			|| this.HavingPrefix || this.HavingSuffix || this.HavingRelic || this.HavingCharm || this.IsSetItem || this.HavingSkill
			|| this.ClassItem.Count != 0 || this.Rarity.Count != 0 || this.Origin.Count != 0);
}