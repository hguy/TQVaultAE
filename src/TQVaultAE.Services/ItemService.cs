using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services
{
	public class ItemService : IItemService
	{
		List<ItemLocation> _ItemDatabase = new List<ItemLocation>();
		public ReadOnlyCollection<ItemLocation> ItemDatabase { get; }
		private readonly SessionContext Ctx;
		private readonly ILogger Log;
		private readonly IItemProvider ItemProvider;
		private readonly IGamePathService GamePathService;

		public ItemService(
			SessionContext sessionContext
			, IItemProvider itemProvider
			, IGamePathService gamePathService
			, ILogger<ItemService> log
		)
		{
			this.Ctx = sessionContext;
			this.Log = log;
			this.ItemProvider = itemProvider;
			this.GamePathService = gamePathService;

			this.ItemDatabase = BuildItemDatabase();
		}

		
		public void CleanupZombies()
		{
			_ItemDatabase.RemoveAll(id => string.IsNullOrWhiteSpace(id.ItemName));
		}

		/// <summary>
		/// Seek for all available items
		/// </summary>
		private ReadOnlyCollection<ItemLocation> BuildItemDatabase()
		{
			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in Ctx.Vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value.Value;

				if (vault == null)
					continue;

				int vaultNumber = -1;
				foreach (SackCollection sack in vault)
				{
					vaultNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack.Cast<Item>())
					{
						_ItemDatabase.Add(new ItemLocation(
							vaultFile
							, Path.GetFileNameWithoutExtension(vaultFile)
							, vaultNumber
							, SackType.Vault
							, new Lazy<Domain.Results.ToFriendlyNameResult>(
								() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
								, LazyThreadSafetyMode.ExecutionAndPublication
							)
						));
					}
				}
			}

			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in Ctx.Players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value.Value;

				if (player == null)
					continue;

				string playerName = this.GamePathService.GetNameFromFile(playerFile);
				if (playerName == null)
					continue;

				int sackNumber = -1;
				foreach (SackCollection sack in player)
				{
					sackNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack.Cast<Item>())
					{
						_ItemDatabase.Add(new ItemLocation(
							playerFile
							, playerName
							, sackNumber
							, SackType.Player
							, new Lazy<Domain.Results.ToFriendlyNameResult>(
								() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
								, LazyThreadSafetyMode.ExecutionAndPublication
							)
						));
					}
				}

				// Now search the Equipment panel
				var equipmentSack = player.EquipmentSack;
				if (equipmentSack == null)
					continue;

				foreach (var item in equipmentSack.Cast<Item>())
				{
					_ItemDatabase.Add(new ItemLocation(
						playerFile
						, playerName
						, 0
						, SackType.Equipment
						, new Lazy<Domain.Results.ToFriendlyNameResult>(
							() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
							, LazyThreadSafetyMode.ExecutionAndPublication
						)
					));
				}
			}

			foreach (KeyValuePair<string, Lazy<Stash>> kvp in Ctx.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value.Value;

				// Make sure we have a valid name and stash.
				if (stash == null)
					continue;

				string stashName = this.GamePathService.GetNameFromFile(stashFile);
				if (stashName == null)
					continue;

				SackCollection sack = stash.Sack;
				if (sack == null)
					continue;

				int sackNumber = 2;
				SackType sackType = SackType.Stash;
				if (stashName == Resources.GlobalTransferStash)
				{
					sackNumber = 1;
					sackType = SackType.TransferStash;
				}
				else if (stashName == Resources.GlobalRelicVaultStash)
				{
					sackNumber = 3;
					sackType = SackType.RelicVaultStash;
				}

				foreach (var item in sack.Cast<Item>())
				{
					_ItemDatabase.Add(new ItemLocation(
						stashFile
						, stashName
						, sackNumber
						, sackType
						, new Lazy<Domain.Results.ToFriendlyNameResult>(
							() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
							, LazyThreadSafetyMode.ExecutionAndPublication
						)
					));
				}
			}

			return new ReadOnlyCollection<ItemLocation>(_ItemDatabase);
		}
	}
}
