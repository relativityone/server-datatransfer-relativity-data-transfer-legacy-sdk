using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.Core.Service.MassImport;
using Relativity.Data.MassImport;
using Relativity.MassImport.DTO;
using Relativity.MassImport.Api;
using Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables;
using BaseContext = Relativity.Core.BaseContext;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class OverlayIdentifierTests : MassImportTestBase
	{
		private readonly bool _isDocumentImport;

		private int _objectTypeId;
		private int _parentId;
		private MassImportField _identifierField;
		private MassImportField _uniqueIdField;

		/// <summary>
		/// Creates instance of OverlayIdentifierTests test fixture.
		/// </summary>
		/// <param name="isDocumentImport">If true tests import of documents, otherwise tests import of RDOs.</param>
		public OverlayIdentifierTests(bool isDocumentImport)
		{
			_isDocumentImport = isDocumentImport;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			if (_isDocumentImport)
			{
				_objectTypeId = (int) Relativity.ArtifactType.Document;
				_parentId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
			}
			else
			{
				_objectTypeId = await RdoHelper
					.CreateObjectTypeAsync(TestParameters, TestWorkspace, nameof(OverlayIdentifierTests))
					.ConfigureAwait(false);
				_parentId = Constants.WorkspaceInternalId;
			}

			_uniqueIdField = await FieldHelper
				.CreateFixedLengthTextField(TestParameters, TestWorkspace, $"{nameof(OverlayIdentifierTests)}UniqueID", _objectTypeId)
				.ConfigureAwait(false);
			_identifierField = await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace, _objectTypeId)
				.ConfigureAwait(false);
		}

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper
				.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, _objectTypeId)
				.ConfigureAwait(false);
		}

		[Test]
		public async Task Import_Succeeds_WhenIdentifierHasValueAsync()
		{
			// arrange
			var settings = GetMassImportSettingsForOverlayIdentifier(OverwriteType.Both);

			MassImportArtifact[] recordsToImport =
			{
				CreateMassImportArtifact(identifier: "A", uniqueId: "001"),
				CreateMassImportArtifact(identifier: "B", uniqueId: "002"),
			};

			// act
			var result = await this.RunObjectsImportAsync(recordsToImport, settings).ConfigureAwait(false);

			// assert
			Assert.That(result.ExceptionDetail, Is.Null, "Unexpected job level error");
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(2), "Number of processed artifacts is invalid");
			Assert.That(result.ArtifactsCreated, Is.EqualTo(2), "Number of created artifacts is invalid");
		}

		[Test]
		public async Task Import_ReturnsItemError_WhenIdentifierIsNullAsync()
		{
			// arrange
			var settings = new MassImportSettings
			{
				ArtifactTypeID = _objectTypeId,
				Overlay = OverwriteType.Append,
				MappedFields = new Relativity.FieldInfo[]
				{
					_identifierField,
				}
			};

			MassImportArtifact[] recordsToImport =
			{
				CreateMassImportArtifact(identifier: "A"),
				CreateMassImportArtifact(identifier: ""),
				CreateMassImportArtifact(identifier: null),
			};

			// act
			var result = await this.RunObjectsImportAsync(recordsToImport, settings).ConfigureAwait(false);

			// assert
			Assert.That(result.ExceptionDetail, Is.Null, "Unexpected job level error");
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(3), "Number of processed artifacts is invalid");
			Assert.That(result.ArtifactsCreated, Is.EqualTo(2), "Number of created artifacts is invalid");
			Assert.That(result.ItemErrors, Has.Count.EqualTo(1), "Number of item level errors is invalid");
			Assert.That(result.ItemErrors.Single(), Is.EqualTo($"{(int)ImportStatus.EmptyIdentifier}"));
		}

		[TestCase(OverwriteType.Append)]
		[TestCase(OverwriteType.Both)]
		public async Task Import_ReturnsItemError_ForNewRecordsWhenIdentifierIsNullAsync(OverwriteType overlayMode)
		{
			// arrange
			var settings = GetMassImportSettingsForOverlayIdentifier(overlayMode);

			MassImportArtifact[] recordsToImport =
			{
				CreateMassImportArtifact(identifier: "A", uniqueId: "001"),
				CreateMassImportArtifact(identifier: "", uniqueId: "002"),
				CreateMassImportArtifact(identifier: null, uniqueId: "003"), // expected item level error
			};

			// act
			var result = await RunObjectsImportAsync(recordsToImport, settings).ConfigureAwait(false);

			// assert
			Assert.That(result.ExceptionDetail, Is.Null, "Unexpected job level error");
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(3), "Number of processed artifacts is invalid");
			Assert.That(result.ArtifactsCreated, Is.EqualTo(2), "Number of created artifacts is invalid");
			Assert.That(result.ItemErrors, Has.Count.EqualTo(1), "Number of item level errors is invalid");
			Assert.That(result.ItemErrors.Single(), Is.EqualTo($"{(int)ImportStatus.EmptyIdentifier}"));
		}

		[TestCase(OverwriteType.Overlay)]
		[TestCase(OverwriteType.Both)]
		public async Task Import_ReturnsItemError_ForExistingRecordsWhenIdentifierIsNullAsync(OverwriteType overlayMode)
		{
			// arrange
			var appendSettings = new MassImportSettings
			{
				ArtifactTypeID = _objectTypeId,
				Overlay = OverwriteType.Append,
				MappedFields = new Relativity.FieldInfo[]
				{
					_identifierField,
					_uniqueIdField,
				}
			};

			MassImportArtifact[] recordsToAppend =
			{
				CreateMassImportArtifact(identifier: "A", uniqueId: "001"),
				CreateMassImportArtifact(identifier: "B", uniqueId: "002"),
				CreateMassImportArtifact(identifier: "C", uniqueId: "003"),
			};

			var appendResult = await RunObjectsImportAsync(recordsToAppend, appendSettings).ConfigureAwait(false);
			Assert.That(appendResult.ArtifactsCreated, Is.EqualTo(3), "Arrange failed - not all initial records were imported");

			var overlaySettings = GetMassImportSettingsForOverlayIdentifier(overlayMode);

			MassImportArtifact[] recordsToOverlay =
			{
				CreateMassImportArtifact(identifier: "A", uniqueId: "001"),
				CreateMassImportArtifact(identifier: "", uniqueId: "002"),
				CreateMassImportArtifact(identifier: null, uniqueId: "003"), // expected item level error
			};

			// act
			var result = await RunObjectsImportAsync(recordsToOverlay, overlaySettings).ConfigureAwait(false);

			// assert
			Assert.That(result.ExceptionDetail, Is.Null, "Unexpected job level error");
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(3), "Number of processed artifacts is invalid");
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(2), "Number of created artifacts is invalid");
			Assert.That(result.ItemErrors, Has.Count.EqualTo(1), "Number of item level errors is invalid");
			Assert.That(result.ItemErrors.Single(), Is.EqualTo($"{(int)ImportStatus.EmptyIdentifier}"));
		}

		[Test]
		public async Task Import_ReturnsItemError_WhenOverlayIdentifierIsNull()
		{
			// arrange
			var settings = new MassImportSettings
			{
				ArtifactTypeID = _objectTypeId,
				Overlay = OverwriteType.Append,
				MappedFields = new Relativity.FieldInfo[]
				{
					_identifierField,
					_uniqueIdField
				},
				KeyFieldArtifactID = _uniqueIdField.ArtifactID
			};

			MassImportArtifact[] recordsToImport =
			{
				CreateMassImportArtifact(identifier: "A", uniqueId: "001"),
				CreateMassImportArtifact(identifier: "B", uniqueId: "002"),
				CreateMassImportArtifact(identifier: "C", uniqueId: null), // expected item level error
			};

			// act
			var result = await RunObjectsImportAsync(recordsToImport, settings).ConfigureAwait(false);

			// assert
			Assert.That(result.ExceptionDetail, Is.Null, "Unexpected job level error");
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(3), "Number of processed artifacts is invalid");
			Assert.That(result.ArtifactsCreated, Is.EqualTo(2), "Number of created artifacts is invalid");
			Assert.That(result.ItemErrors, Has.Count.EqualTo(1), "Number of item level errors is invalid");
			Assert.That(result.ItemErrors.Single(), Is.EqualTo($"{(int)ImportStatus.EmptyOverlayIdentifier}"));
		}

		private MassImportArtifact CreateMassImportArtifact(string identifier, string uniqueId) =>
			new MassImportArtifact(
				new List<object> { identifier, uniqueId },
				parentFolderId: _parentId);

		private MassImportArtifact CreateMassImportArtifact(string identifier) =>
			new MassImportArtifact(
				new List<object> { identifier },
				parentFolderId: _parentId);

		private MassImportSettings GetMassImportSettingsForOverlayIdentifier(OverwriteType overlayMode)
		{
			return new MassImportSettings
			{
				ArtifactTypeID = _objectTypeId,
				Overlay = overlayMode,
				MappedFields = new Relativity.FieldInfo[]
				{
					_identifierField,
					_uniqueIdField
				},
				KeyFieldArtifactID = _uniqueIdField.ArtifactID
			};
		}

		/// <summary>
		/// This method is used to run import and return item level errors.
		/// Current implementation of <see cref="MassImportManager"/>does not return item level error.
		/// </summary>
		/// <param name="settings"></param>
		private async Task<MassImportResults> RunObjectsImportAsync(MassImportArtifact[] artifacts, MassImportSettings settings)
		{
			BaseContext context = CoreContext.ChicagoContext;
			var artifactManager = new ArtifactManager();

			var populateStagingTablesStage = new PopulateStagingTablesStage<TableNames>(context, artifacts, settings, artifactManager);
			void LoadStagingTablesAction(TableNames tableNames) => populateStagingTablesStage.Execute(tableNames.Native, tableNames.Code, tableNames.Objects);

			MassImportManagerBase.MassImportResults internalResult;
			if (settings.ArtifactTypeID == (int)Relativity.ArtifactType.Document)
			{
				internalResult = MassImporter.ImportNativesForObjectManager(
					context,
					settings,
					LoadStagingTablesAction,
					dataGridReader: null);
			}
			else
			{
				internalResult = MassImporter.ImportObjectsForObjectManager(
					context,
					settings,
					returnAffectedArtifactIDs: false,
					LoadStagingTablesAction);
			}

			IEnumerable<string> itemLevelErrors = await ReadItemLevelErrorsAsync(internalResult.RunID).ConfigureAwait(false);

			var result = new MassImportResults
			{
				ArtifactsProcessed = artifacts.Length,
				ArtifactsCreated = internalResult.ArtifactsCreated,
				ArtifactsUpdated = internalResult.ArtifactsUpdated,
				RunId = internalResult.RunID,
				ExceptionDetail = ConvertExceptionDetail(internalResult.ExceptionDetail),
				ItemErrors = itemLevelErrors,
			};

			return result;
		}

		private static MassImportExceptionDetail ConvertExceptionDetail(Relativity.MassImport.DTO.SoapExceptionDetail exceptionDetail)
		{
			if (exceptionDetail == null)
			{
				return null;
			}

			return new MassImportExceptionDetail()
			{
				ExceptionMessage = exceptionDetail.ExceptionMessage,
				Details = exceptionDetail.Details,
				ExceptionFullText = exceptionDetail.ExceptionFullText,
				ExceptionTrace = exceptionDetail.ExceptionTrace,
				ExceptionType = exceptionDetail.ExceptionType
			};
		}

		private async Task<IEnumerable<string>> ReadItemLevelErrorsAsync(string runId)
		{
			if (string.IsNullOrEmpty(runId))
			{
				return Enumerable.Empty<string>();
			}

			var retrieveErrorsQuery = new QueryInformation
			{
				Statement = $"SELECT [kCura_Import_Status] FROM [EDDS{TestWorkspace.WorkspaceId}].[Resource].[RELNATTMP_{runId}] WHERE [kCura_Import_Status] != 0"
			};
			var errors = await CoreContext.ChicagoContext.DBContext.ExecuteQueryAsReaderAsync(retrieveErrorsQuery).ConfigureAwait(false);
			var output = new List<string>();
			foreach (DbDataRecord error in errors)
			{
				long errorCode = (long)error["kCura_Import_Status"];
				output.Add(errorCode.ToString());
			}

			return output;
		}
	}
}
