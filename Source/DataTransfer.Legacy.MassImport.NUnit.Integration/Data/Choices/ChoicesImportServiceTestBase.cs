using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.Data.MassImport;
using Relativity.Data.Toggles;
using Relativity.MassImport;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Choices;
using Relativity.Toggles;

namespace MassImport.NUnit.Integration.Data.Choices
{
	[TestFixture]
	public abstract class ChoicesImportServiceTestBase : EmptyWorkspaceTestBase
	{
		protected const int QueryTimeoutInSeconds = 5;

		protected Mock<IToggleProvider> ToggleProviderMock { get; private set; }
		protected TableNames TableNames { get; private set; }
		private protected ImportMeasurements ImportMeasurements { get; private set; }

		private List<string> _tablesToDeleteInTearDown;

		private protected abstract IChoicesImportService CreateSut(Relativity.MassImport.DTO.NativeLoadInfo settings);

		protected abstract Task SetOverlayBehaviorForFieldAsync(FieldInfo choiceField, OverlayBehavior overlayBehavior);

		protected readonly FieldInfo IdentifierField = new FieldInfo
		{
			ArtifactID = 1003667,
			Category = FieldCategory.Identifier,
			CodeTypeID = 0,
			DisplayName = WellKnownFields.ControlNumber,
			EnableDataGrid = false,
			FormatString = null,
			ImportBehavior = null,
			IsUnicodeEnabled = true,
			TextLength = 255,
			Type = FieldTypeHelper.FieldType.Varchar,
		};

		protected readonly FieldInfo SingleChoiceField = new FieldInfo
		{
			ArtifactID = 40_000,
			CodeTypeID = 100,
			DisplayName = "SingleChoice",
			Type = FieldTypeHelper.FieldType.Code,
		};

		protected readonly FieldInfo MultiChoiceField = new FieldInfo
		{
			ArtifactID = 40_001,
			CodeTypeID = 101,
			DisplayName = "MultiChoice",
			Type = FieldTypeHelper.FieldType.MultiCode,
		};

		[SetUp]
		public async Task SetUpAsync()
		{
			_tablesToDeleteInTearDown = new List<string>();
			SingleChoiceField.ImportBehavior = default; // this can be mutated in test
			MultiChoiceField.ImportBehavior = default; // this can be mutated in test

			ToggleProviderMock = new Mock<IToggleProvider>();
			TableNames = new TableNames();
			ImportMeasurements = new ImportMeasurements();

			await CreateMocksOfWorkspaceTablesAsync().ConfigureAwait(false);
			await CreateStagingTablesAsync(TableNames).ConfigureAwait(false);
		}

		[TearDown]
		public Task TearDownAsync()
		{
			var deleteStatements = _tablesToDeleteInTearDown.Select(tableName => $"DROP TABLE {tableName};");
			var dropWorkspaceTables = new QueryInformation
			{
				Statement = string.Join("\n", deleteStatements)
			};

			return EddsdboContext.ExecuteNonQueryAsync(dropWorkspaceTables);
		}

		[Test]
		public async Task ShouldDoNothingWhenNativeStagingTableIsEmptyAsync()
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(SingleChoiceField, MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Both,
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					IdentifierField,
					SingleChoiceField,
					MultiChoiceField,
				}
			};
			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, OverlayBehavior.ReplaceAll).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			await AssertZCodeTableIsValidAsync(SingleChoiceField, new (int, int)[] { }).ConfigureAwait(false);
			await AssertZCodeTableIsValidAsync(MultiChoiceField, new (int, int)[] { }).ConfigureAwait(false);
		}

		[TestCase(FieldTypeHelper.FieldType.Code)]
		[TestCase(FieldTypeHelper.FieldType.MultiCode)]
		public async Task ShouldThrowExceptionWhenChoicesAreMissingAsync(FieldTypeHelper.FieldType choiceFieldType)
		{
			// arrange
			FieldInfo choiceField;
			if (choiceFieldType == FieldTypeHelper.FieldType.Code)
			{
				choiceField = SingleChoiceField;
			}
			else
			{
				choiceField = MultiChoiceField;
				await SetOverlayBehaviorForFieldAsync(choiceField, OverlayBehavior.UseRelativityDefaults).ConfigureAwait(false);
			}

			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(choiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Both,
				OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
				MappedFields = new[]
				{
					IdentifierField,
					choiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = true},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>()
			{
				["A"] = new[] { 1001 },
			};

			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(choiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act & asserts
			var expectedExceptionConstraints = Throws.Exception
				.With.TypeOf<ExecuteSQLStatementFailedException>()
				.And.InnerException.Message.EqualTo("Some supplied choice ids are invalid");
			Assert.That(() => sut.PopulateCodeArtifactTable(), expectedExceptionConstraints);
		}

		[TestCase(true, OverwriteType.Append)]
		[TestCase(true, OverwriteType.Both)]
		[TestCase(false, OverwriteType.Overlay)]
		[TestCase(false, OverwriteType.Both)]
		public async Task ShouldLinkSingleChoiceForDocumentNotLinkedToAnyChoiceAsync(bool isNewDocument, OverwriteType overwriteType)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(SingleChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = overwriteType,
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					IdentifierField,
					SingleChoiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = isNewDocument},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>()
			{
				["A"] = new[] { 1001 },
			};

			await InsertChoicesToCodeTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1001, 1)
			};
			(int choiceId, int documentId)[] expectedCreatedMappings =
			{
				(1001, 1)
			};
			(int choiceId, int documentId)[] expectedDeletedMappings = { };
			await AssertZCodeTableIsValidAsync(SingleChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(SingleChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverwriteType.Overlay, OverlayBehavior.MergeAll)]
		[TestCase(OverwriteType.Both, OverlayBehavior.MergeAll)]
		[TestCase(OverwriteType.Overlay,  OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Both,  OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Overlay,  OverlayBehavior.UseRelativityDefaults)]
		[TestCase(OverwriteType.Both,  OverlayBehavior.UseRelativityDefaults)]
		public async Task ShouldNotChangeExistingLinksForSingleChoiceAsync(OverwriteType overwriteType, OverlayBehavior overlayBehavior)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(SingleChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = overwriteType,
				OverlayBehavior = overlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					SingleChoiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1001 },
			};

			var existingDocumentsToChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001 },
			};

			await InsertMappingsToZCodeTableAsync(SingleChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1001, 1)
			};
			(int choiceId, int documentId)[] expectedCreatedMappings = { (1001, 1) };
			(int choiceId, int documentId)[] expectedDeletedMappings = { (1001, 1) };

			await AssertZCodeTableIsValidAsync(SingleChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(SingleChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverwriteType.Overlay)]
		[TestCase(OverwriteType.Both)]
		public async Task ShouldReplaceSingleChoicesForValidDocuments(OverwriteType overwriteType)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(SingleChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = overwriteType,
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					IdentifierField,
					SingleChoiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
				new DocumentDto{ArtifactId = 2, Identifier = "B", IsNew = false, IsInvalid = true},
				new DocumentDto{ArtifactId = 3, Identifier = "C", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1002 },
				["B"] = new[] { 1002 },
				["C"] = new[] { 1002 },
			};

			var existingDocumentsToChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001 },
				[2] = new[] { 1001 },
				[3] = new[] { 1001 },
			};

			await InsertMappingsToZCodeTableAsync(SingleChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertChoicesToCodeTableAsync(SingleChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(SingleChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1002, 1),
				(1001, 2), // this document was invalid
				(1002, 3)
			};
			(int choiceId, int documentId)[] expectedCreatedMappings = { (1002, 1), (1002, 3) };
			(int choiceId, int documentId)[] expectedDeletedMappings = { (1001, 1), (1001, 3) };

			await AssertZCodeTableIsValidAsync(SingleChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(SingleChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverlayBehavior.ReplaceAll)]
		[TestCase(OverlayBehavior.MergeAll)]
		[TestCase(OverlayBehavior.UseRelativityDefaults, OverlayBehavior.ReplaceAll)]
		[TestCase(OverlayBehavior.UseRelativityDefaults, OverlayBehavior.MergeAll)]
		public async Task ShouldNotChangeExistingLinksForMultiChoiceAsync(OverlayBehavior importOverlayBehavior, OverlayBehavior fieldOverlayBehavior = OverlayBehavior.UseRelativityDefaults)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Overlay,
				OverlayBehavior = importOverlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					MultiChoiceField
				}
			};

			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, fieldOverlayBehavior).ConfigureAwait(false);

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1001, 1002 },
			};

			var existingDocumentsToChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001, 1002 },
			};

			await InsertMappingsToZCodeTableAsync(MultiChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1001, 1),
				(1002, 1),
			};

			bool isMerge = importOverlayBehavior == OverlayBehavior.MergeAll ||
						   (importOverlayBehavior == OverlayBehavior.UseRelativityDefaults &&
							fieldOverlayBehavior == OverlayBehavior.MergeAll);
			(int choiceId, int documentId)[] expectedCreatedMappings = isMerge
				? new (int, int)[] { }
				: new[] { (1001, 1), (1002, 1) };
			(int choiceId, int documentId)[] expectedDeletedMappings = isMerge
				? new (int, int)[] { }
				: new[] { (1001, 1), (1002, 1) };

			await AssertZCodeTableIsValidAsync(MultiChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(MultiChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverlayBehavior.MergeAll)]
		[TestCase(OverlayBehavior.UseRelativityDefaults, OverlayBehavior.MergeAll)]
		public async Task ShouldLinkMultiChoicesForValidNewDocuments(OverlayBehavior importOverlayBehavior, OverlayBehavior fieldOverlayBehavior = OverlayBehavior.UseRelativityDefaults)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Append,
				OverlayBehavior = importOverlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					MultiChoiceField,
				}
			};

			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, fieldOverlayBehavior).ConfigureAwait(false);

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = true},
				new DocumentDto{ArtifactId = 2, Identifier = "B", IsNew = true, IsInvalid = true},
				new DocumentDto{ArtifactId = 3, Identifier = "C", IsNew = true},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1001, 1002 },
				["B"] = new[] { 1001, },
				["C"] = new[] { 1002, 1003 },
			};
			
			await InsertChoicesToCodeTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1001, 1),
				(1002, 1),
				(1002, 3),
				(1003, 3),
			};
			(int choiceId, int documentId)[] expectedCreatedMappings =
			{
				(1001, 1),
				(1002, 1),
				(1002, 3),
				(1003, 3),
			};
			(int choiceId, int documentId)[] expectedDeletedMappings = { };

			await AssertZCodeTableIsValidAsync(MultiChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(MultiChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverlayBehavior.MergeAll)]
		[TestCase(OverlayBehavior.UseRelativityDefaults, OverlayBehavior.MergeAll)]
		public async Task ShouldMergeMultiChoicesForValidDocuments(OverlayBehavior importOverlayBehavior, OverlayBehavior fieldOverlayBehavior = OverlayBehavior.UseRelativityDefaults)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Overlay,
				OverlayBehavior = importOverlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					MultiChoiceField,
				}
			};

			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, fieldOverlayBehavior).ConfigureAwait(false);

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
				new DocumentDto{ArtifactId = 2, Identifier = "B", IsNew = false, IsInvalid = true},
				new DocumentDto{ArtifactId = 3, Identifier = "C", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1002, 1004 },
				["B"] = new[] { 1002, 1004 },
				["C"] = new[] { 1002, 1004 },
			};

			var existingDocumentsToChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001, 1003 },
				[2] = new[] { 1001, 1003 },
				[3] = new[] { 1001, 1003 },
			};

			await InsertMappingsToZCodeTableAsync(MultiChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertChoicesToCodeTableAsync(MultiChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1001, 1),
				(1002, 1),
				(1003, 1),
				(1004, 1),
				(1001, 2), // this document was invalid
				(1003, 2), // this document was invalid
				(1001, 3),
				(1002, 3),
				(1003, 3),
				(1004, 3),
			};
			(int choiceId, int documentId)[] expectedCreatedMappings =
			{
				(1002, 1),
				(1004, 1),
				(1002, 3),
				(1004, 3),
			};
			(int choiceId, int documentId)[] expectedDeletedMappings = { };

			await AssertZCodeTableIsValidAsync(MultiChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(MultiChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverwriteType.Overlay, OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Both, OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Overlay, OverlayBehavior.UseRelativityDefaults, OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Both, OverlayBehavior.UseRelativityDefaults, OverlayBehavior.ReplaceAll)]
		public async Task ShouldReplaceMultiChoicesForValidDocuments(
			OverwriteType overwriteType,
			OverlayBehavior importOverlayBehavior,
			OverlayBehavior fieldOverlayBehavior = OverlayBehavior.UseRelativityDefaults)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = overwriteType,
				OverlayBehavior = importOverlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					MultiChoiceField,
				}
			};

			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, fieldOverlayBehavior).ConfigureAwait(false);

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
				new DocumentDto{ArtifactId = 2, Identifier = "B", IsNew = false, IsInvalid = true},
				new DocumentDto{ArtifactId = 3, Identifier = "C", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1002, 1004 },
				["B"] = new[] { 1002, 1004 },
				["C"] = new[] { 1002, 1004 },
			};

			var existingDocumentsToChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001, 1003 },
				[2] = new[] { 1001, 1003 },
				[3] = new[] { 1001, 1003 },
			};

			await InsertMappingsToZCodeTableAsync(MultiChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertChoicesToCodeTableAsync(MultiChoiceField, existingDocumentsToChoiceMappings).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(MultiChoiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1002, 1),
				(1004, 1),
				(1001, 2), // this document was invalid
				(1003, 2), // this document was invalid
				(1002, 3),
				(1004, 3),
			};
			(int choiceId, int documentId)[] expectedCreatedMappings =
			{
				(1002, 1),
				(1004, 1),
				(1002, 3),
				(1004, 3),
			};
			(int choiceId, int documentId)[] expectedDeletedMappings =
			{
				(1001, 1),
				(1003, 1),
				(1001, 3),
				(1003, 3),
			};

			await AssertZCodeTableIsValidAsync(MultiChoiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(MultiChoiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(FieldTypeHelper.FieldType.Code, false, false)]
		[TestCase(FieldTypeHelper.FieldType.Code, false, true)]
		[TestCase(FieldTypeHelper.FieldType.Code, true, false)]
		[TestCase(FieldTypeHelper.FieldType.MultiCode, false, false)]
		[TestCase(FieldTypeHelper.FieldType.MultiCode, false, true)]
		[TestCase(FieldTypeHelper.FieldType.MultiCode, true, false)]
		public async Task ShouldThrowExceptionWhenMappingsAreDuplicatedAndIgnoreDuplicatesDisabled(
			FieldTypeHelper.FieldType choiceType,
			bool ignoredDuplicatesToggleEnabled,
			bool ignoredDuplicatesFieldEnabled)
		{
			// arrange
			ToggleProviderMock
				.Setup(x => x.IsEnabled<IgnoreDuplicateValuesForMassImportChoice>())
				.Returns(ignoredDuplicatesToggleEnabled);

			FieldInfo choiceField;
			if (choiceType == FieldTypeHelper.FieldType.Code)
			{
				choiceField = SingleChoiceField;
			}
			else
			{
				choiceField = MultiChoiceField;
				await SetOverlayBehaviorForFieldAsync(choiceField, OverlayBehavior.UseRelativityDefaults).ConfigureAwait(false);
			}

			FieldInfo.ImportBehaviorChoice importBehavior = ignoredDuplicatesFieldEnabled
				? FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates
				: default;
			choiceField.ImportBehavior = importBehavior;

			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(choiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Overlay,
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					IdentifierField,
					choiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1002, 1002 },
			};

			await InsertChoicesToCodeTableAsync(choiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(choiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act & assert
			Assert.Throws<ExecuteSQLStatementFailedException>(() => sut.PopulateCodeArtifactTable());
		}

		[TestCase(FieldTypeHelper.FieldType.Code)]
		[TestCase(FieldTypeHelper.FieldType.MultiCode)]
		public async Task ShouldNotThrowExceptionWhenMappingsAreDuplicatedAndIgnoreDuplicatesEnabled(FieldTypeHelper.FieldType choiceType)
		{
			// arrange
			ToggleProviderMock
				.Setup(x => x.IsEnabled<IgnoreDuplicateValuesForMassImportChoice>())
				.Returns(true);

			FieldInfo choiceField;
			if (choiceType == FieldTypeHelper.FieldType.Code)
			{
				choiceField = SingleChoiceField;
			}
			else
			{
				choiceField = MultiChoiceField;
				await SetOverlayBehaviorForFieldAsync(choiceField, OverlayBehavior.UseRelativityDefaults).ConfigureAwait(false);
			}
			choiceField.ImportBehavior = FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates;

			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(choiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = OverwriteType.Overlay,
				OverlayBehavior = OverlayBehavior.ReplaceAll,
				MappedFields = new[]
				{
					IdentifierField,
					choiceField,
				}
			};

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
			};

			var documentsToChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1002, 1002 },
			};

			await InsertChoicesToCodeTableAsync(choiceField, documentsToChoiceMapping).ConfigureAwait(false);
			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(choiceField, documentsToChoiceMapping).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			(int choiceId, int documentId)[] expectedMappingForChoice =
			{
				(1002, 1),
			};
			(int choiceId, int documentId)[] expectedCreatedMappings = { (1002, 1) };
			(int choiceId, int documentId)[] expectedDeletedMappings = { };

			await AssertZCodeTableIsValidAsync(choiceField, expectedMappingForChoice).ConfigureAwait(false);
			await AssertAuditInformationArePresentInMapTableAsync(choiceField, expectedCreatedMappings, expectedDeletedMappings).ConfigureAwait(false);
		}

		[TestCase(OverwriteType.Overlay, OverlayBehavior.MergeAll)]
		[TestCase(OverwriteType.Both, OverlayBehavior.MergeAll)]
		[TestCase(OverwriteType.Overlay, OverlayBehavior.ReplaceAll)]
		[TestCase(OverwriteType.Both, OverlayBehavior.ReplaceAll)]
		public async Task ShouldLinkSingleAndMultiChoices(OverwriteType overwriteType, OverlayBehavior importOverlayBehavior)
		{
			// arrange
			await CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(SingleChoiceField, MultiChoiceField).ConfigureAwait(false);

			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = TableNames.RunId,
				Overlay = overwriteType,
				OverlayBehavior = importOverlayBehavior,
				MappedFields = new[]
				{
					IdentifierField,
					MultiChoiceField,
					SingleChoiceField,
				}
			};

			await SetOverlayBehaviorForFieldAsync(MultiChoiceField, OverlayBehavior.UseRelativityDefaults).ConfigureAwait(false);

			var documents = new[]
			{
				new DocumentDto{ArtifactId = 1, Identifier = "A", IsNew = false},
				new DocumentDto{ArtifactId = 2, Identifier = "B", IsNew = false},
			};

			var documentToSingleChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 2002 },
			};

			var documentToMultiChoiceMapping = new Dictionary<string, int[]>
			{
				["A"] = new[] { 1001, 1002 },
				["B"] = new[] { 1001, 1003 },
			};

			var existingDocumentsToSingleChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 2001 },
				[2] = new[] { 2000 },
			};

			var existingDocumentsToMultiChoiceMappings = new Dictionary<int, int[]>
			{
				[1] = new[] { 1001, 1003 },
				[2] = new[] { 1001, 1003 },
			};

			await InsertChoicesToCodeTableAsync(SingleChoiceField, existingDocumentsToSingleChoiceMappings).ConfigureAwait(false);
			await InsertChoicesToCodeTableAsync(SingleChoiceField, documentToSingleChoiceMapping).ConfigureAwait(false);
			await InsertMappingsToZCodeTableAsync(SingleChoiceField, existingDocumentsToSingleChoiceMappings).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(SingleChoiceField, documentToSingleChoiceMapping).ConfigureAwait(false);

			await InsertChoicesToCodeTableAsync(MultiChoiceField, documentToMultiChoiceMapping).ConfigureAwait(false);
			await InsertChoicesToCodeTableAsync(MultiChoiceField, existingDocumentsToMultiChoiceMappings).ConfigureAwait(false);
			await InsertMappingsToZCodeTableAsync(MultiChoiceField, existingDocumentsToMultiChoiceMappings).ConfigureAwait(false);
			await InsertChoicesToStagingTableAsync(MultiChoiceField, documentToMultiChoiceMapping).ConfigureAwait(false);

			await InsertDocumentsToNativeStagingTableAsync(documents).ConfigureAwait(false);

			var sut = CreateSut(settings);

			// act
			sut.PopulateCodeArtifactTable();

			// assert
			// assert - single choice
			(int choiceId, int documentId)[] expectedMappingForSingleChoice =
			{
				(2002, 1),
			};
			await AssertZCodeTableIsValidAsync(SingleChoiceField, expectedMappingForSingleChoice).ConfigureAwait(false);

			// assert - multi choice
			(int choiceId, int documentId)[] expectedMappingForMultiChoice;
			if (importOverlayBehavior == OverlayBehavior.ReplaceAll)
			{
				expectedMappingForMultiChoice = new[]
				{
					(1001, 1),
					(1002, 1),
					(1001, 2),
					(1003, 2),
				};
			}
			else
			{
				expectedMappingForMultiChoice = new[]
				{
					(1001, 1),
					(1002, 1),
					(1003, 1),
					(1001, 2),
					(1003, 2),
				};
			}

			await AssertZCodeTableIsValidAsync(MultiChoiceField, expectedMappingForMultiChoice).ConfigureAwait(false);
		}

		protected async Task CreateZCodeTablesAndSetDefaultOverlayBehaviorForFieldsAsync(params FieldInfo[] choices)
		{
			var sb = new StringBuilder();
			foreach (var choice in choices)
			{
				string tableName = $"[EDDSDBO].[ZCodeArtifact_{choice.CodeTypeID}]";
				string createZCodeTable = $@"
				CREATE TABLE {tableName}(
					[CodeArtifactID] [int] NOT NULL,
					[AssociatedArtifactID] [int] NOT NULL
				);";
				sb.Append(createZCodeTable);
				_tablesToDeleteInTearDown.Add(tableName);

				await SetOverlayBehaviorForFieldAsync(choice, OverlayBehavior.UseRelativityDefaults).ConfigureAwait(false);
			}

			if (sb.Length == 0)
			{
				return;
			}

			var query = new QueryInformation
			{
				Statement = sb.ToString()
			};
			await EddsdboContext.ExecuteNonQueryAsync(query).ConfigureAwait(false);
		}

		protected Task InsertDocumentsToNativeStagingTableAsync(IEnumerable<DocumentDto> documents)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("kCura_Import_Status", typeof(long)));
			dataTable.Columns.Add(new DataColumn("kCura_Import_IsNew", typeof(bool)));
			dataTable.Columns.Add(new DataColumn("ArtifactID", typeof(int)));
			dataTable.Columns.Add(new DataColumn(WellKnownFields.ControlNumber, typeof(string)));
			foreach (DocumentDto document in documents)
			{
				int status = document.IsInvalid ? 1 : 0;
				dataTable.Rows.Add(status, document.IsNew, document.ArtifactId, document.Identifier);
			}
			var parameters = new SqlBulkCopyParameters { DestinationTableName = $"[Resource].[{TableNames.Native}]" };
			parameters.ColumnMappings.AddRange(new[]
			{
				new SqlBulkCopyColumnMapping("kCura_Import_Status", "kCura_Import_Status"),
				new SqlBulkCopyColumnMapping("kCura_Import_IsNew", "kCura_Import_IsNew"),
				new SqlBulkCopyColumnMapping("ArtifactID", "ArtifactID"),
				new SqlBulkCopyColumnMapping(WellKnownFields.ControlNumber, WellKnownFields.ControlNumber)
			});
			return EddsdboContext.ExecuteBulkCopyAsync(dataTable, parameters, CancellationToken.None);
		}

		protected Task InsertChoicesToStagingTableAsync(FieldInfo choiceField, Dictionary<string, int[]> documentsToChoicesMapping)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("DocumentIdentifier", typeof(string)));
			dataTable.Columns.Add(new DataColumn("CodeArtifactID", typeof(int)));
			dataTable.Columns.Add(new DataColumn("CodeTypeID", typeof(int)));

			var rows = documentsToChoicesMapping
				.Select(x =>
					x.Value.Select(codeArtifactId => new { DocumentId = x.Key, CodeArtifactId = codeArtifactId }))
				.SelectMany(x => x);
			foreach (var row in rows)
			{
				dataTable.Rows.Add(row.DocumentId, row.CodeArtifactId, choiceField.CodeTypeID);
			}

			var parameters = new SqlBulkCopyParameters { DestinationTableName = $"[Resource].[{TableNames.Code}]" };
			return EddsdboContext.ExecuteBulkCopyAsync(dataTable, parameters, CancellationToken.None);
		}

		protected Task InsertChoicesToCodeTableAsync<T>(FieldInfo choiceField, Dictionary<T, int[]> documentsToChoicesMapping)
		{
			var uniqueChoices = documentsToChoicesMapping
				.Select(x => x.Value)
				.SelectMany(x => x)
				.Distinct();

			return InsertChoicesToCodeTableAsync(choiceField, uniqueChoices);
		}

		protected Task InsertChoicesToCodeTableAsync(FieldInfo choiceField, IEnumerable<int> uniqueChoices)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("ArtifactID", typeof(int)));
			dataTable.Columns.Add(new DataColumn("CodeTypeID", typeof(int)));

			foreach (var choiceArtifactId in uniqueChoices)
			{
				dataTable.Rows.Add(choiceArtifactId, choiceField.CodeTypeID);
			}
			var parameters = new SqlBulkCopyParameters { DestinationTableName = "[EDDSDBO].[Code]" };
			return EddsdboContext.ExecuteBulkCopyAsync(dataTable, parameters, CancellationToken.None);
		}

		protected Task InsertMappingsToZCodeTableAsync(FieldInfo choiceField, Dictionary<int, int[]> documentsToChoicesMapping)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add(new DataColumn("CodeArtifactID", typeof(int)));
			dataTable.Columns.Add(new DataColumn("AssociatedArtifactID", typeof(int)));

			var rows = documentsToChoicesMapping
				.Select(x =>
					x.Value.Select(codeArtifactId => new { DocumentId = x.Key, CodeArtifactId = codeArtifactId }))
				.SelectMany(x => x);

			foreach (var row in rows)
			{
				dataTable.Rows.Add(row.CodeArtifactId, row.DocumentId);
			}
			var parameters = new SqlBulkCopyParameters { DestinationTableName = $"[EDDSDBO].[ZCodeArtifact_{choiceField.CodeTypeID}]" };
			return EddsdboContext.ExecuteBulkCopyAsync(dataTable, parameters, CancellationToken.None);
		}

		protected async Task AssertZCodeTableIsValidAsync(FieldInfo choiceField, (int choiceId, int documentId)[] expectedMapping)
		{
			var query = new QueryInformation
			{
				Statement = $"SELECT CodeArtifactID, AssociatedArtifactID FROM [EDDSDBO].[ZCodeArtifact_{choiceField.CodeTypeID}]"
			};

			var actualMapping = new List<(int choiceId, int documentId)>();
			using (var reader = await EddsdboContext.ExecuteQueryAsReaderAsync(query).ConfigureAwait(false))
			{
				while (await reader.ReadAsync().ConfigureAwait(false))
				{
					int choiceId = (int)reader["CodeArtifactID"];
					int documentId = (int)reader["AssociatedArtifactID"];
					actualMapping.Add((choiceId, documentId));
				}
			}

			const string failureMessage = "All expected document to choice mapping should be present.";
			CollectionAssert.AreEquivalent(expectedMapping, actualMapping, failureMessage);
		}

		protected async Task AssertAuditInformationArePresentInMapTableAsync(
			FieldInfo choiceField,
			(int choiceId, int documentId)[] expectedCreatedMappings,
			(int choiceId, int documentId)[] expectedDeletedMappings)
		{
			var query = new QueryInformation
			{
				Statement = $"SELECT ArtifactID, MappedArtifactID, IsNew FROM [Resource].[{TableNames.Map}] WHERE [FieldArtifactID] = {choiceField.ArtifactID}"
			};

			var actualCreatedMappings = new List<(int choiceId, int documentId)>();
			var actualDeletedMappings = new List<(int choiceId, int documentId)>();
			using (var reader = await EddsdboContext.ExecuteQueryAsReaderAsync(query).ConfigureAwait(false))
			{
				while (await reader.ReadAsync().ConfigureAwait(false))
				{
					int documentId = (int)reader["ArtifactID"];
					int choiceId = (int)reader["MappedArtifactID"];
					bool wasCreated = (bool)reader["IsNew"];
					if (wasCreated)
					{
						actualCreatedMappings.Add((choiceId, documentId));
					}
					else
					{
						actualDeletedMappings.Add((choiceId, documentId));
					}
				}
			}

			CollectionAssert.AreEquivalent(expectedCreatedMappings, actualCreatedMappings, "Incorrect Audit information for created mappings");
			CollectionAssert.AreEquivalent(expectedDeletedMappings, actualDeletedMappings, "Incorrect Audit information for deleted mappings");
		}

		private Task CreateMocksOfWorkspaceTablesAsync()
		{
			string createMockOfCodeTable = @"
			CREATE TABLE [EDDSDBO].[Code](
				[ArtifactID] [int] NOT NULL,
				[CodeTypeID] [int] NOT NULL
			);";

			string createMockOfFieldTable = @"
			CREATE TABLE [EDDSDBO].[Field](
				[ArtifactID] [int] NOT NULL,
				[OverlayMergeValues] [bit]
			);";

			_tablesToDeleteInTearDown.Add("[EDDSDBO].[Code]");
			_tablesToDeleteInTearDown.Add("[EDDSDBO].[Field]");

			var query = new QueryInformation
			{
				Statement = createMockOfCodeTable + createMockOfFieldTable
			};
			return EddsdboContext.ExecuteNonQueryAsync(query);
		}

		private Task CreateStagingTablesAsync(TableNames tableNames)
		{
			string createCodeTable = $@"
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

			string createSubsetOfNativeTable = $@"
			CREATE TABLE [Resource].[{tableNames.Native}] (
				[kCura_Import_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
				[kCura_Import_Status] BIGINT NOT NULL,
				[kCura_Import_IsNew] BIT NOT NULL,
				[ArtifactID] INT NOT NULL,
				[{WellKnownFields.ControlNumber}] NVARCHAR(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
			);";

			string createMapTable = $@"
			CREATE TABLE [Resource].[{tableNames.Map}] (
				[ArtifactID] INT NOT NULL,
				[MappedArtifactID] INT NOT NULL,
				[FieldArtifactID] INT NOT NULL,
				[IsNew] BIT NOT NULL,
				CONSTRAINT PK_{tableNames.Map} PRIMARY KEY (ArtifactID, FieldArtifactID, IsNew, MappedArtifactID)
			);";

			var query = new QueryInformation
			{
				Statement = createCodeTable + createSubsetOfNativeTable + createMapTable
			};
			return EddsdboContext.ExecuteNonQueryAsync(query);
		}

		protected class DocumentDto
		{
			public int ArtifactId { get; set; }
			public string Identifier { get; set; }
			public bool IsNew { get; set; }
			public bool IsInvalid { get; set; }
		}
	}
}
