using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Relativity.MassImport.Api;
using Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables;

namespace Relativity.MassImport.NUnit.Core
{
	[TestFixture]
	public class MassImportReaderTests
	{
		[Test]
		public void MassImportReaderSimpleDataTest()
		{
			//arrange
			var mappingColumns = Enumerable.Range(0, 16).Select(columnIndex => new SqlBulkCopyColumnMapping(columnIndex, columnIndex));

			var artifacts = new List<MassImportArtifact>()
			{
				new MassImportArtifact(fieldValues: new List<object>() { "DOC1", 123, new [] { 1, 2 }, new DateTime(2020,9,8), "DataGridText1", 233.45 }, parentFolderId: 1001),
				new MassImportArtifact(fieldValues: new List<object>() { "DOC2", 234, new [] { 3, 4 }, new DateTime(2020,10,9), "DataGridText2", 344.56 }, parentFolderId: 4001),
			};
			var fieldIndexes = new int[] { 0, 1, -1, 3, 5 };
			var appArtifactId = 1001;
			var rootCaseArtifactId = 1002;

			var expectedArtifact1 = new List<object> { 0, 0, 0, 0, 1, string.Empty, string.Empty, System.DBNull.Value, System.DBNull.Value, 0, 1002, "DOC1", 123, null, new DateTime(2020, 9, 8), 233.45 };
			var expectedArtifact2 = new List<object> { 0, 0, 0, 0, 2, string.Empty, string.Empty, System.DBNull.Value, System.DBNull.Value, 0, 4001, "DOC2", 234, null, new DateTime(2020, 10, 9), 344.56 };

			//act
			MassImportReader _sut = new MassImportReader(mappingColumns, artifacts, fieldIndexes, appArtifactId, rootCaseArtifactId);

			//assert
			Assert.True(_sut.Read());
			var actualArtifact1 = Enumerable.Range(0, 16).Select(_sut.GetColumnValue).ToList();
			CollectionAssert.AreEqual(expectedArtifact1, actualArtifact1);
			Assert.True(_sut.Read());
			var actualArtifact2 = Enumerable.Range(0, 16).Select(_sut.GetColumnValue).ToList();
			CollectionAssert.AreEqual(expectedArtifact2, actualArtifact2);
			Assert.False(_sut.Read());
		}
	}
}
