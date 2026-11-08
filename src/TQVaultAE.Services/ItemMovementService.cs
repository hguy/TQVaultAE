using Microsoft.Extensions.Logging;
using System.Drawing;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

/// <summary>
/// Service for handling item movement operations.
/// Contains pure business logic extracted from GUI layer.
/// </summary>
public class ItemMovementService(ILogger<ItemMovementService> log) : IItemMovementService
{
    private readonly ILogger<ItemMovementService> _log = log;

    /// <summary>
    /// Finds open cells in a sack for placing an item.
    /// Searches vertically for open space and returns (-1, -1) on failure.
    /// </summary>
    /// <param name="sack">The sack to search for open cells.</param>
    /// <param name="itemWidth">Width of the item to place.</param>
    /// <param name="itemHeight">Height of the item to place.</param>
    /// <param name="sackWidth">Width of the sack in cells.</param>
    /// <param name="sackHeight">Height of the sack in cells.</param>
    /// <returns>Point with coordinates for the upper left cell, or (-1, -1) if no space.</returns>
    public Point FindOpenCells(SackCollection sack, int itemWidth, int itemHeight, int sackWidth, int sackHeight)
    {
        if (sack == null || itemWidth <= 0 || itemHeight <= 0 || sackWidth <= 0 || sackHeight <= 0)
            return new Point(-1, -1);

        // Search vertically for open space
        for (int x = 0; x < sackWidth - itemWidth + 1; ++x)
        {
            for (int y = 0; y < sackHeight; ++y)
            {
                var foundItems = FindAllItems(sack, new Rectangle(x, y, itemWidth, itemHeight));
                
                if ((foundItems == null || foundItems.Count == 0) && itemWidth + x <= sackWidth)
                {
                    if (y + itemHeight <= sackHeight)
                    {
                        // The slot is free
                        return new Point(x, y);
                    }
                }
                else if (foundItems != null && foundItems.Count > 0)
                {
                    // Skip over the item
                    y += foundItems[0].Height - 1;
                }
            }
        }

        // We could not find a place for the item
        return new Point(-1, -1);
    }

    /// <summary>
    /// Validates if an item can be placed in the specified sack.
    /// Checks both space availability and item type restrictions.
    /// </summary>
    public bool CanPlaceItem(SackCollection sack, Item item, int sackWidth, int sackHeight)
    {
        if (sack == null || item == null)
            return false;

        // Check if item is valid for the stash type
        if (!IsItemValidForStashType(item, sack.StashType))
            return false;

        // Check if there's space for the item
        var position = FindOpenCells(sack, item.Width, item.Height, sackWidth, sackHeight);
        return position.X >= 0 && position.Y >= 0;
    }

    /// <summary>
    /// Checks if an item is valid for placement in a specific stash type.
    /// Relic Vault Stash only accepts artifacts, relics, charms, or formulae.
    /// </summary>
    public bool IsItemValidForStashType(Item item, StashType? stashType)
    {
        if (item == null)
            return false;

        // Relic Vault Stash only accepts artifacts, relics, charms, or formulae
        if (stashType == StashType.RelicVaultStash)
        {
            return item.IsArtifact || item.IsRelicOrCharm || item.IsFormulae;
        }

        return true;
    }

    /// <summary>
    /// Moves an item from source to destination sack.
    /// </summary>
    public ItemMoveResult MoveItem(Item item, SackCollection sourceSack, SackCollection destinationSack, Point destinationPosition)
    {
        if (item == null)
            return ItemMoveResultFailed("Item is null");

        if (destinationSack == null)
            return ItemMoveResultFailed("Destination sack is null", item);

        if (destinationPosition.X < 0 || destinationPosition.Y < 0)
            return ItemMoveResultFailed("Invalid destination position", item, destinationSack);

        // Remove from source sack if provided
        if (sourceSack != null)
        {
            sourceSack.RemoveItem(item);
        }

        // Set the item's position
        item.PositionX = destinationPosition.X;
        item.PositionY = destinationPosition.Y;

        // Add to destination sack
        destinationSack.AddItem(item);

        this._log.LogDebug("Moved item {ItemId} to position ({X}, {Y})", 
            item.BaseItemId, destinationPosition.X, destinationPosition.Y);

        return ItemMoveResult.Succeeded(item, destinationSack, destinationPosition);
    }

    /// <summary>
    /// Updates item location metadata after a move.
    /// This ensures search results always reflect the current location.
    /// </summary>
    public void UpdateItemLocation(Item item, PlayerCollection destinationPlayer, SackType destinationSackType, 
        int destinationSackNumber, StashType? destinationStashType = null, string? containerName = null)
    {
        if (item == null || destinationPlayer == null)
            return;

        // Update the item's location properties to reflect its new home
        item.Place.Path = destinationPlayer.PlayerFile;
        item.Place.Name = containerName ?? destinationPlayer.PlayerName;

        // Calculate sack number based on sack type
        switch (destinationSackType)
        {
            case SackType.Equipment:
                item.Place.SackNumber = BagIdConstants.BAGID_EQUIPMENTPANEL;
                break;
            case SackType.Stash:
                item.Place.SackNumber = destinationStashType.HasValue ? (int)destinationStashType : destinationSackNumber;
                break;
            case SackType.Vault:
            case SackType.Player:
            default:
                // For vaults and player sacks, use the current sack index
                item.Place.SackNumber = destinationSackNumber;
                break;
        }

        item.Place.SackType = destinationSackType;
        item.Place.StashType = destinationStashType;

        this._log.LogDebug("Updated item location: Path={Path}, SackType={SackType}, SackNumber={SackNumber}, StashType={StashType}",
            item.Place.Path, destinationSackType, item.Place.SackNumber, destinationStashType);
    }

    /// <inheritdoc/>
    public bool Autosort(SackCollection sack, Size sackSize, bool vertical)
    {
        var sortedItems = sack
            .OrderByDescending(item => ((item.Height * 3) + item.Width) * 100)
            .ThenByDescending(item => item.ItemGroup)
            .ThenBy(item => item.BaseItemId)
            .ThenByDescending(item => item.IsRelicComplete)
            .ToList();

        if (sortedItems.Count == 0)
            return true;

        SackCollection backUpSack = sack.Duplicate();
        sack.EmptySack();

        // Rows then columns, or columns then rows.
        int outer = vertical ? sackSize.Width : sackSize.Height;
        int inner = vertical ? sackSize.Height : sackSize.Width;

        foreach (Item item in sortedItems)
        {
            if (!PlaceSorted(sack, item, sackSize, vertical, outer, inner))
            {
                // Put the sack back as it was.
                sack.EmptySack();
                foreach (Item backUp in backUpSack)
                    sack.AddItem(backUp.Clone());

                sack.IsModified = false;
                return false;
            }
        }

        return true;
    }

    /// <summary>Places an item in the first free cell of a sort, scanning <paramref name="outer"/> lines of <paramref name="inner"/> cells.</summary>
    private bool PlaceSorted(SackCollection sack, Item item, Size sackSize, bool vertical, int outer, int inner)
    {
        int offset = vertical ? item.Width - 1 : item.Height - 1;

        for (int j = 0; j < outer - offset; ++j)
        {
            for (int k = 0; k < inner; ++k)
            {
                var cells = vertical
                    ? new Rectangle(j, k, item.Width, item.Height)
                    : new Rectangle(k, j, item.Width, item.Height);
                var foundItems = FindAllItems(sack, cells)!;

                bool lineFits = vertical ? item.Width + j <= sackSize.Width : item.Height + j <= sackSize.Height;
                if (foundItems.Count == 0 && lineFits)
                {
                    bool cellFits = vertical ? k + item.Height <= sackSize.Height : k + item.Width <= sackSize.Width;
                    if (cellFits)
                    {
                        item.PositionX = cells.X;
                        item.PositionY = cells.Y;
                        sack.AddItem(item);
                        return true;
                    }

                    // No room left on this line.
                    break;
                }

                // Skip over the item in the way.
                k += (vertical ? foundItems[0].Height : foundItems[0].Width) - 1;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public bool MoveItems(SackCollection sack, IReadOnlyCollection<Item> items, Size sackSize, Point offset)
    {
        foreach (Item item in items)
        {
            var movedItemArea = new Rectangle(new Point(item.PositionX + offset.X, item.PositionY + offset.Y), item.Size);

            // Another item in the way
            foreach (Item blockingItem in FindAllItems(sack, movedItemArea)!)
            {
                if (!items.Contains(blockingItem))
                    return false;
            }

            // Outside of the sack
            if (movedItemArea.Top < 0 || movedItemArea.Left < 0
                || movedItemArea.Bottom - 1 >= sackSize.Height || movedItemArea.Right - 1 >= sackSize.Width)
            {
                return false;
            }
        }

        foreach (Item item in items)
        {
            item.PositionX += offset.X;
            item.PositionY += offset.Y;
        }

        return true;
    }

    #region Private Helper Methods

    /// <summary>
    /// Find all items that are at least partially within the cell rectangle.
    /// Extracted from SackPanel.FindAllItems().
    /// </summary>
    private System.Collections.ObjectModel.Collection<Item>? FindAllItems(SackCollection sack, Rectangle cellRectangle)
    {
        var items = new System.Collections.ObjectModel.Collection<Item>();
        int minX = cellRectangle.X;
        int maxX = cellRectangle.X + cellRectangle.Width;
        int minY = cellRectangle.Y;
        int maxY = cellRectangle.Y + cellRectangle.Height;

        for (int x = minX; x < maxX; ++x)
        {
            for (int y = minY; y < maxY; ++y)
            {
                var item = FindItem(sack, new Point(x, y));

                if (item != null)
                {
                    // Add this item to the collection if it is not already in there
                    if (!items.Contains(item))
                    {
                        items.Add(item);
                    }

                    y = item.Height + item.Location.Y - 1;
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Gets an item at a cell location.
    /// </summary>
    /// <param name="sack">The sack to look into.</param>
    /// <param name="cellLocation">The cell coordinates.</param>
    /// <param name="ignoredItem">An item to skip, such as the item being dragged.</param>
    /// <returns>The item covering the cell, or null.</returns>
    public Item? FindItem(SackCollection sack, Point cellLocation, Item? ignoredItem = null)
    {
        if (sack == null)
            return null;

        // Find the item for this point
        foreach (var item in sack)
        {
            if (ignoredItem is not null && item == ignoredItem)
                continue;

            if (RecordId.IsNullOrEmpty(item.BaseItemId))
                continue;

            int x = item.PositionX;
            int y = item.PositionY;

            // See if this item overlaps the cell
            if ((x <= cellLocation.X) && ((x + item.Width - 1) >= cellLocation.X) &&
                (y <= cellLocation.Y) && ((y + item.Height - 1) >= cellLocation.Y))
            {
                return item;
            }
        }

        return null;
    }

    private static ItemMoveResult ItemMoveResultFailed(string errorMessage, Item? item = null, SackCollection? destinationSack = null)
        => ItemMoveResult.Failed(errorMessage, item, destinationSack);

    #endregion
}
