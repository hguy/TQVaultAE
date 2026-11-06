using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IIconService
{
	/// <summary>
	/// Icons from the game files, loaded once on a background thread.
	/// </summary>
	/// <remarks>The first call starts the load; it is called after startup so the icons are ready when the bag button settings open.</remarks>
	Task<ReadOnlyCollection<IconInfo>> GetIconDatabaseAsync();
}