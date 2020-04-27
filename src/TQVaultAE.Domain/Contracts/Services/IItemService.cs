using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface IItemService
	{
		/// <summary>
		/// Expose the item database built when instanciated
		/// </summary>
		ReadOnlyCollection<ItemLocation> ItemDatabase { get; }

		/// <summary>
		/// Remove records not able to produce there name.
		/// </summary>
		void CleanupZombies();
	}
}
