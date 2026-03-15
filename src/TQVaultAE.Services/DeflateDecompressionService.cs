using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Services;

public class DeflateDecompressionService : IDecompressionService
{
	private readonly ILogger _logger;

	public DeflateDecompressionService(ILogger<DeflateDecompressionService> logger)
	{
		this._logger = logger;
	}

	public byte[] DecompressZlib(ReadOnlySpan<byte> data)
	{
		if (data.Length < 2)
			return Array.Empty<byte>();

		try
		{
			var skipZlibHeader = data.Length > 6 && data[0] == 0x78 && (data[1] == 0x01 || data[1] == 0x9C || data[1] == 0xDA);
			var compressedData = skipZlibHeader ? data.Slice(2) : data;

			using var output = new MemoryStream();
			using (var deflate = new DeflateStream(new MemoryStream(compressedData.ToArray()), CompressionMode.Decompress))
			{
				deflate.CopyTo(output);
			}
			return output.ToArray();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error decompressing zlib data");
			return Array.Empty<byte>();
		}
	}

	public byte[] DecompressZlib(Memory<byte> data)
	{
		return DecompressZlib(data.Span);
	}
}
