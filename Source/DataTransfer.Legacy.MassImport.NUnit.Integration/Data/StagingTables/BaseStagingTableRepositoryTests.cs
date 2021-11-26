using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using NUnit.Framework;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.StagingTables;

namespace MassImport.NUnit.Integration.Data.StagingTables
{
	public abstract class BaseStagingTableRepositoryTests : EmptyWorkspaceTestBase
	{
		protected TableNames TableNames { get; private set; }

		private protected ImportMeasurements ImportMeasurements { get; private set; }

		private protected abstract BaseStagingTableRepository CreateSut();


		[SetUp]
		public void SetUp()
		{
			TableNames = new TableNames();
			ImportMeasurements = new ImportMeasurements();
		}

		[Test]
		public async Task ShouldReadNumberOfChoicesPerCodeTypeIdWhenStagingTableIsEmpty()
		{
			// arrange
			await CreateStagingTableForChoicesAsync(TableNames).ConfigureAwait(false);

			try
			{
				var sut = CreateSut();

				// act
				var actual = sut.ReadNumberOfChoicesPerCodeTypeId();

				// assert
				Assert.That(actual, Is.Empty);
			}
			finally
			{
				await DeleteStagingTableForChoicesAsync(TableNames).ConfigureAwait(false);
			}
		}

		[Test]
		public async Task ShouldReadNumberOfChoicesPerCodeTypeId()
		{
			// arrange
			await CreateStagingTableForChoicesAsync(TableNames).ConfigureAwait(false);

			try
			{
				int firstCodeTypeId = 1;
				var choicesForFirstCodeType = new Dictionary<string, int[]>
				{
					["DOC1"] = new[] { 1, 2, 3 },
					["DOC2"] = new[] { 4242, 23 },
					["DOC4"] = new[] { 3 }
				};
				await this.InsertChoicesToStagingTableAsync(firstCodeTypeId, choicesForFirstCodeType).ConfigureAwait(false);

				int secondCodeTypeId = 2;
				var choicesForSecondCodeType = new Dictionary<string, int[]>
				{
					["DOC1"] = new[] { 1, 2, 3 },
					["DOC3"] = new[] { 13 }
				};
				await this.InsertChoicesToStagingTableAsync(secondCodeTypeId, choicesForSecondCodeType).ConfigureAwait(false);

				var sut = CreateSut();

				// act
				var actual = sut.ReadNumberOfChoicesPerCodeTypeId();

				// assert
				Dictionary<int, int> expectedResult = new Dictionary<int, int>
				{
					[firstCodeTypeId] = 3 + 2 + 1,
					[secondCodeTypeId] = 3 + 1
				};
				Assert.That(actual, Is.EquivalentTo(expectedResult));
			}
			finally
			{
				await DeleteStagingTableForChoicesAsync(TableNames).ConfigureAwait(false);
			}
		}

		private Task CreateStagingTableForChoicesAsync(TableNames tableNames)
		{
			string createTableQuery = $@"
			CREATE TABLE [Resource].[{tableNames.Code}] (
				[DocumentIdentifier] NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
				[CodeArtifactID] INT NOT NULL,
				[CodeTypeID] INT NOT NULL,

				INDEX IX_CodeTypeID_DocumentIdentifier CLUSTERED
				(
					[CodeTypeID] ASC,
					[DocumentIdentifier] ASC
				)
			);";

			return EddsdboContext.ExecuteNonQueryAsync(new QueryInformation { Statement = createTableQuery });
		}

		private Task DeleteStagingTableForChoicesAsync(TableNames tableNames)
		{
			string dropTableQuery = $"DROP TABLE [Resource].[{tableNames.Code}];";

			return EddsdboContext.ExecuteNonQueryAsync(new QueryInformation { Statement = dropTableQuery });
		}

		private Task InsertChoicesToStagingTableAsync(int codeTypeId,
			Dictionary<string, int[]> documentsToChoicesMapping)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("DocumentIdentifier", typeof(string)));
			dataTable.Columns.Add(new DataColumn("CodeArtifactID", typeof(int)));
			dataTable.Columns.Add(new DataColumn("CodeTypeID", typeof(int)));

			var rows = documentsToChoicesMapping
				.Select(x => x.Value.Select(codeArtifactId => new { DocumentId = x.Key, CodeArtifactId = codeArtifactId }))
				.SelectMany(x => x);
			foreach (var row in rows)
			{
				dataTable.Rows.Add(row.DocumentId, row.CodeArtifactId, codeTypeId);
			}

			var parameters = new SqlBulkCopyParameters { DestinationTableName = $"[Resource].[{TableNames.Code}]" };
			return EddsdboContext.ExecuteBulkCopyAsync(dataTable, parameters, CancellationToken.None);
		}
	}
}