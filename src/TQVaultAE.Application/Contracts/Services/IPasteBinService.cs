using System.Threading.Tasks;

namespace TQVaultAE.Application.Contracts.Services;

public interface IPasteBinService
{
	Task<string> UploadAsync(string text);
	Task<string> FetchPasteAsync(string pasteUrl);
}