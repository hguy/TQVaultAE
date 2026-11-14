//-----------------------------------------------------------------------
// <copyright file="TexConverter.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers.Binary;
using ImageMagick;

namespace TQVaultAE.DbCli.Core;

/// <summary>Texture conversion.</summary>
public static partial class TexConverter
{
	// DDS layout, offsets from the start of the file (magic included).
	internal const int DdsHeaderSize = 128;
	internal const int OffHeaderSize = 4;
	internal const int OffPfSize = 76;
	internal const int OffPfFlags = 80;
	internal const int OffPfFourCC = 84;
	internal const int OffPfBitCount = 88;
	internal const int OffPfRMask = 92;
	internal const int OffPfGMask = 96;
	internal const int OffPfBMask = 100;
	internal const int OffPfAMask = 104;
	internal const int OffCaps1 = 108;

	internal const uint DdpfAlphaPixels = 0x1;
	internal const uint DdpfFourCC = 0x4;
	internal const uint DdpfRgb = 0x40;
	internal const uint DdsCapsTexture = 0x1000;

	private const uint MagicDds = 0x20534444; // "DDS "
	private const uint MagicDdsr = 0x52534444; // "DDSR"

	/// <summary>Number of frames in a .tex (animated textures have several).</summary>
	public static int FrameCount(ReadOnlySpan<byte> tex) => Frames(tex).Count;

	/// <summary>Valid .dds for one frame: magic fixed to "DDS ", RGB(A) masks and ALPHAPIXELS filled for uncompressed formats, DDSCAPS_TEXTURE set.</summary>
	public static byte[] ToDds(ReadOnlySpan<byte> tex, int frame = 0)
	{
		var frames = Frames(tex);
		if ((uint)frame >= (uint)frames.Count)
			throw new ArgumentOutOfRangeException(nameof(frame), $"Texture has {frames.Count} frame(s).");
		var (offset, length) = frames[frame];
		var dds = tex.Slice(offset, length).ToArray();
		FixDdsHeader(dds);
		return dds;
	}

	/// <summary>PNG of one frame (Magick.NET).</summary>
	public static byte[] ToPng(ReadOnlySpan<byte> tex, int frame = 0)
	{
		var dds = ToDds(tex, frame);
		using var image = new MagickImage(dds, MagickFormat.Dds);
		return image.ToByteArray(MagickFormat.Png);
	}

	/// <summary>Frame ranges (offset, length) of the embedded DDS images.</summary>
	internal static List<(int Offset, int Length)> Frames(ReadOnlySpan<byte> tex)
	{
		if (tex.Length < 8 || tex[0] != (byte)'T' || tex[1] != (byte)'E' || tex[2] != (byte)'X')
			throw new InvalidDataException("Not a TEX texture.");
		int pos = tex[3] switch {
			1 => 8, // 'TEX' 01, u32 fps
			2 => 9, // 'TEX' 02, u32 fps, u8 flag
			_ => throw new InvalidDataException($"Unsupported TEX version {tex[3]}."),
		};

		var frames = new List<(int, int)>();
		while (pos + 4 <= tex.Length) {
			int size = BinaryPrimitives.ReadInt32LittleEndian(tex[pos..]);
			pos += 4;
			if (size <= 0)
				break;
			if (size > tex.Length - pos)
				throw new InvalidDataException($"TEX frame {frames.Count} declares {size} bytes, only {tex.Length - pos} left.");
			frames.Add((pos, size));
			pos += size;
		}
		if (frames.Count == 0)
			throw new InvalidDataException("TEX contains no frame.");
		return frames;
	}

	/// <summary>In-place DDS header fix-ups (same logic as the GUI's BitmapService.LoadFromTexMemory).</summary>
	internal static void FixDdsHeader(Span<byte> dds)
	{
		if (dds.Length < DdsHeaderSize)
			throw new InvalidDataException("DDS frame shorter than its header.");
		uint magic = BinaryPrimitives.ReadUInt32LittleEndian(dds);
		if (magic != MagicDds && magic != MagicDdsr)
			throw new InvalidDataException($"Unknown texture magic 0x{magic:X8}.");
		if (BinaryPrimitives.ReadInt32LittleEndian(dds[OffHeaderSize..]) != 124 || BinaryPrimitives.ReadInt32LittleEndian(dds[OffPfSize..]) != 32)
			throw new InvalidDataException("Invalid DDS header.");

		BinaryPrimitives.WriteUInt32LittleEndian(dds, MagicDds);

		uint flags = BinaryPrimitives.ReadUInt32LittleEndian(dds[OffPfFlags..]);
		int bitCount = BinaryPrimitives.ReadInt32LittleEndian(dds[OffPfBitCount..]);
		if ((flags & DdpfFourCC) == 0 && bitCount >= 24) {
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfRMask..], 0x00FF0000);
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfGMask..], 0x0000FF00);
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfBMask..], 0x000000FF);
			flags |= DdpfRgb;
			if (bitCount == 32) {
				flags |= DdpfAlphaPixels;
				BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfAMask..], 0xFF000000);
			}
			BinaryPrimitives.WriteUInt32LittleEndian(dds[OffPfFlags..], flags);
		}

		uint caps = BinaryPrimitives.ReadUInt32LittleEndian(dds[OffCaps1..]);
		BinaryPrimitives.WriteUInt32LittleEndian(dds[OffCaps1..], caps | DdsCapsTexture);
	}
}
