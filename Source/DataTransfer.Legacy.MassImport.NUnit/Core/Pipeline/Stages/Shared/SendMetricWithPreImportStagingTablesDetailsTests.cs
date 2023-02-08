using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Data.MassImport;
using Relativity.Logging;
using Relativity.MassImport.Core;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Stages.Shared
{
	using Relativity.API;

	[TestFixture]
	public class SendMetricWithPreImportStagingTablesDetailsTests
	{
		private MassImportContext _massImportContext;
		private Mock<ILog> _loggerMock;
		private Mock<IStagingTableRepository> _stagingTableRepositoryMock;
		private Mock<IMassImportMetricsService> _metricsServiceMock;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<ILog>();
			_loggerMock
				.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
				.Returns(_loggerMock.Object);

			_massImportContext = new MassImportContext(
				baseContext: null, // not used by SendMetricWithPreImportStagingTablesDetails
				new LoggingContext("correlationId", "clientName", _loggerMock.Object),
				new MassImportJobDetails(new TableNames(), clientSystemName: "test", importType: "test"),
				caseSystemArtifactId: -1, // not used by SendMetricWithPreImportStagingTablesDetails
				new Mock<IHelper>().Object
			);

			_stagingTableRepositoryMock = new Mock<IStagingTableRepository>();
			_metricsServiceMock = new Mock<IMassImportMetricsService>();
		}

		[Test]
		public void ShouldReadStatisticsFromRepositoryAndSendMetrics()
		{
			// arrange
			const int input = 88;
			var numberOfChoicesPerCodeTypeId = new Dictionary<int, int>
			{
				[45] = 1,
				[1] = 4
			};
			_stagingTableRepositoryMock
				.Setup(x => x.ReadNumberOfChoicesPerCodeTypeId())
				.Returns(numberOfChoicesPerCodeTypeId);

			// this stage does not use input value
			var sut = new SendMetricWithPreImportStagingTablesDetails<int>(
				_massImportContext,
				_stagingTableRepositoryMock.Object,
				_metricsServiceMock.Object);

			// act
			int result = sut.Execute(input);

			// assert
			var expectedCustomData = new Dictionary<string, object>
			{
				["NumberOfChoices"] = 5,
				["NumberOfChoices_45"] = 1,
				["NumberOfChoices_1"] = 4
			};
			string expectedCorrelationId = _massImportContext.JobDetails.CorrelationId;

			Assert.That(result, Is.EqualTo(input), "this stage does not modify output, it should be equal to the input value");
			_metricsServiceMock.Verify(x => x.SendPreImportStagingTableStatistics(
				expectedCorrelationId,
				It.Is<Dictionary<string, object>>(actualCustomData => AssertCustomDataIsValid(expectedCustomData, actualCustomData))
			));
		}

		[Test]
		public void ShouldNotThrowWhenRepositoryThrows()
		{
			// arrange
			const int input = 88;
			var expectedException = new InvalidOperationException("test");
			_stagingTableRepositoryMock
				.Setup(x => x.ReadNumberOfChoicesPerCodeTypeId())
				.Throws(expectedException);

			// this stage does not use input value
			var sut = new SendMetricWithPreImportStagingTablesDetails<int>(
				_massImportContext,
				_stagingTableRepositoryMock.Object,
				_metricsServiceMock.Object);

			// act
			int result = sut.Execute(input);

			// assert
			Assert.That(result, Is.EqualTo(input), "this stage does not modify output, it should be equal to the input value");
			_loggerMock.Verify(x => x.LogWarning(expectedException, It.IsAny<string>()));
		}

		[Test]
		public void ShouldNotThrowWhenMetricsServiceThrows()
		{
			// arrange
			const int input = 88;
			var expectedException = new InvalidOperationException("test");
			_stagingTableRepositoryMock
				.Setup(x => x.ReadNumberOfChoicesPerCodeTypeId())
				.Returns(new Dictionary<int, int>());

			_metricsServiceMock
				.Setup(x => x.SendPreImportStagingTableStatistics(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
				.Throws(expectedException);

			// this stage does not use input value
			var sut = new SendMetricWithPreImportStagingTablesDetails<int>(
				_massImportContext,
				_stagingTableRepositoryMock.Object,
				_metricsServiceMock.Object);

			// act
			int result = sut.Execute(input);

			// assert
			Assert.That(result, Is.EqualTo(input), "this stage does not modify output, it should be equal to the input value");
			_loggerMock.Verify(x => x.LogWarning(expectedException, It.IsAny<string>()));
		}

		private bool AssertCustomDataIsValid(Dictionary<string, object> expected, Dictionary<string, object> actual)
		{
			Assert.That(actual, Is.EquivalentTo(expected));
			return true;
		}
	}
}
