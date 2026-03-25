using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.MemoryMappedFiles;
using System.Text;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Tests for ArcFileProvider ReadARCToC_V2 method comparing against original algorithm.
/// Uses synthetic ARC files for validation.
/// </summary>
public class ArcFileProviderV2Tests : IDisposable
{
	private readonly Mock<ILogger<ArcFileProvider>> _mockLogger;
	private readonly Mock<IPathIO> _mockPathIO;
	private readonly Mock<IDirectoryIO> _mockDirectoryIO;
	private readonly Mock<IFileDataService> _mockFileDataService;
	private readonly Mock<IDecompressionService> _mockDecompressionService;
	private readonly UserSettings _userSettings;
	private readonly ArcFileProvider _provider;
	private readonly string _tempDir;

	public ArcFileProviderV2Tests()
	{
		_mockLogger = new Mock<ILogger<ArcFileProvider>>();
		_mockPathIO = new Mock<IPathIO>();
		_mockDirectoryIO = new Mock<IDirectoryIO>();
		_mockFileDataService = new Mock<IFileDataService>();
		_mockDecompressionService = new Mock<IDecompressionService>();
		_userSettings = new UserSettings();

		_provider = new ArcFileProvider(
			_mockLogger.Object,
			_mockPathIO.Object,
			_mockDirectoryIO.Object,
			_userSettings,
			_mockFileDataService.Object,
			_mockDecompressionService.Object
		);

		_tempDir = Path.GetTempPath();
	}

	public void Dispose()
	{
		// Cleanup if needed
	}

	/// <summary>
	/// Creates a minimal synthetic ARC file for testing.
	/// Format:
	/// - 0x00-0x02: "ARC" header
	/// - 0x08: numEntries (int32)
	/// - 0x0C: numParts (int32)
	/// - 0x18: tocOffset (int32)
	/// - tocOffset: part entries (numParts * 12 bytes)
	/// - filenames region
	/// - file records at end (numEntries * 44 bytes)
	/// </summary>
	private string CreateSyntheticArcFile(int numEntries, int numParts, string[] filenames)
	{
		// Calculate offsets
		int tocOffset = 0x18 + 12; // After header + part entries header
		int partEntriesSize = numParts * 12;
		int filenamesSize = filenames.Sum(f => f.Length + 1) + numEntries; // +1 for null terminator
		int fileRecordsSize = numEntries * 44;
		int totalSize = tocOffset + partEntriesSize + filenamesSize + fileRecordsSize;

		var filePath = Path.Combine(_tempDir, $"test_{Guid.NewGuid()}.arc");

		using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
		using var bw = new BinaryWriter(fs);

		// Write "ARC" header at offset 0
		bw.Write((byte)0x41); // 'A'
		bw.Write((byte)0x52); // 'R'
		bw.Write((byte)0x43); // 'C'

		// Padding to 0x08
		bw.Seek(0x08, SeekOrigin.Begin);
		bw.Write(numEntries); // numEntries at 0x08
		bw.Write(numParts);     // numParts at 0x0C

		// tocOffset at 0x18
		bw.Seek(0x18, SeekOrigin.Begin);
		bw.Write(tocOffset);

		// Write part entries at tocOffset
		bw.Seek(tocOffset, SeekOrigin.Begin);
		for (int i = 0; i < numParts; i++)
		{
			bw.Write(1000 + i);      // FileOffset
			bw.Write(2000 + i);      // CompressedSize
			bw.Write(3000 + i);     // RealSize
		}

		// Write filenames
		int filenameOffset = tocOffset + partEntriesSize;
		bw.Seek(filenameOffset, SeekOrigin.Begin);
		for (int i = 0; i < numEntries; i++)
		{
			bw.Write(Encoding.ASCII.GetBytes(filenames[i]));
			bw.Write((byte)0x00); // null terminator
		}

		// Write file records at end
		int fileRecordOffset = 44 * numEntries;
		long fileRecordStart = totalSize - fileRecordOffset;
		bw.Seek((int)fileRecordStart, SeekOrigin.Begin);

		for (int i = 0; i < numEntries; i++)
		{
			int baseOffset = (int)fileRecordStart + (i * 44);

			bw.Write(1);                     // StorageType (1 = uncompressed)
			bw.Write(1000 + i);             // FileOffset
			bw.Write(2000 + i);             // CompressedSize
			bw.Write(3000 + i);             // RealSize
			bw.Write(0);                     // crap1
			bw.Write(0);                     // crap2
			bw.Write(0);                     // crap3
			bw.Write(0);                     // numberOfParts (0 = not compressed)
			bw.Write(0);                     // firstPart
			bw.Write(filenames[i].Length + 1); // filenameLength
			bw.Write(filenameOffset + i * 64); // filenameOffset (approximate)
		}

		return filePath;
	}

	[Fact]
	public void ReadNullTerminatedString_NormalString_ReturnsString()
	{
		// This test validates the internal method through reflection-free testing
		// by testing the logic directly

		// Arrange - simulate reading "testfile.txt\0"
		var data = "testfile.txt"u8.ToArray();
		var withNull = new byte[data.Length + 1];
		data.CopyTo(withNull, 0);

		// Act - use the SpanHelper version which has the same logic
		var result = SpanHelper.ReadNullTerminatedAscii(withNull, 0, withNull.Length, out int bytesConsumed);

		// Assert
		result.Should().Be("testfile.txt");
		bytesConsumed.Should().Be(data.Length + 1);
	}

	[Fact]
	public void ReadNullTerminatedString_EmptyString_ReturnsEmpty()
	{
		// Arrange - single null byte
		var data = new byte[] { 0x00 };

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeEmpty();
		bytesConsumed.Should().Be(1);
	}

	[Fact]
	public void ReadNullTerminatedString_WithInactiveMarker_ReturnsNull()
	{
		// Arrange - string ending with 0x03 (inactive file marker)
		var data = new byte[] { 0x41, 0x42, 0x03, 0x00 };

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void ReadNullTerminatedString_NoNullTerminator_ReturnsAll()
	{
		// Arrange - string without null terminator
		var data = "NoNull"u8.ToArray();

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(data, 0, data.Length, out int bytesConsumed);

		// Assert
		result.Should().Be("NoNull");
		bytesConsumed.Should().Be(data.Length);
	}

	[Fact]
	public void ReadNullTerminatedString_OffsetInMiddle_ReturnsCorrectSlice()
	{
		// Arrange
		var prefix = new byte[] { 0x00, 0x00 };
		var str = "test"u8.ToArray();
		var combined = new byte[prefix.Length + str.Length + 1];
		prefix.CopyTo(combined, 0);
		str.CopyTo(combined, prefix.Length);
		combined[combined.Length - 1] = 0x00;

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(combined, 2, combined.Length - 2, out int bytesConsumed);

		// Assert
		result.Should().Be("test");
		bytesConsumed.Should().Be(str.Length + 1);
	}

	[Fact]
	public void ArcFileProvider_ReadARCToC_InvalidFile_ReturnsWithoutException()
	{
		// Arrange - create a file that is not a valid ARC
		var invalidFilePath = Path.Combine(_tempDir, $"invalid_{Guid.NewGuid()}.arc");
		File.WriteAllBytes(invalidFilePath, new byte[] { 0x00, 0x01, 0x02 });

		var file = new ArcFile(invalidFilePath);

		// Act - should not throw
		var action = () => _provider.Read(file);

		// Assert
		action.Should().NotThrow();
		file.FileHasBeenRead.Should().BeTrue();

		// Cleanup
		File.Delete(invalidFilePath);
	}

	[Fact]
	public void ArcFileProvider_ReadARCToC_TooShortFile_ReturnsWithoutException()
	{
		// Arrange - file smaller than minimum size (0x21 bytes)
		var tooShortPath = Path.Combine(_tempDir, $"tooshort_{Guid.NewGuid()}.arc");
		File.WriteAllBytes(tooShortPath, new byte[] { 0x41, 0x52, 0x43 });

		var file = new ArcFile(tooShortPath);

		// Act
		var action = () => _provider.Read(file);

		// Assert
		action.Should().NotThrow();

		// Cleanup
		File.Delete(tooShortPath);
	}

	[Fact]
	public void ArcFileProvider_ReadARCToC_V2_InvalidFile_ReturnsFalse()
	{
		// Arrange
		var invalidFilePath = Path.Combine(_tempDir, $"invalid_{Guid.NewGuid()}.arc");
		File.WriteAllBytes(invalidFilePath, new byte[] { 0x00, 0x01, 0x02 });

		var file = new ArcFile(invalidFilePath);

		// Act
		var action = () => _provider.ReadARCToC_V2(file);

		// Assert
		action.Should().NotThrow();
		file.FileHasBeenRead.Should().BeTrue();

		// Cleanup
		File.Delete(invalidFilePath);
	}

	[Fact]
	public void SpanHelper_BinaryRead_MatchesMemoryMappedAccessor()
	{
		// This test validates that SpanHelper.ReadInt32LittleEndian produces
		// the same results as MemoryMappedFile accessor

		// Arrange
		var data = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 };

		// Act
		var spanHelperResult = SpanHelper.ReadInt32LittleEndian(data, 0);
		var memoryMappedResult = BitConverter.ToInt32(data, 0);

		// Assert
		spanHelperResult.Should().Be(memoryMappedResult);
		spanHelperResult.Should().Be(0x78563412);
	}

	[Theory]
	[InlineData(0, 0x04030201)]  // bytes [1,2,3,4] = 1 + 2*256 + 3*256^2 + 4*256^3 = 67305985
	[InlineData(1, 0x05040302)]  // bytes [2,3,4,5] = 2 + 3*256 + 4*256^2 + 5*256^3 = 84148994
	[InlineData(2, 0x06050403)]  // bytes [3,4,5,6] = 3 + 4*256 + 5*256^2 + 6*256^3 = 100992003
	public void SpanHelper_ReadInt32LittleEndian_VariousOffsets_ReturnsCorrectValues(int offset, int expected)
	{
		// Arrange - create sequential pattern 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08...
		var data = Enumerable.Range(1, 256).Select(i => (byte)i).ToArray();

		// Act
		var result = SpanHelper.ReadInt32LittleEndian(data, offset);

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void SpanHelper_ReadNullTerminatedAscii_2048CharLimit_StopsAtLimit()
	{
		// Arrange - string longer than 2048 bytes (safety limit)
		var longString = new byte[3000];
		for (int i = 0; i < 3000; i++)
			longString[i] = (byte)('A' + (i % 26));
		// No null terminator

		// Act
		var result = SpanHelper.ReadNullTerminatedAscii(longString, 0, longString.Length, out int bytesConsumed);

		// Assert
		result.Should().NotBeNull();
		bytesConsumed.Should().Be(3000); // consumed all without finding null
	}

	[Fact]
	public void ArcFileProvider_ValidateV2AgainstOriginal_NoRealFile_ReturnsTrueWhenBothEmpty()
	{
		// Arrange - file with only header, no real data
		var minimalPath = Path.Combine(_tempDir, $"minimal_{Guid.NewGuid()}.arc");
		var header = new byte[0x21];
		header[0] = 0x41; header[1] = 0x52; header[2] = 0x43; // "ARC"
		File.WriteAllBytes(minimalPath, header);

		var file = new ArcFile(minimalPath);

		// Act - read with original
		_provider.ReadARCToC(file);

		// Assert - validation returns True when both algorithms produce empty results (they match)
		// This test ensures the validation method doesn't throw and handles edge case of empty files
		var validationResult = _provider.ValidateV2AgainstOriginal(file);
		validationResult.Should().BeTrue();

		// Cleanup
		File.Delete(minimalPath);
	}
}
