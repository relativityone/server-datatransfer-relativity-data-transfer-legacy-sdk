using System;
using NUnit.Framework;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.NUnit.Data
{
	public class ColumnDefinitionInfoTest
	{
		private ColumnDefinitionInfo _instanceUnderTest;
		private const string CollationName = "Some_collation";
		private const int FieldLength = 111;
		private const bool IsUnicodeEnabled = true;
		private const int OriginalArtifactID = 1;

		[SetUp()]
		public void Setup()
		{
			_instanceUnderTest = new ColumnDefinitionInfo();
			_instanceUnderTest.CollationName = CollationName;
			_instanceUnderTest.FieldLength = FieldLength;
			_instanceUnderTest.IsUnicodeEnabled = IsUnicodeEnabled;
			_instanceUnderTest.OriginalArtifactID = OriginalArtifactID;
		}

		[Test]
		[TestCase(FieldTypeHelper.FieldType.Object, "NVARCHAR(111) COLLATE Some_collation NULL")]
		[TestCase(FieldTypeHelper.FieldType.Integer, "INT NULL")]
		[TestCase(FieldTypeHelper.FieldType.Boolean, "BIT NULL")]
		[TestCase(FieldTypeHelper.FieldType.Code, "NVARCHAR(200) COLLATE Some_collation NULL")]
		[TestCase(FieldTypeHelper.FieldType.Currency, "DECIMAL(17,2) NULL")]
		[TestCase(FieldTypeHelper.FieldType.Date, "DATETIME NULL")]
		[TestCase(FieldTypeHelper.FieldType.Decimal, "DECIMAL(17,2) NULL")]
		[TestCase(FieldTypeHelper.FieldType.File, "")]
		[TestCase(FieldTypeHelper.FieldType.Text, "NVARCHAR(MAX) COLLATE Some_collation NULL")]
		[TestCase(FieldTypeHelper.FieldType.OffTableText, "NVARCHAR(MAX) COLLATE Some_collation NULL")]
		[TestCase(FieldTypeHelper.FieldType.User, "INT NULL")]
		[TestCase(FieldTypeHelper.FieldType.Varchar, "NVARCHAR(111) COLLATE Some_collation NULL")]
		[TestCase(FieldTypeHelper.FieldType.Objects, "NVARCHAR(MAX) NULL")]
		[TestCase(FieldTypeHelper.FieldType.MultiCode, "NVARCHAR(MAX) COLLATE Some_collation NULL")]
		public void ItShouldReturnCorrectColumnDescription(FieldTypeHelper.FieldType fieldType, string expectedDescription)
		{
			var fieldInfo = new FieldInfo();
			fieldInfo.Type = fieldType;

			Assert.That(_instanceUnderTest.GetColumnDescription(fieldInfo), Is.EqualTo(expectedDescription));
		}

		[Test]
		[TestCase(FieldTypeHelper.FieldType.LayoutText)]
		public void ItShouldThrowException(FieldTypeHelper.FieldType fieldType)
		{
			var fieldInfo = new FieldInfo();
			fieldInfo.Type = fieldType;

			Assert.Throws<ArgumentException>(() => _instanceUnderTest.GetColumnDescription(fieldInfo));
		}
	}
}