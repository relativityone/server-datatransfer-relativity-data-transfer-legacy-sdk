using System.Linq;
using NUnit.Framework;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

namespace MassImport.NUnit.Integration.Data.SqlFramework
{
	[TestFixture]
	public class QueryMetricsCollectorTests : EmptyWorkspaceTestBase
	{
		[Test]
		public void ShouldMeasureExecutionTime()
		{
			// arrange
			const string metricName = "TestMetric";

			var importMeasurements = new ImportMeasurements();

			ISqlQueryPart query = new InlineSqlQuery($"SELECT 1;");
			query = new PrintSectionQuery(query, metricName);
			query = new StatisticsTimeOnQuery(query);

			// act
			using (new QueryMetricsCollector(EddsdboContext, importMeasurements))
			{
				EddsdboContext.ExecuteSqlStatementAsScalar<int>(query.BuildQuery());
			}

			// assert
			var measureNames = importMeasurements.GetMeasures().Select(x => x.Key);
			Assert.That(measureNames, Contains.Item(metricName), "Metric is not present");
		}

		[Test]
		public void ShouldMeasureTwoExecutionTimes()
		{
			// arrange
			const string firstMetricName = "TestMetric";
			const string secondMetricName = "SecondTestMetric";

			var importMeasurements = new ImportMeasurements();

			ISqlQueryPart firstQuery = new InlineSqlQuery($"SELECT 1;");
			firstQuery = new PrintSectionQuery(firstQuery, firstMetricName);
			
			ISqlQueryPart secondQuery = new InlineSqlQuery($"SELECT 2;");
			
			ISqlQueryPart thirdQuery = new InlineSqlQuery($"SELECT 3;");
			thirdQuery = new PrintSectionQuery(thirdQuery, secondMetricName);
			
			ISqlQueryPart query = new SerialSqlQuery(firstQuery, secondQuery, thirdQuery);
			query = new StatisticsTimeOnQuery(query);

			// act
			using (new QueryMetricsCollector(EddsdboContext, importMeasurements))
			{
				EddsdboContext.ExecuteNonQuerySQLStatement(query.BuildQuery());
			}

			// assert
			var measureNames = importMeasurements.GetMeasures().Select(x => x.Key).ToArray();
			Assert.That(measureNames, Contains.Item(firstMetricName), "Metric is not present");
			Assert.That(measureNames, Contains.Item(secondMetricName), "Metric is not present");
		}

		[Test]
		public void ShouldNotMeasureExecutionTimeWhenDisposed()
		{
			// arrange
			var importMeasurements = new ImportMeasurements();

			var metricName = "TestMetric";
			ISqlQueryPart query = new InlineSqlQuery($"SELECT 1;");
			query = new PrintSectionQuery(query, metricName);
			query = new StatisticsTimeOnQuery(query);

			// act
			using (new QueryMetricsCollector(EddsdboContext, importMeasurements))
			{
				ISqlQueryPart queryWithoutMetrics = new InlineSqlQuery($"SELECT 0;");
				EddsdboContext.ExecuteSqlStatementAsScalar<int>(queryWithoutMetrics.BuildQuery());
			}

			EddsdboContext.ExecuteSqlStatementAsScalar<int>(query.BuildQuery());

			// assert
			Assert.That(importMeasurements.GetMeasures(), Is.Empty, "Query was executed after QueryMetricsCollector was disposed");

		}
	}
}
