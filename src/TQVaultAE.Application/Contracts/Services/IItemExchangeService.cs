using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

public interface IItemExchangeService
{
	string SerializeItem(Item item);
	string EncodeToClipboardPayload(string json);
	string DecodeFromClipboardPayload(string base64);
	ImportResult ImportFromJson(string json);
	bool IsPasteBinUrl(string text);
}