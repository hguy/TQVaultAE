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
}