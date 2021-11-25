using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Data.MassImport;
using Relativity.Data.Toggles;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.NUnit.Properties;
using Relativity.MassImport.NUnit.TestHelpers;
using Relativity.Toggles;

namespace Relativity.MassImport.NUnit.Data.Choices
{
	[TestFixture]
	public class OldChoicesImportServiceTests
	{
		private const string RunId = "974a2b26_d665_4f42_8b3b_31949b335a01";
		private const int QueryTimeoutInSeconds = 10;

		private Mock<BaseContext> _baseContextMock;
		private Mock<IToggleProvider> _toggleProviderMock;

		private readonly FieldInfo _identifierField = new FieldInfo
		{
			ArtifactID = 1,
			DisplayName = "Id",
			Category = FieldCategory.Identifier
		};

		private readonly FieldInfo _singleChoiceField = new FieldInfo
		{
			ArtifactID = 2,
			DisplayName = "SingleChoice1",
			Type = FieldTypeHelper.FieldType.Code,
			CodeTypeID = 101,
			ImportBehavior = FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates
		};

		private readonly FieldInfo _multiChoiceField = new FieldInfo
		{
			ArtifactID = 2,
			DisplayName = "MultiChoice1",
			Type = FieldTypeHelper.FieldType.MultiCode,
			CodeTypeID = 102,
		};

		[SetUp]
		public void SetUp()
		{
			// MockBehavior has to be strict to make sure that only set up methods were called
			_baseContextMock = new Mock<BaseContext>(MockBehavior.Strict);
			_toggleProviderMock = new Mock<IToggleProvider>();
		}

		[Test]
		public void ShouldNotExecuteQueryWhenChoiceFieldsNotMapped()
		{
			// arrange
			var setting = new NativeLoadInfo
			{
				MappedFields = new[]
				{
					_identifierField
				},
				KeyFieldArtifactID = 1
			};
			IChoicesImportService sut = CreateSut(setting);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			// mock behavior is strict, so it would have thrown an exception if we had called any method
		}

		[Test]
		public void ShouldExecuteQueryForSingleChoice()
		{
			// arrange
			string expectedQuery = Resources.OldChoicesImportServiceTests_ShouldExecuteQueryForSingleChoice;
			_baseContextMock
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>(), QueryTimeoutInSeconds))
				.Returns(0); // we don't use return value

			var setting = new NativeLoadInfo
			{
				OverlayBehavior = OverlayBehavior.MergeAll,
				MappedFields = new[]
				{
					_identifierField,
					_singleChoiceField
				},
				KeyFieldArtifactID = 1
			};
			IChoicesImportService sut = CreateSut(setting);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			_baseContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(It.Is<string>(actual => actual.IsEqualIgnoringWhitespaces(expectedQuery)), QueryTimeoutInSeconds));
		}

		[Test]
		public void ShouldExecuteQueryForMultiChoice()
		{
			// arrange
			string expectedQuery = Resources.OldChoicesImportServiceTests_ShouldExecuteQueryForMultiChoice;
			_baseContextMock
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>(), QueryTimeoutInSeconds))
				.Returns(0); // we don't use return value

			var setting = new NativeLoadInfo
			{
				OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
				MappedFields = new[]
				{
					_identifierField,
					_multiChoiceField
				},
				KeyFieldArtifactID = 1
			};
			IChoicesImportService sut = CreateSut(setting);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			_baseContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(It.Is<string>(actual => actual.IsEqualIgnoringWhitespaces(expectedQuery)), QueryTimeoutInSeconds));
		}

		[Test]
		public void ShouldExecuteQueryForTwoChoices()
		{
			// arrange
			string expectedQuery = Resources.OldChoicesImportServiceTests_ShouldExecuteQueryForTwoChoices;
			_baseContextMock
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>(), QueryTimeoutInSeconds))
				.Returns(0); // we don't use return value

			var setting = new NativeLoadInfo
			{
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					_identifierField,
					_singleChoiceField,
					_multiChoiceField
				},
				KeyFieldArtifactID = 1
			};
			IChoicesImportService sut = CreateSut(setting);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			_baseContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(It.Is<string>(actual => actual.IsEqualIgnoringWhitespaces(expectedQuery)), QueryTimeoutInSeconds));
		}

		[Test]
		public void ShouldIncludeDistinctClause()
		{
			// arrange
			string expectedQuery = Resources.OldChoicesImportServiceTests_ShouldIncludeDistinctClause;
			_baseContextMock
				.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>(), QueryTimeoutInSeconds))
				.Returns(0); // we don't use return value

			_toggleProviderMock
				.Setup(x => x.IsEnabled<IgnoreDuplicateValuesForMassImportChoice>())
				.Returns(true);

			var setting = new NativeLoadInfo
			{
				OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
				MappedFields = new[]
				{
					_identifierField,
					_singleChoiceField
				},
				KeyFieldArtifactID = 1
			};
			IChoicesImportService sut = CreateSut(setting);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			_baseContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(It.Is<string>(actual => actual.IsEqualIgnoringWhitespaces(expectedQuery)), QueryTimeoutInSeconds));
		}

		private OldChoicesImportService CreateSut(NativeLoadInfo settings)
		{
			var tableNames = new TableNames(RunId);
			var importMeasurements = new ImportMeasurements();

			return new OldChoicesImportService(
				_baseContextMock.Object,
				_toggleProviderMock.Object,
				tableNames,
				importMeasurements,
				settings,
				QueryTimeoutInSeconds);
		}
	}
}
