using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation.Win32.Models;

/// <summary>
/// A bag button icon: its On, Off and Over pictures. The pictures are decoded the first time they are asked for.
/// </summary>
public sealed record IconInfo(IconCategory Category, RecordId On, RecordId Off, RecordId Over)
{
	/// <summary>Decodes a picture of the icon; set by the icon service.</summary>
	public Func<RecordId, Bitmap> BitmapLoader { private get; init; } = _ => throw new InvalidOperationException("No bitmap loader.");

	/// <summary>The game archives hold the On picture.</summary>
	public bool HasOn { get; init; }

	/// <summary>The game archives hold the Off picture.</summary>
	public bool HasOff { get; init; }

	/// <summary>The game archives hold the Over picture.</summary>
	public bool HasOver { get; init; }

	public Bitmap? OnBitmap => this.HasOn ? this.BitmapLoader(this.On) : null;

	public Bitmap? OffBitmap => this.HasOff ? this.BitmapLoader(this.Off) : null;

	public Bitmap? OverBitmap => this.HasOver ? this.BitmapLoader(this.Over) : null;

	/// <summary>Picture <paramref name="resourceId"/> of this icon, decoded on first use.</summary>
	public Bitmap BitmapOf(RecordId resourceId) => this.BitmapLoader(resourceId);

	/// <summary>
	/// Tell if <see cref="Over"/> is a duplicate of <see cref="On"/> or <see cref="Off"/>
	/// </summary>
	public bool IsOverSameAsOthers
		=> Over == Off || Over == On;

	public bool IsEmpty
		=> !this.HasOn && !this.HasOff && !this.HasOver;

	/// <summary>
	/// Tell if <paramref name="resourceId"/> belong to this record
	/// </summary>
	/// <param name="resourceId"></param>
	/// <returns></returns>
	public bool Own(RecordId resourceId)
	{
		if (RecordId.IsNullOrEmpty(resourceId)) return false;
		return resourceId == this.Off || resourceId == this.On || resourceId == this.Over;
	}
}