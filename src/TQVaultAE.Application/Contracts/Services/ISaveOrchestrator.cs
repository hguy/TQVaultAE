using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Runs the save of the session: modified characters, vaults and stashes, then the configuration, then the git backup.
/// </summary>
public interface ISaveOrchestrator
{
	/// <summary>
	/// Saves what <paramref name="request"/> asks, in this order: characters, vaults, stashes, configuration,
	/// git commit and push (only when the git backup is enabled).
	/// When a character, vault or stash file cannot be written, <paramref name="onError"/> decides:
	/// <see cref="SaveDecision.Retry"/> runs that step again, <see cref="SaveDecision.Ignore"/> gives it up and goes on,
	/// <see cref="SaveDecision.Abort"/> rethrows the <see cref="IOException"/>, so the later steps do not run.
	/// </summary>
	/// <param name="request">What to save.</param>
	/// <param name="onError">Asked after each file error.</param>
	/// <exception cref="IOException">The user aborted after a file error.</exception>
	SaveResult Save(SaveRequest request, Func<SaveError, SaveDecision> onError);
}
