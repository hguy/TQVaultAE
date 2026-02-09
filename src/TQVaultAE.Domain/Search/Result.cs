//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

/// <summary>
/// Class for an individual result in the results list.
/// </summary>
public class Result
{
	/// <summary>
	/// Reference to the Item - contains current location and all item data
	/// </summary>
	public readonly Item Item;

	/// <summary>
	/// Gets the container file path from the Item
	/// </summary>
	public string Container => this.Item?.ContainerPath ?? string.Empty;

	/// <summary>
	/// Gets the container display name from the Item
	/// </summary>
	public string ContainerName => this.Item?.ContainerName ?? string.Empty;

	/// <summary>
	/// Gets the sack number from the Item
	/// </summary>
	public int SackNumber => this.Item?.SackNumber ?? 0;

	/// <summary>
	/// Gets the sack type from the Item
	/// </summary>
	public SackType SackType => this.Item?.ContainerType ?? default;

	private readonly Lazy<ToFriendlyNameResult> FriendlyNamesLazyLoader;
	public ToFriendlyNameResult FriendlyNames { get; private set; }
	public string ItemName { get; private set; }
	public ItemStyle ItemStyle { get; private set; }
	public TQColor TQColor { get; private set; }
	public int RequiredLevel { get; private set; }

	public string IdString
		=> string.Join("|", new[] {
			Container
			, ContainerName
			, SackNumber.ToString()
			, this.SackType.ToString()
			, this.FriendlyNames?.FullNameBagTooltip ?? string.Empty
		});


	/// <summary>
	/// Creates a new Result wrapping an Item
	/// </summary>
	/// <param name="item">The Item to wrap</param>
	/// <param name="fnames">Lazy loader for friendly names</param>
	public Result(Item item, Lazy<ToFriendlyNameResult> fnames)
	{
		this.Item = item ?? throw new ArgumentNullException(nameof(item));
		this.FriendlyNamesLazyLoader = fnames ?? throw new ArgumentNullException(nameof(fnames));
	}

	public void LazyLoad()
	{
		this.FriendlyNames = this.FriendlyNamesLazyLoader.Value;
		this.ItemName = this.FriendlyNames.FullNameClean;
		this.ItemStyle = this.Item.ItemStyle;
		this.TQColor = this.Item.ItemStyle.TQColor();
		this.RequiredLevel = GetRequirement(this.FriendlyNames.RequirementVariables.Values, "levelRequirement");
	}

	private int GetRequirement(IList<Variable> variables, string key)
	{
		return variables
			.Where(v => string.Equals(v.Name, key, StringComparison.InvariantCultureIgnoreCase) && v.DataType == VariableDataType.Integer && v.NumberOfValues > 0)
			.Select(v => v.GetInt32(0))
			.DefaultIfEmpty(0)
			.Max();
	}

}