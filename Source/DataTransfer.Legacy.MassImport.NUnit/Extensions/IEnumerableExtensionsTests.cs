using System.Linq;
using DataTransfer.Legacy.MassImport.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace DataTransfer.Legacy.MassImport.NUnit.Extensions
{
	[TestFixture]
	public class IEnumerableExtensionsTests
	{
		[Test]
		public void Batch_ShouldReturnCorrectNumberOfBatches()
		{
			// Arrange
			var source = Enumerable.Range(1, 10);
			var batchSize = 2;

			// Act
			var result = source.Batch(batchSize);

			// Assert
			result.Should().HaveCount(5);
		}

		[Test]
		public void Batch_ShouldReturnCorrectBatchSizes()
		{
			// Arrange
			var source = Enumerable.Range(1, 10);
			var batchSize = 3;

			// Act
			var result = source.Batch(batchSize);

			// Assert
			foreach (var batch in result)
			{
				batch.Should().HaveCountLessOrEqualTo(batchSize);
			}
		}

		[Test]
		public void Batch_ShouldReturnCorrectBatches()
		{
			// Arrange
			var source = Enumerable.Range(1, 10);
			var batchSize = 3;

			// Act
			var result = source.Batch(batchSize);

			// Assert
			var expected = new[]
			{
				new[] { 1, 2, 3 },
				new[] { 4, 5, 6 },
				new[] { 7, 8, 9 },
				new[] { 10 }
			};

			result.Should().BeEquivalentTo(expected);
		}

		[Test]
		public void Batch_ShouldReturnEmptyList_WhenSourceIsEmpty()
		{
			// Arrange
			var source = Enumerable.Empty<int>();
			var batchSize = 2;

			// Act
			var result = source.Batch(batchSize);

			// Assert
			result.Should().BeEmpty();
		}
	}
}