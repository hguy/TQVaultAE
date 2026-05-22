using System.Net.Http;
using System.Threading.Tasks;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;

namespace TQVaultAE.Services;

public class PasteBinService : IPasteBinService
{
	private readonly HttpClient _httpClient;
	private readonly UserSettings _userSettings;

	private const string PasteBinApiUrl = "https://pastebin.com/api/api_post.php";
	private const string PasteBinRawUrl = "https://pastebin.com/raw/{0}";

	public PasteBinService(HttpClient httpClient, UserSettings userSettings)
	{
		_httpClient = httpClient;
		_userSettings = userSettings;
	}

	public async Task<string> UploadAsync(string text)
	{
		var content = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["api_dev_key"] = _userSettings.PasteBinApiKey ?? string.Empty,
			["api_option"] = "paste",
			["api_paste_code"] = text,
			["api_paste_private"] = "1",
			["api_paste_expire_date"] = _userSettings.PasteBinExpiration ?? "1M"
		});

		var response = await _httpClient.PostAsync(PasteBinApiUrl, content);
		response.EnsureSuccessStatusCode();

		var result = await response.Content.ReadAsStringAsync();

		if (result.StartsWith("https://pastebin.com/", System.StringComparison.OrdinalIgnoreCase))
			return result.Trim();

		throw new InvalidOperationException($"PasteBin API error: {result}");
	}

	public async Task<string> FetchPasteAsync(string pasteUrl)
	{
		var pasteId = GetPasteIdFromUrl(pasteUrl);
		var rawUrl = string.Format(PasteBinRawUrl, pasteId);

		var response = await _httpClient.GetAsync(rawUrl);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	private static string GetPasteIdFromUrl(string url)
	{
		var uri = new Uri(url);
		return uri.AbsolutePath.Trim('/');
	}
}