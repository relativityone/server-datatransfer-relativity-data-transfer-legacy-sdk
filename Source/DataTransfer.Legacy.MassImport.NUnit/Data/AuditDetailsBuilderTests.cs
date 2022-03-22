using DataTransfer.Legacy.MassImport.NUnit.Properties;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.NUnit.TestHelpers;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	internal class AuditDetailsBuilderTests
	{
		private const string RUN_ID = "AAD09DF6-A4C7-4DF0-B963-0050C7809000";
		private const int ARTIFACT_TYPE_ID = 1000050;
		private const int MAPPED_FIELDS_ARTIFACT_ID = 100123;
		private const int IDENTIFIER_ARTIFACT_ID = 100000;

		private AuditDetailsBuilder _builder;
		private Mock<IColumnDefinitionCache> _columnDefinitionCacheMock;
		private Mock<BaseContext> ContextMock { get; set; }

		[SetUp]
		public void SetUp()
		{
			ContextMock = new Mock<BaseContext>();

			ContextMock.Setup(ct => ct.ExecuteSqlStatementAsScalar<string>(It.IsAny<string>(), null, It.IsAny<int>()))
				.Returns("Test_Collation");

			_columnDefinitionCacheMock = new Mock<IColumnDefinitionCache>();
		}

		[Test]
		public void ShouldGenerateCorrectAuditDetails(
			[Values(true, false)] bool performAudit,
			[Values(ImportAuditLevel.FullAudit, ImportAuditLevel.NoAudit, ImportAuditLevel.NoSnapshot)] ImportAuditLevel auditLevel,
			[Values(OverlayBehavior.MergeAll, OverlayBehavior.ReplaceAll, OverlayBehavior.UseRelativityDefaults)] OverlayBehavior overlayBehavior)
		{
			// ARRANGE
			string expectedDetailsClause, expectedMapClause;

			if (performAudit && auditLevel == ImportAuditLevel.FullAudit)
			{
				expectedDetailsClause = Resources.AuditDetailsBuilderTests_detailsClause_Audit;
				expectedMapClause = Resources.AuditDetailsBuilderTests_mapClause_Audit;
			}
			else
			{
				expectedDetailsClause = Resources.AuditDetailsBuilderTests_detailsClause_NoAudit;
				expectedMapClause = string.Empty;
			}

			var settings = InitializeSettings(
				RUN_ID,
				ARTIFACT_TYPE_ID,
				auditLevel,
				overlayBehavior,
				OverwriteType.Append,
				new FieldInfo[]
				{
					new FieldInfo {DisplayName = "Boolean Field", Type = FieldTypeHelper.FieldType.Boolean, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID},
					new FieldInfo {DisplayName = "Code Field", Type = FieldTypeHelper.FieldType.Code, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Date Field", Type = FieldTypeHelper.FieldType.Date, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "File Field", Type = FieldTypeHelper.FieldType.File, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Object Field", Type = FieldTypeHelper.FieldType.Object, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Text Field", Type = FieldTypeHelper.FieldType.Text, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Varchar Field", Type = FieldTypeHelper.FieldType.Varchar, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Empty Field", Type = FieldTypeHelper.FieldType.Empty, ArtifactID = IDENTIFIER_ARTIFACT_ID, Category = FieldCategory.Identifier},
				});

			_builder = new AuditDetailsBuilder(
				ContextMock.Object,
				settings,
				_columnDefinitionCacheMock.Object,
				new TableNames(RUN_ID),
				(int)Relativity.ArtifactType.Document);

			// ACT
			var results = _builder.GenerateAuditDetails(performAudit, false);

			// ASSERT
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item1, expectedDetailsClause);
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item2, expectedMapClause);
		}

		[Test]
		public void ShouldGenerateAuditDetailsWithEncoding(
			[Values(true, false)] bool includeExtractedTextEncoding)
		{
			// ARRANGE
			var settings = InitializeSettings(
				RUN_ID,
				ARTIFACT_TYPE_ID,
				ImportAuditLevel.NoAudit,
				OverlayBehavior.MergeAll,
				OverwriteType.Append,
				null);

			_builder = new AuditDetailsBuilder(
				ContextMock.Object,
				settings,
				_columnDefinitionCacheMock.Object,
				new TableNames(RUN_ID),
				(int)Relativity.ArtifactType.Document);

			// ACT
			var results = _builder.GenerateAuditDetails(false, includeExtractedTextEncoding);

			// ASSERT
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item1, includeExtractedTextEncoding
																				? Resources.AuditDetailsBuilderTests_detailsClause_NoAudit_Encoding
																				: Resources.AuditDetailsBuilderTests_detailsClause_NoAudit);
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item2, string.Empty);
		}

		[Test]
		public void ShouldGenerateAuditDetailsForMultiCodeField(
			[Values(OverlayBehavior.MergeAll, OverlayBehavior.ReplaceAll)] OverlayBehavior overlayBehavior)
		{
			// ARRANGE
			_columnDefinitionCacheMock.Setup(x => x[It.IsAny<int>()]).Returns(new ColumnDefinitionInfo { OverlayMergeValues = false });

			var settings = InitializeSettings(
				RUN_ID,
				ARTIFACT_TYPE_ID,
				ImportAuditLevel.FullAudit,
				overlayBehavior,
				OverwriteType.Append,
				new FieldInfo[]
				{
					new FieldInfo {DisplayName = "MultiCode Field", Type = FieldTypeHelper.FieldType.MultiCode, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID },
					new FieldInfo {DisplayName = "Empty Field", Type = FieldTypeHelper.FieldType.Empty, ArtifactID = IDENTIFIER_ARTIFACT_ID, Category = FieldCategory.Identifier},
				});

			_builder = new AuditDetailsBuilder(
				ContextMock.Object,
				settings,
				_columnDefinitionCacheMock.Object,
				new TableNames(RUN_ID),
				(int)Relativity.ArtifactType.Document);

			// ACT
			var results = _builder.GenerateAuditDetails(true, false);

			// ASSERT
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item1, overlayBehavior == OverlayBehavior.MergeAll
				? Resources.AuditDetailsBuilderTests_MultiCode_detailsClause_MergeAll
				: Resources.AuditDetailsBuilderTests_MultiCode_detailsClause_ReplaceAll);
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item2, overlayBehavior == OverlayBehavior.MergeAll
				? Resources.AuditDetailsBuilderTests_MultiCode_mapClause_MergeAll
				: Resources.AuditDetailsBuilderTests_MultiCode_mapClause_ReplaceAll);
		}

		[Test]
		public void ShouldNotGenerateAuditDetailsWhenNonAuditableFieldsAreMapped()
		{
			// ARRANGE
			var settings = InitializeSettings(
				RUN_ID,
				ARTIFACT_TYPE_ID,
				ImportAuditLevel.FullAudit,
				OverlayBehavior.MergeAll,
				OverwriteType.Overlay,
				new FieldInfo[]
				{
					new FieldInfo {DisplayName = "Auto Create Field", Type = FieldTypeHelper.FieldType.Empty, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID, Category = FieldCategory.AutoCreate},
					new FieldInfo {DisplayName = "Empty Field", Type = FieldTypeHelper.FieldType.Empty, ArtifactID = IDENTIFIER_ARTIFACT_ID, Category = FieldCategory.Identifier},
					new FieldInfo {DisplayName = "Empty Field", Type = FieldTypeHelper.FieldType.Empty, ArtifactID = MAPPED_FIELDS_ARTIFACT_ID, Category = FieldCategory.Identifier},
				});

			_builder = new AuditDetailsBuilder(
				ContextMock.Object,
				settings,
				_columnDefinitionCacheMock.Object,
				new TableNames(RUN_ID),
				(int)Relativity.ArtifactType.Document);

			// ACT
			var results = _builder.GenerateAuditDetails(true, false);

			// ASSERT
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item1, Resources.AuditDetailsBuilderTests_detailsClause_NoAudit);
			ThenTheStringsAreEqualIgnoringWhiteSpaces(results.Item2, string.Empty);
		}

		private void ThenTheStringsAreEqualIgnoringWhiteSpaces(string result, string expectedResult)
		{
			var normalizedResult = result.RemoveWhitespaces();
			var normalizedExpectedResult = expectedResult.RemoveWhitespaces();

			Assert.AreEqual(normalizedExpectedResult, normalizedResult, "Generated audit details differ from the expected values.");
		}

		private static Relativity.MassImport.DTO.ObjectLoadInfo InitializeSettings(string runId, int artifactTypeId, ImportAuditLevel auditLevel, OverlayBehavior overlayBehavior, OverwriteType overwriteType, FieldInfo[] mappedFields)
		{
			var objectLoadInfo = new Relativity.MassImport.DTO.ObjectLoadInfo
			{
				RunID = runId,
				ArtifactTypeID = artifactTypeId,
				AuditLevel = auditLevel,
				OverlayBehavior = overlayBehavior,
				MappedFields = mappedFields,
				Overlay = overwriteType,
			};

			return objectLoadInfo;
		}
	}
}