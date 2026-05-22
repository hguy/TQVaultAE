using System.Threading.Tasks;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IItemExchangeService
{
	string SerializeItem(Item item);
	string SerializeSackCollection(SackCollection sack, int sackNumber);
	string SerializePlayerCollection(PlayerCollection vault);
	string EncodeToClipboardPayload(string json);
	string DecodeFromClipboardPayload(string base64);
	ImportResult ImportFromJson(string json);
	string DetectScope(string json);
	void ImportVaultInto(PlayerCollection vault, ImportResult importData);
	bool IsPasteBinUrl(string text);
	bool HasPasteBinApiKey { get; }
	Task<string> ExportToPasteBinAsync(string json);
	Task<string> ImportFromPasteBinAsync(string pasteUrl);
}