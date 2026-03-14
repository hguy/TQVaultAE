using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Logs;

namespace TQVaultAE.Services;

public class LegacyFileDataService : IFileDataService
{
	private readonly ILogger _logger;

	public LegacyFileDataService(ILogger<LegacyFileDataService> logger)
	{
		this._logger = logger;
	}

	public ReadOnlySpan<byte> GetReadOnlySpan(string filePath, int offset, int length)
	{
		try
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream.Seek(offset, SeekOrigin.Begin);
			var buffer = new byte[length];
			int bytesRead = stream.Read(buffer, 0, length);
			if (bytesRead < length)
				Array.Resize(ref buffer, bytesRead);
			return buffer;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading file at offset {Offset}, length {Length}", offset, length);
			return ReadOnlySpan<byte>.Empty;
		}
	}

	public Memory<byte> GetMemory(string filePath, int offset, int length)
	{
		try
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream.Seek(offset, SeekOrigin.Begin);
			var buffer = new byte[length];
			int bytesRead = stream.Read(buffer, 0, length);
			if (bytesRead < length)
				Array.Resize(ref buffer, bytesRead);
			return buffer;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading file at offset {Offset}, length {Length}", offset, length);
			return Memory<byte>.Empty;
		}
	}

	public T Read<T>(string filePath, int offset) where T : unmanaged
	{
		try
		{
			var size = Marshal.SizeOf<T>();
			var buffer = new byte[size];
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream.Seek(offset, SeekOrigin.Begin);
			stream.Read(buffer, 0, size);

			return MemoryMarshal.Read<T>(buffer);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading type {Type} at offset {Offset}", typeof(T).Name, offset);
			return default;
		}
	}

	public void ReleaseAll()
	{
	}
}
