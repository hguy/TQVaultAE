using System.Drawing;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation.Win32.Contracts;

/// <summary>
/// Item bitmaps and UI scaling for the Windows user interface, plus the user notifications of <see cref="IUserNotifier"/>.
/// </summary>
public interface IUIService : IUserNotifier
{
	/// <summary>
	/// Gets the default bitmap
	/// </summary>
	Bitmap DefaultBitmap { get; }

	/// <summary>
	/// Gets the half of an item unit size which is the unit of measure of item size in TQ.
	/// Division takes place after internal scaling by db scale.
	/// </summary>
	int HalfUnitSize { get; }

	/// <summary>
	/// Gets the UI design DPI which is used to for scaling comparisons.
	/// </summary>
	/// <remarks>Use 96 DPI which is "normal" for Windows.</remarks>
	float DESIGNDPI { get; }

	/// <summary>
	/// Gets the item unit size which is the unit of measure of item size in TQ.
	/// An item with a ItemUnitSize x ItemUnitSize bitmap would be 1x1.
	/// Internally scaled by db scale.
	/// </summary>
	int ItemUnitSize { get; }

	/// <summary>
	/// Gets or sets the scaling of the UI
	/// </summary>
	float Scale { get; set; }

	/// <summary>
	/// Gets the item's bitmap
	/// </summary>
	Bitmap GetBitmap(Item itm);

	/// <summary>
	/// Loads a bitmap from a resource Id string
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <returns>Bitmap fetched from the database</returns>
	Bitmap LoadBitmap(RecordId resourceId);

	/// <summary>
	/// Loads a bitmap from <paramref name="texData"/> with an identifier <paramref name="resourceId"/>
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <param name="texData">raw DDS image data</param>
	/// <returns>Bitmap converted from <paramref name="texData"/></returns>
	Bitmap LoadBitmap(RecordId resourceId, byte[] texData);

	/// <summary>
	/// Loads the relic overlay bitmap from the database.
	/// </summary>
	/// <returns>Relic overlay bitmap</returns>
	Bitmap LoadRelicOverlayBitmap();
}
