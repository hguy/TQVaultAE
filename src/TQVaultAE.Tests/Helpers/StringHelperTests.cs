using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Helpers;

public class StringHelperTests
{
	[Fact]
	public void MakeMD5_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Test with various strings
		string[] testInputs = { "hello", "test", "password123", "TQVaultAE" };

		foreach (var input in testInputs)
		{
			// Act: Compute using old method (original implementation)
			string oldHash = MakeMD5Old(input);

			// Compute using new method (optimized implementation)
			string newHash = input.MakeMD5();

			// Assert: Both should produce identical results
			newHash.Should().Be(oldHash, because: $"MD5 should match for input '{input}'");
		}
	}

	[Fact]
	public void MakeMD5_KnownInput_ProducesKnownHash()
	{
		// Arrange: "hello" produces known MD5 hash
		string input = "hello";
		string expectedHash = "5d41402abc4b2a76b9719d911017c592";

		// Act
		string result = input.MakeMD5();

		// Assert
		result.Should().Be(expectedHash);
	}

	[Fact]
	public void MakeMD5_EmptyString_ProducesValidHash()
	{
		// Arrange: empty string has known MD5
		string input = "";
		string expectedHash = "d41d8cd98f00b204e9800998ecf8427e";

		// Act
		string result = input.MakeMD5();

		// Assert
		result.Should().Be(expectedHash);
	}

	[Fact]
	public void MakeMD5_UnicodeString_ProducesValidHash()
	{
		// Arrange: Unicode string
		string input = "你好世界"; // "Hello World" in Chinese

		// Act
		string result = input.MakeMD5();

		// Assert: Should produce a valid 32-character hex string
		result.Should().HaveLength(32);
		result.Should().MatchRegex("^[a-f0-9]{32}$");
	}

	[Fact]
	public void MakeMD5_LongString_ProducesValidHash()
	{
		// Arrange: Long string
		string input = new string('a', 10000);

		// Act
		string result = input.MakeMD5();

		// Assert: Should produce valid hash
		result.Should().HaveLength(32);
		result.Should().MatchRegex("^[a-f0-9]{32}$");
	}

	/// <summary>
	/// OLD method: Original implementation without StringBuilder pre-allocation
	/// </summary>
	private static string MakeMD5Old(string input)
	{
		using var md5Hash = MD5.Create();

		// Convert the input string to a byte array and compute the hash.
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		StringBuilder sBuilder = new StringBuilder();

		for (int i = 0; i < data.Length; i++)
			sBuilder.Append(data[i].ToString("x2"));

		return sBuilder.ToString();
	}

	#region ToFirstCharUpperCase Tests

	[Fact]
	public void ToFirstCharUpperCase_String_UppercasesFirstChar()
	{
		// Arrange
		var input = "defensiveCold";

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("DefensiveCold");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_UppercasesFirstChar()
	{
		// Arrange
		var input = "offensivePhysical".AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("OffensivePhysical");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_SingleChar_ReturnsUppercase()
	{
		// Arrange
		var input = "a".AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("A");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_Empty_ReturnsEmpty()
	{
		// Arrange
		var input = ReadOnlySpan<char>.Empty;

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_AlreadyUppercase_StaysUppercase()
	{
		// Arrange
		var input = "DEFENSIVEBLOCKRECOVERY".AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().Be("DEFENSIVEBLOCKRECOVERY");
	}

	[Fact]
	public void ToFirstCharUpperCase_Span_LongString_UsesStackalloc()
	{
		// Arrange - String longer than 64 chars triggers stackalloc path
		var input = new string('x', 100).AsSpan();

		// Act
		var result = input.ToFirstCharUpperCase();

		// Assert
		result.Should().StartWith("X");
		result.Should().HaveLength(100);
	}

	[Fact]
	public void ToFirstCharUpperCase_StringAndSpan_ProduceSameResult()
	{
		// Arrange
		var input = "blockRecoveryTime";

		// Act
		var stringResult = input.ToFirstCharUpperCase();
		var spanResult = input.AsSpan().ToFirstCharUpperCase();

		// Assert
		spanResult.Should().Be(stringResult);
	}

	#endregion

	#region RemoveSuffix Tests

	[Fact]
	public void RemoveSuffix_Span_RemovesSuffix()
	{
		// Arrange
		var input = "defensiveColdRatio".AsSpan();

		// Act
		var result = input.RemoveSuffix(5); // Remove "Ratio"

		// Assert
		result.Should().Be("defensiveCold");
	}

	[Fact]
	public void RemoveSuffix_Span_ExactLength_ReturnsEmpty()
	{
		// Arrange
		var input = "cold".AsSpan();

		// Act
		var result = input.RemoveSuffix(4);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void RemoveSuffix_Span_TooLong_ReturnsEmpty()
	{
		// Arrange
		var input = "abc".AsSpan();

		// Act
		var result = input.RemoveSuffix(10);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void RemoveSuffix_String_CallsSpanOverload()
	{
		// Arrange
		var input = "offensivePhysical";

		// Act
		var result = input.RemoveSuffix(8); // Remove "Physical"

		// Assert
		result.Should().Be("offensive");
	}

	[Fact]
	public void RemoveSuffix_StringAndSpan_ProduceSameResult()
	{
		// Arrange
		var input = "retaliationSlowPhysical";

		// Act
		var stringResult = input.RemoveSuffix(8);
		var spanResult = input.AsSpan().RemoveSuffix(8);

		// Assert
		spanResult.Should().Be(stringResult);
	}

	#endregion

	#region ConcatSlice Tests

	[Fact]
	public void ConcatSlice_SkipPrefix_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "Defense".AsSpan();
		var span = "defensiveCold".AsSpan();

		// Act - Skip "defensive" (9 chars), keep "Cold"
		var result = StringHelper.ConcatSlice(prefix, span, 9);

		// Assert
		result.Should().Be("DefenseCold");
	}

	[Fact]
	public void ConcatSlice_SkipPrefix_WithExplicitLength_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "Damage".AsSpan();
		var span = "offensiveChaosModifier".AsSpan();

		// Act - Skip "offensive" (9 chars), take "Chaos" (5 chars)
		var result = StringHelper.ConcatSlice(prefix, span, 9, 5);

		// Assert
		result.Should().Be("DamageChaos");
	}

	[Fact]
	public void ConcatSlice_EmptyPrefix_ReturnsSlice()
	{
		// Arrange
		var prefix = ReadOnlySpan<char>.Empty;
		var span = "blockRecovery".AsSpan();

		// Act
		var result = StringHelper.ConcatSlice(prefix, span, 5); // Skip "block"

		// Assert
		result.Should().Be("Recovery");
	}

	[Fact]
	public void ConcatSlice_EmptySuffix_ReturnsPrefix()
	{
		// Arrange
		var prefix = "Defense".AsSpan();
		var span = "defensive".AsSpan();

		// Act - Skip entire span
		var result = StringHelper.ConcatSlice(prefix, span, 9);

		// Assert
		result.Should().Be("Defense");
	}

	[Fact]
	public void ConcatSlice_BothEmpty_ReturnsEmpty()
	{
		// Act
		var result = StringHelper.ConcatSlice(
			ReadOnlySpan<char>.Empty,
			ReadOnlySpan<char>.Empty,
			0);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ConcatSlice_LongSuffix_ConcatenatesCorrectly()
	{
		// Arrange
		var prefix = "Skill".AsSpan();
		var span = "skillFireball".AsSpan();

		// Act - Skip "skill" (5 chars)
		var result = StringHelper.ConcatSlice(prefix, span, 5);

		// Assert
		result.Should().Be("SkillFireball");
	}

	#endregion

	#region ContainsIgnoreCase Tests

	[Fact]
	public void ContainsIgnoreCase_Span_FindsSubstring()
	{
		// Arrange
		var input = "defensiveCold".AsSpan();

		// Act & Assert
		input.ContainsIgnoreCase("cold".AsSpan()).Should().BeTrue();
		input.ContainsIgnoreCase("COLD".AsSpan()).Should().BeTrue();
		input.ContainsIgnoreCase("Cold".AsSpan()).Should().BeTrue();
		input.ContainsIgnoreCase("fire".AsSpan()).Should().BeFalse();
	}

	[Fact]
	public void ContainsIgnoreCase_String_FindsSubstring()
	{
		// Arrange
		var input = "offensivePhysicalDamage";

		// Act & Assert
		input.ContainsIgnoreCase("physical").Should().BeTrue();
		input.ContainsIgnoreCase("PHYSICAL").Should().BeTrue();
		input.ContainsIgnoreCase("damage").Should().BeTrue();
	}

	[Fact]
	public void ContainsIgnoreCase_String_NotFound_ReturnsFalse()
	{
		// Arrange
		var input = "blockRecoveryTime";

		// Act
		var result = input.ContainsIgnoreCase("lightning");

		// Assert
		result.Should().BeFalse();
	}

	#endregion
}