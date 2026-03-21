using System.Buffers.Binary;
using System.IO;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

public class TQDataServiceTests
{
	private readonly TQDataService _tqData;

	public TQDataServiceTests()
	{
		var mockLogger = new Mock<ILogger<TQDataService>>();
		_tqData = new TQDataService(mockLogger.Object);
	}

	#region ReadCString Tests

	[Fact]
	public void ReadCString_Span_EmptyString_ReturnsEmpty()
	{
		// Arrange: length=0
		byte[] data = new byte[]
		{
			0x00, 0x00, 0x00, 0x00  // length = 0
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().BeEmpty();
		offset.Should().Be(4); // 4 bytes for length
	}

	[Fact]
	public void ReadCString_Span_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange: "test"
		byte[] data = new byte[]
		{
			0x04, 0x00, 0x00, 0x00,  // length = 4
			0x74, 0x65, 0x73, 0x74   // "test"
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be("test");
		offset.Should().Be(8); // 4 bytes length + 4 bytes data
	}

	[Fact]
	public void ReadCString_Span_ExtendedCharacters_ReturnsCorrectString()
	{
		// Arrange: "café" with extended characters
		string input = "café";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(input);
		byte[] data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void ReadCString_Span_MultipleStringsInSequence_ParsesAll()
	{
		// Arrange: ["first", "second", ""]
		var strings = new[] { "first", "second", "" };
		// Each string needs: 4 bytes (length) + byte count
		var totalLength = strings.Sum(s => 4 + TQDataService.Encoding1252.GetByteCount(s));
		var data = new byte[totalLength];
		int pos = 0;

		foreach (var str in strings)
		{
			var bytes = TQDataService.Encoding1252.GetBytes(str);
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), bytes.Length);
			pos += 4;
			bytes.CopyTo(data, pos);
			pos += bytes.Length;
		}

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result1 = _tqData.ReadCString(span, ref offset);
		var result2 = _tqData.ReadCString(span, ref offset);
		var result3 = _tqData.ReadCString(span, ref offset);

		// Assert
		result1.Should().Be("first");
		result2.Should().Be("second");
		result3.Should().BeEmpty();
		offset.Should().Be(data.Length);
	}

	/// <summary>
	/// Verifies ReadCString with known C-string data.
	/// Note: BinaryWriter uses different format, so we test with direct data construction.
	/// </summary>
	[Fact]
	public void ReadCString_Span_KnownData_ProducesCorrectResult()
	{
		// Arrange: "Hello" with CP1252 encoding
		string testString = "Hello";
		byte[] stringBytes = TQDataService.Encoding1252.GetBytes(testString);
		var data = new byte[4 + stringBytes.Length];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), stringBytes.Length);
		stringBytes.CopyTo(data, 4);

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadCString(span, ref offset);

		// Assert
		result.Should().Be(testString);
	}

	#endregion

	#region ReadUTF16String Tests

	[Fact]
	public void ReadUTF16String_Span_EmptyString_ReturnsEmpty()
	{
		// Arrange: charCount = 0
		byte[] data = new byte[]
		{
			0x00, 0x00, 0x00, 0x00  // charCount = 0
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().BeEmpty();
		offset.Should().Be(4); // 4 bytes for charCount
	}

	[Fact]
	public void ReadUTF16String_Span_SimpleAsciiString_ReturnsCorrectString()
	{
		// Arrange: "test" in UTF-16 LE (4 characters)
		byte[] data = new byte[]
		{
			0x04, 0x00, 0x00, 0x00,  // charCount = 4
			0x74, 0x00,              // 't'
			0x65, 0x00,              // 'e'
			0x73, 0x00,              // 's'
			0x74, 0x00               // 't'
		};
		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be("test");
		offset.Should().Be(12); // 4 + (4 * 2)
	}

	[Fact]
	public void ReadUTF16String_Span_UnicodeCharacters_ReturnsCorrectString()
	{
		// Arrange: "tëst" with special character
		string input = "tëst";
		int charCount = input.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);

		// Write UTF-16 LE
		Encoding.Unicode.GetBytes(input, data.AsSpan(4, charCount * 2));

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be(input);
	}

	[Fact]
	public void ReadUTF16String_Span_MultipleStringsInSequence_ParsesAll()
	{
		// Arrange: ["alpha", "beta"]
		var strings = new[] { "alpha", "beta" };
		// Each string needs: 4 bytes (char count) + (charCount * 2) bytes (UTF-16)
		var totalLength = strings.Sum(s => 4 + s.Length * 2);
		var data = new byte[totalLength];
		int pos = 0;

		foreach (var str in strings)
		{
			BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(pos, 4), str.Length);
			pos += 4;
			Encoding.Unicode.GetBytes(str, data.AsSpan(pos, str.Length * 2));
			pos += str.Length * 2;
		}

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result1 = _tqData.ReadUTF16String(span, ref offset);
		var result2 = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result1.Should().Be("alpha");
		result2.Should().Be("beta");
		offset.Should().Be(data.Length);
	}

	/// <summary>
	/// Verifies ReadUTF16String with known UTF-16 data.
	/// Note: BinaryWriter uses different format, so we test with direct data construction.
	/// </summary>
	[Fact]
	public void ReadUTF16String_Span_KnownData_ProducesCorrectResult()
	{
		// Arrange: UTF-16 LE string "Test"
		string testString = "Test";
		int charCount = testString.Length;
		var data = new byte[4 + charCount * 2];
		BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0, 4), charCount);
		Encoding.Unicode.GetBytes(testString, data.AsSpan(4, charCount * 2));

		ReadOnlySpan<byte> span = data;
		int offset = 0;

		// Act
		var result = _tqData.ReadUTF16String(span, ref offset);

		// Assert
		result.Should().Be(testString);
	}

	#endregion

	#region BinaryFindKey Tests

	/// <summary>
	/// OLD IMPLEMENTATION: Byte-by-byte comparison (for comparison testing)
	/// </summary>
	private static (int indexOf, int nextOffset) BinaryFindKeyOld(ReadOnlySpan<byte> dataSource, ReadOnlySpan<byte> key, int offset = 0)
	{
		int i = offset, j = 0;
		for (; i <= (dataSource.Length - key.Length); i++)
		{
			if (dataSource[i] == key[0])
			{
				j = 1;
				for (; j < key.Length && dataSource[i + j] == key[j]; j++) ;
				if (j == key.Length)
					goto found;

			}
		}
		// Not found
		return (-1, 0);
		found:
		return (i, i + key.Length);
	}

	[Fact]
	public void BinaryFindKey_FindsKeyAtStart()
	{
		// Arrange: Data with key at start
		var data = new byte[] { 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }; // "numItems"
		var key = TQDataService.Encoding1252.GetBytes("numItems");

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(0);
		result.nextOffset.Should().Be(8);
	}

	[Fact]
	public void BinaryFindKey_FindsKeyInMiddle()
	{
		// Arrange: Data with key in the middle
		var data = new byte[100];
		var key = TQDataService.Encoding1252.GetBytes("test");
		key.CopyTo(data, 50);

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(50);
		result.nextOffset.Should().Be(54);
	}

	[Fact]
	public void BinaryFindKey_NotFound_ReturnsNegativeOne()
	{
		// Arrange: Data without the key
		var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };
		var key = TQDataService.Encoding1252.GetBytes("missing");

		// Act
		var result = _tqData.BinaryFindKey(data, key);

		// Assert
		result.indexOf.Should().Be(-1);
	}

	[Fact]
	public void BinaryFindKey_OldAndNew_ProduceSameResult()
	{
		// Arrange: Various test cases
		var testCases = new[]
		{
			(new byte[] { 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }, TQDataService.Encoding1252.GetBytes("numItems")), // Key at start
			(new byte[] { 0x00, 0x00, 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 }, TQDataService.Encoding1252.GetBytes("numItems")), // Key in middle
			(new byte[] { 0x01, 0x02, 0x03, 0x04 }, TQDataService.Encoding1252.GetBytes("missing")), // Not found
		};

		foreach (var (data, key) in testCases)
		{
			// Act - OLD implementation
			var oldResult = BinaryFindKeyOld(data.AsSpan(), key.AsSpan());

			// Act - NEW implementation (uses SequenceEqual)
			var newResult = _tqData.BinaryFindKey(data, key);

			// Assert - Results should be identical
			newResult.indexOf.Should().Be(oldResult.indexOf, $"indexOf mismatch for key '{TQDataService.Encoding1252.GetString(key)}'");
			newResult.nextOffset.Should().Be(oldResult.nextOffset, $"nextOffset mismatch for key '{TQDataService.Encoding1252.GetString(key)}'");
		}
	}

	[Fact]
	public void BinaryFindKey_WithOffset_SkipsBytesCorrectly()
	{
		// Arrange: Data with key but offset before it
		var data = new byte[20];
		var key = TQDataService.Encoding1252.GetBytes("test");
		key.CopyTo(data, 10);

		// Act - Search starting at offset 5
		var result = _tqData.BinaryFindKey(data, key, 5);

		// Assert
		result.indexOf.Should().Be(10);
	}

	[Fact]
	public void BinaryFindKey_StringOverload_WorksCorrectly()
	{
		// Arrange: String overload expects data with length prefix (like BinaryWriter format)
		// "numItems" = 8 chars, so we need: 4 bytes (length=8) + 8 bytes ("numItems")
		var data = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x6E, 0x75, 0x6D, 0x49, 0x74, 0x65, 0x6D, 0x73 };

		// Act
		var result = _tqData.BinaryFindKey(data, "numItems");

		// Assert: Returns position after the length prefix
		result.indexOf.Should().Be(4);
		result.nextOffset.Should().Be(12);
	}

	#endregion
}
