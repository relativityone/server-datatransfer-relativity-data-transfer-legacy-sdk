using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.NUnit.Data.Cache
{
	public class ColumnDefinitionCacheTests
	{
		private ColumnDefinitionCache _instanceUnderTest;

		private string CollationName = "SomeCollation";
		private bool UseUnicodeEncoding = false;
		private int ActualFieldLength = 1;
		private int OriginalArtifactID = 123456;
		private bool OverlayMergeValues = true;
		private string ActualColumnName = "Column Name";
		private string ActualLinkArg = "Another Col Name";
		private string ObjectTypeName = "Type Name";
		private int ObjectTypeArtifactTypeID = 2;
		private string ContainingObjectField = "Some Field";
		private string NewObjectField = "Object Field";
		private string RelationalTableSchemaName = "RelationalTableSchemaName";
		private int AssociatedParentID = 4;
		private int TopLevelParentArtifactId = 5;
		private int TopLevelParentAccessControlListId = 6;

		private Mock<BaseContext> _baseContextMock;

		[SetUp]
		public void InitTestCase()
		{
			_baseContextMock = new Mock<BaseContext>();
			_instanceUnderTest = new ColumnDefinitionCache(_baseContextMock.Object);
		}

		[Test]
		public void ItShouldLoadColumnDefinitionFromCache()
		{
			string runId = Guid.NewGuid().ToString();
			var fieldInfo = new FieldInfo()
			{
				ArtifactID = OriginalArtifactID,
				EnableDataGrid = false
			};

			_baseContextMock
				.Setup(x => x.ExecuteSqlStatementAsDataTable(ColumnDefinitionCache.GetCachedColumnDefinitionSql(runId), kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout))
				.Returns(this.CreateDataTableWithRow(GetColDefInfoCacheValues(UseUnicodeEncoding), GetColDefInfoCacheColumnList()));

			_instanceUnderTest.LoadDataFromCache(runId);
			_instanceUnderTest.ValidateFieldMapping(new[] { fieldInfo });

			ColumnDefinitionInfo returnedObject = _instanceUnderTest[OriginalArtifactID];

			Assert.That(returnedObject, Is.Not.Null);
			Assert.That(returnedObject.CollationName, Is.EqualTo(CollationName));
			Assert.That(returnedObject.UnicodeMarker, Is.EqualTo(string.Empty));
			Assert.That(returnedObject.FieldLength, Is.EqualTo((object)ActualFieldLength));
			Assert.That(returnedObject.OriginalArtifactID, Is.EqualTo((object)OriginalArtifactID));
			Assert.That(returnedObject.OverlayMergeValues, Is.EqualTo((object)OverlayMergeValues));
			Assert.That(returnedObject.ColumnName, Is.EqualTo(ActualColumnName));
			Assert.That(returnedObject.LinkArg, Is.EqualTo(ActualLinkArg));
			Assert.That(returnedObject.ObjectTypeName, Is.EqualTo(ObjectTypeName));
			Assert.That(returnedObject.ContainingObjectField, Is.EqualTo(ContainingObjectField));
			Assert.That(returnedObject.NewObjectField, Is.EqualTo(NewObjectField));
			Assert.That(returnedObject.RelationalTableSchemaName, Is.EqualTo(RelationalTableSchemaName));
			Assert.That(returnedObject.AssociatedParentID, Is.EqualTo((object)AssociatedParentID));
			Assert.That(returnedObject.TopLevelParentArtifactId, Is.EqualTo((object)TopLevelParentArtifactId));
			Assert.That(returnedObject.TopLevelParentAccessControlListId, Is.EqualTo((object)TopLevelParentAccessControlListId));
		}

		[Test(Description = "This tests checks that special fields like LongText in DG, LayoutText, File should not be validated")]
		public void ItShouldNotThrowExceptionForDgEnabledFieldNotinCache()
		{
			string runId = Guid.NewGuid().ToString();
			var fieldInfoDg = new FieldInfo()
			{
				ArtifactID = OriginalArtifactID,
				EnableDataGrid = true
			};

			var fieldInfoFile = new FieldInfo()
			{
				ArtifactID = OriginalArtifactID,
				Type = FieldTypeHelper.FieldType.File
			};

			var fieldInfoLayoutText = new FieldInfo()
			{
				ArtifactID = OriginalArtifactID,
				Type = FieldTypeHelper.FieldType.LayoutText
			};

			_baseContextMock
				.Setup(x => x.ExecuteSqlStatementAsDataTable(ColumnDefinitionCache.GetCachedColumnDefinitionSql(runId), kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout))
				.Returns(new DataTable());
			
			_instanceUnderTest.LoadDataFromCache(runId);
			Assert.DoesNotThrow(() => _instanceUnderTest.ValidateFieldMapping(new[] { fieldInfoDg, fieldInfoFile, fieldInfoLayoutText }));
		}

		[Test]
		public void ItShouldThrowExceptionForFieldNotInCache()
		{
			_instanceUnderTest = new ColumnDefinitionCache(_baseContextMock.Object);

			string runId = Guid.NewGuid().ToString();
			var fieldInfo = new FieldInfo()
			{
				ArtifactID = OriginalArtifactID,
				EnableDataGrid = false
			};

			_baseContextMock
				.Setup(x => x.ExecuteSqlStatementAsDataTable(ColumnDefinitionCache.GetCachedColumnDefinitionSql(runId), kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout))
				.Returns(new DataTable());

			_instanceUnderTest.LoadDataFromCache(runId);

			Assert.Throws<KeyNotFoundException>(() => _instanceUnderTest.ValidateFieldMapping(new[] { fieldInfo }));
		}

		private string[] GetColDefInfoCacheColumnList()
		{
			return new[]
			{
				"CollationName",
				"UseUnicodeEncoding",
				"ActualFieldLength",
				"OriginalArtifactID",
				"OverlayMergeValues",
				"ActualColumnName",
				"ActualLinkArg",
				"ObjectTypeName",
				"ObjectTypeArtifactTypeID",
				"ContainingObjectField",
				"NewObjectField",
				"RelationalTableSchemaName",
				"AssociatedParentID",
				"TopLevelParentArtifactId",
				"TopLevelParentAccessControlListId"
			};
		}

		private object[] GetColDefInfoCacheValues(bool unicode)
		{
			return new object[]
			{
				CollationName,
				unicode,
				ActualFieldLength,
				OriginalArtifactID,
				OverlayMergeValues,
				ActualColumnName,
				ActualLinkArg,
				ObjectTypeName,
				ObjectTypeArtifactTypeID,
				ContainingObjectField,
				NewObjectField,
				RelationalTableSchemaName,
				AssociatedParentID,
				TopLevelParentArtifactId,
				TopLevelParentAccessControlListId
			};
		}

		private DataTable CreateDataTableWithRow(object[] objectList, string[] colNames = null)
		{
			var dataTable = CreateDataTableWithCols(objectList, colNames);
			var dataRow = dataTable.NewRow();
			for (int index = 0, loopTo = objectList.Count() - 1; index <= loopTo; index++)
			{
				dataRow[index] = objectList[index];
			}
			dataTable.Rows.Add(dataRow);
			return dataTable;
		}

		private DataTable CreateDataTableWithCols(object[] objectList, string[] colNames)
		{
			var dataTable = new DataTable();
			for (int index = 0, loopTo = objectList.Count() - 1; index <= loopTo; index++)
			{
				dataTable.Columns.Add(colNames is null ? string.Format("Column{0}", index) : colNames[index], objectList[index].GetType());
			}
			return dataTable;
		}
	}
}