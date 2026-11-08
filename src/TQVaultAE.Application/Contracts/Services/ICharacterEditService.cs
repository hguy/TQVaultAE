using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Applies the character edit dialog to a character: level, experience, difficulty, attributes and masteries.
/// </summary>
public interface ICharacterEditService
{
	/// <summary>
	/// Builds the edited player info of <paramref name="source"/>, without changing it.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>The masteries allowed follow the level: none under 2, one under 8, else two.</item>
	/// <item>Experience text that is not a number keeps the experience of <paramref name="source"/>.</item>
	/// <item>A reset of the attributes puts them back to their minimum and adds the points invested to the pool of
	/// <paramref name="source"/>.</item>
	/// <item>A reset of the masteries removes their skills and gives back their skill points.</item>
	/// <item>Unlocking another difficulty raises the money to 5M for Epic, 7.5M for Legendary.</item>
	/// </list>
	/// </remarks>
	PlayerInfo BuildPlayerInfo(PlayerInfo source, CharacterEditRequest request);

	/// <summary>
	/// Applies <paramref name="request"/> to the character of <paramref name="player"/> and writes it to its raw data.
	/// </summary>
	/// <returns>false when the character has no player info.</returns>
	bool Apply(PlayerCollection player, CharacterEditRequest request);
}
