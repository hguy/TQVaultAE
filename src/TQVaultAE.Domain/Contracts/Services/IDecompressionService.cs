using System;
using System.Buffers;

namespace TQVaultAE.Domain.Contracts.Services;

public interface IDecompressionService
{
	byte[] DecompressZlib(ReadOnlySpan<byte> data);
	byte[] DecompressZlib(Memory<byte> data);
}
