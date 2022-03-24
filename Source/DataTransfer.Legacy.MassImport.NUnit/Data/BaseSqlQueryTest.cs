using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using kCura.Data;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Data.Caching;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.NUnit.Data
{
	internal abstract class BaseSqlQueryTest
	{
		private const string KeyFieldName = "KeyFieldName";
		private const int SingleObjectArtifactId = 100123;
		private const string SingleObjectFieldName = "SingleObjectFieldName";
		private const string SingleObjectTableName = "SingleObjectTableName";
		private const int TopLevelParentArtifactId = 11;
		private const int TopLevelParentAccessControlListId = 21;

		protected const int UserId = 9;
		protected const int AuditUserId = 9999;
		protected const int CaseArtifactId = 10000;
		protected const string RequestOrigination = "RequestOrigination";
		protected const string RecordOrigination = "RecordOrigination";
		protected const bool PerformAudit = true;
		protected const string SingleObjectIdFieldColumnName = "SingleObjectIdFieldColumnName";
		protected const int ArtifactTypeId = 1000050;
		protected const string RunId = "DCD09DF6-A4C7-4DF0-B963-0050C7809038";
		
		protected FieldInfo SingleObjectField = new FieldInfo();
		protected Mock<BaseContext> ContextMock { get; private set; }
		protected ColumnDefinitionCache ColumnDefinitionCache { get; private set; }

		[SetUp]
		protected void BaseSetUp()
		{
			ContextMock = new Mock<BaseContext>();
			ContextMock.Setup(ct => ct.GetConnection()).Returns(new SqlConnection("data source=localhost;initial catalog=FakeDb;user id=Userdbo;password=pwd; workstation id=localhost;"));
			ContextMock.Setup(ct => ct.IsMasterDatabase).Returns(false);
			ContextMock.Setup(ct => ct.Database).Returns("EDDS");

			ColumnDefinitionCache = new ColumnDefinitionCache(this.ContextMock.Object);
		}

		public static bool ThenSQLsAreEqual(string actual, string expected)
		{
			string actualNormalized = Regex.Replace(actual, @"[\s\r\n\t]", "");
			string expectedNormalized = Regex.Replace(expected, @"[\s\r\n\t]", "");

			Assert.That(actualNormalized, Is.EqualTo(expectedNormalized), "SQL query is different than expected");
			return true;
		}

		protected Relativity.MassImport.DTO.ObjectLoadInfo InitializeSettings()
		{
			const int keyFieldArtifactId = 0;
			var keyField = new FieldInfo();
			keyField.Category = FieldCategory.Identifier;
			keyField.DisplayName = KeyFieldName;
			keyField.ArtifactID = keyFieldArtifactId;
			SingleObjectField = new FieldInfo();
			SingleObjectField.DisplayName = SingleObjectFieldName;
			SingleObjectField.ArtifactID = SingleObjectArtifactId;
			var objectLoadInfo = new Relativity.MassImport.DTO.ObjectLoadInfo();
			objectLoadInfo.RunID = RunId;
			objectLoadInfo.MappedFields = new FieldInfo[] { keyField, SingleObjectField };
			objectLoadInfo.KeyFieldArtifactID = keyField.ArtifactID;
			objectLoadInfo.ArtifactTypeID = ArtifactTypeId;

			return objectLoadInfo;
		}

		protected DataTable CreateSimpleDataTableMock()
		{
			var dataTableMock = new DataTable();
			dataTableMock.Columns.Add("FirstColumn");
			dataTableMock.Rows.Add(SingleObjectIdFieldColumnName);

			return dataTableMock;
		}

		protected DataTable CreateDataTableMock(int fieldArtifactTypeId)
		{
			var mockCacheDataTable = new DataTable();
			mockCacheDataTable.Columns.Add("CollationName");
			mockCacheDataTable.Columns.Add("UseUnicodeEncoding");
			mockCacheDataTable.Columns.Add("ActualFieldLength");
			mockCacheDataTable.Columns.Add("OriginalArtifactID");
			mockCacheDataTable.Columns.Add("OverlayMergeValues");
			mockCacheDataTable.Columns.Add("ActualColumnName");
			mockCacheDataTable.Columns.Add("ActualLinkArg");
			mockCacheDataTable.Columns.Add("ObjectTypeName");
			mockCacheDataTable.Columns.Add("ObjectTypeArtifactTypeID");
			mockCacheDataTable.Columns.Add("ContainingObjectField");
			mockCacheDataTable.Columns.Add("NewObjectField");
			mockCacheDataTable.Columns.Add("RelationalTableSchemaName");
			mockCacheDataTable.Columns.Add("AssociatedParentID");
			mockCacheDataTable.Columns.Add("TopLevelParentArtifactId");
			mockCacheDataTable.Columns.Add("TopLevelParentAccessControlListId");

			var singleObjectRow = mockCacheDataTable.NewRow();
			singleObjectRow["OriginalArtifactID"] = SingleObjectArtifactId;
			singleObjectRow["ActualColumnName"] = SingleObjectIdFieldColumnName;
			singleObjectRow["ObjectTypeName"] = SingleObjectTableName;
			singleObjectRow["ObjectTypeArtifactTypeID"] = fieldArtifactTypeId;
			mockCacheDataTable.Rows.Add(singleObjectRow);

			var globalFieldRow = mockCacheDataTable.NewRow();
			globalFieldRow["OriginalArtifactID"] = -1;
			globalFieldRow["TopLevelParentArtifactId"] = TopLevelParentArtifactId;
			globalFieldRow["TopLevelParentAccessControlListId"] = TopLevelParentAccessControlListId;
			mockCacheDataTable.Rows.Add(globalFieldRow);

			return mockCacheDataTable;
		}
	}
}