using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	[TestOf(typeof(ImportMeasurements))]
	public class ImportMeasurementsTests
	{
		[Test(Description = "ImportMeasurements should parse time statistics.")]
		public void ShouldParseTimeStatistics()
		{
			// Arrange
			var importMeasurements = new ImportMeasurements();

			// Act
			importMeasurements.ParseTimeStatistics(@"
Mass Import Section: Section1
 SQL Server Execution Times:
   CPU time = 0 ms,  elapsed time = 1 ms.
 SQL Server Execution Times:
   CPU time = 0 ms,  elapsed time = 2 ms.

 SQL Server Execution Times:
   CPU time = 0 ms,  elapsed time = 3 ms.

Mass Import Section: Section2
 SQL Server Execution Times:
   CPU time = 0 ms,  elapsed time = 4 ms.
");

			// Assert
			CollectionAssert.AreEquivalent(
				new[]
				{
					new KeyValuePair<string, long>("Section1", 6L),
					new KeyValuePair<string, long>("Section2", 4L)
				},
				importMeasurements.GetMeasures());
		}

		[Test]
		public void ShouldReturnEmptyCollectionWhenNoCounters()
		{
			// arrange
			var sut = new ImportMeasurements();

			// act
			IEnumerable<KeyValuePair<string, int>> actualCounters = sut.GetCounters();

			// assert
			Assert.That(actualCounters, Is.Empty);
		}

		[Test]
		public void ShouldReturnSingeCounterIncrementedOnce()
		{
			// arrange
			const string counterName = "testCounter";

			var sut = new ImportMeasurements();
			sut.IncrementCounter(counterName);

			// act
			List<KeyValuePair<string, int>> actualCounters = sut.GetCounters().ToList();

			// assert
			Assert.That(actualCounters, Has.One.Items);

			var counter = actualCounters.Single();
			Assert.That(counter.Key, Is.EqualTo(counterName));
			Assert.That(counter.Value, Is.EqualTo(1));
		}

		[Test]
		public void ShouldReturnSingeCounterIncrementedTwice()
		{
			// arrange
			const string counterName = "testCounter";

			var sut = new ImportMeasurements();
			sut.IncrementCounter(counterName);
			sut.IncrementCounter(counterName);

			// act
			List<KeyValuePair<string, int>> actualCounters = sut.GetCounters().ToList();

			// assert
			Assert.That(actualCounters, Has.One.Items);

			var counter = actualCounters.Single();
			Assert.That(counter.Key, Is.EqualTo(counterName));
			Assert.That(counter.Value, Is.EqualTo(2));
		}

		[Test]
		public void ShouldReturnTwoCounters()
		{
			// arrange
			const string firstCounterName = "testCounter";
			const string secondCounterName = "second";

			var sut = new ImportMeasurements();
			sut.IncrementCounter(firstCounterName);
			sut.IncrementCounter(secondCounterName);
			sut.IncrementCounter(firstCounterName);

			// act
			List<KeyValuePair<string, int>> actualCounters = sut.GetCounters().ToList();

			// assert
			Assert.That(actualCounters, Has.Count.EqualTo(2));

			var firstCounter = actualCounters.Single(x => x.Key == firstCounterName);
			var secondCounter = actualCounters.Single(x => x.Key == secondCounterName);
			Assert.That(firstCounter.Value, Is.EqualTo(2));
			Assert.That(secondCounter.Value, Is.EqualTo(1));
		}
	}
}
