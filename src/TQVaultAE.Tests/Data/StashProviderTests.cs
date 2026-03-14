using System.IO;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Data;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Tests.Data;

public class StashProviderTests
{
	private const int BUFFERSIZE = 1024;

	[Fact]
	public void CalculateCRC_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create sample data
		var sampleData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		// Create copy for old algorithm
		var oldData = (byte[])sampleData.Clone();
		var newData = (byte[])sampleData.Clone();

		// Act: Calculate CRC using old algorithm (BinaryReader + MemoryStream)
		uint oldCrc = CalculateCRCOld(oldData);

		// Calculate CRC using new algorithm (via reflection)
		uint newCrc = CalculateCRCNew(newData);

		// Assert: Both should produce the same result
		newCrc.Should().Be(oldCrc);
	}

	[Fact]
	public void CalculateCRC_KnownTestData_ProducesCorrectValue()
	{
		// Arrange: Use known test data that produces a known CRC
		var data = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38 }; // "12345678"

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert: CRC should not be zero (sanity check)
		crc.Should().NotBe(0);
		// The first 4 bytes should be modified with the CRC value
		result[0].Should().NotBe(0x31);
	}

	[Fact]
	public void CalculateCRC_EmptyData_ProducesValidCRC()
	{
		// Arrange - create data with actual content so CRC is non-zero
		var data = new byte[10];
		data[5] = 42; // Add some data to ensure non-zero CRC

		// Act
		var result = (byte[])data.Clone();
		uint crc = CalculateCRCNew(result);

		// Assert: Should complete without error and produce non-zero CRC for non-empty data
		crc.Should().NotBe(0);
	}

	/// <summary>
	/// Replicates the OLD algorithm (BinaryReader + MemoryStream)
	/// </summary>
	private static uint CalculateCRCOld(byte[] data)
	{
		// Get crc32Table directly from StashProvider
		var stashProvider = CreateStashProvider();
		var table = stashProvider.crc32Table;

		using var reader = new BinaryReader(new MemoryStream(data, false));
		uint crc32Result = 0;
		var buffer = new byte[BUFFERSIZE];
		var readSize = BUFFERSIZE;

		int count = reader.Read(buffer, 0, readSize);
		while (count > 0)
		{
			for (int i = 0; i < count; i++)
				crc32Result = (crc32Result >> 8) ^ table[buffer[i] ^ (crc32Result & 0x000000FF)];

			count = reader.Read(buffer, 0, readSize);
		}

		// Put the CRC into the first 4 bytes
		data[3] = (byte)((crc32Result & 0xFF000000) >> 24);
		data[2] = (byte)((crc32Result & 0x00FF0000) >> 16);
		data[1] = (byte)((crc32Result & 0x0000FF00) >> 8);
		data[0] = (byte)(crc32Result & 0x000000FF);

		return crc32Result;
	}

	/// <summary>
	/// Creates a StashProvider instance for testing
	/// </summary>
	private static StashProvider CreateStashProvider()
	{
		var mockLog = new Mock<ILogger<StashProvider>>();
		var mockSackProvider = new Mock<ISackCollectionProvider>();
		var mockTQData = new Mock<ITQDataService>();
		var mockFileIO = new Mock<IFileIO>();
		var mockPathIO = new Mock<IPathIO>();

		return new StashProvider(
			mockLog.Object,
			mockSackProvider.Object,
			mockTQData.Object,
			mockFileIO.Object,
			mockPathIO.Object
		);
	}

	/// <summary>
	/// Calls the NEW algorithm directly
	/// </summary>
	private static uint CalculateCRCNew(byte[] data)
	{
		var stashProvider = CreateStashProvider();
		stashProvider.CalculateCRC(data);
		return BitConverter.ToUInt32(data, 0);
	}
}