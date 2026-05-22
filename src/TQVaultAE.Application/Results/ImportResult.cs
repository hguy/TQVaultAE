using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

public class ImportResult
{
	public bool Success { get; set; }
	public string Scope { get; set; }
	public Item Item { get; set; }
	public string ErrorMessage { get; set; }

	public static ImportResult Succeeded(Item item)
		=> new()
		{
			Success = true,
			Scope = "item",
			Item = item
		};

	public static ImportResult Failed(string errorMessage)
		=> new()
		{
			Success = false,
			ErrorMessage = errorMessage
		};
}