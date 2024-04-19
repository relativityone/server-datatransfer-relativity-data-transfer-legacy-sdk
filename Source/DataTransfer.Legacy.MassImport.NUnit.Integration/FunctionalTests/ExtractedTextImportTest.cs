using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using DataTransfer.Legacy.MassImport.Data;
using FluentAssertions;
using FluentAssertions.Execution;
using MassImport.NUnit.Integration.Helpers;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.Api;
using Relativity.MassImport.DTO;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture(false)]
	[TestFixture(true, IgnoreReason = "Test requires direct access to the fileshare")]
	public class ExtractedTextImportTest : MassImportTestBase
	{
		private const int _artifactTypeId = (int)ArtifactType.Document;

		private readonly bool _useDataGrid;

		private bool _wasNewWorkspaceCreated;
		private int _rootFolderId;
		private FieldInfo _identifierField;
		private FieldInfo _extractedTextField;

		public ExtractedTextImportTest(bool useDataGrid)
		{
			_useDataGrid = useDataGrid;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			_extractedTextField = await FieldHelper.ReadExtractedTextField(TestParameters, TestWorkspace).ConfigureAwait(false);
			if (_useDataGrid && !_extractedTextField.EnableDataGrid)
			{
				await CreateNewTestWorkspace().ConfigureAwait(false);
				await FieldHelper.EnableDataGridForFieldAsync(TestParameters, TestWorkspace, _extractedTextField.ArtifactID, _extractedTextField.DisplayName).ConfigureAwait(false);
				_wasNewWorkspaceCreated = true;
			}
			else if (!_useDataGrid && _extractedTextField.EnableDataGrid)
			{
				// DataGrid is disabled by default, so we need to create a new workspace to test the case when DataGrid is disabled
				await CreateNewTestWorkspace().ConfigureAwait(false);
				_wasNewWorkspaceCreated = true;
			}

			if (_useDataGrid)
			{
				kCura.Config.Manager.SetValue(section: "Relativity.DataGrid", name: "DataGridIndexPrefix", value: "emttest", machineName: string.Empty);

				StorageAccessProvider.InitializeStorageAccess(Mock.Of<IWindsorContainer>());
				var storageAccess = await Relativity.Storage.StorageAccessFactory.CreateLocalAccessAsync("PTCI-4941411").ConfigureAwait(false);
				StorageAccessProvider.SetStorageAccess(storageAccess);
			}

			_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
			_identifierField = await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace, _artifactTypeId).ConfigureAwait(false);
			_extractedTextField = await FieldHelper.ReadExtractedTextField(TestParameters, TestWorkspace).ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDown()
		{
			if (_wasNewWorkspaceCreated)
			{
				await AssemblySetup.OneTimeTearDownAsync().ConfigureAwait(false);
				AssemblySetup.OneTimeSetUp();
			}
		}

		[SetUp]
		public async Task SetUp()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, _artifactTypeId).ConfigureAwait(false);
		}

		[Test]
		public async Task ApiMassImportManager_ImportsExtractedText_PassedByValue()
		{
			// Arrange
			const string expectedIdentifier = "RecordWithText_1";
			const string expectedExtractedText = "Extracted Text Value";

			var artifactManager = new ArtifactManager();
			var baseContext = CoreContext.ChicagoContext;
			var sut = new MassImportManager(AssemblySetup.TestLogger, artifactManager, baseContext);

			var fields = new List<FieldInfo> { _identifierField, _extractedTextField };
			var appendSettings = new MassImportSettings
			{
				ArtifactTypeID = _artifactTypeId,
				Overlay = OverwriteType.Append,
				MappedFields = fields.ToArray(),
			};

			var fieldsValues = new List<object> { expectedIdentifier, expectedExtractedText };
			var artifacts = new[]
			{
				new MassImportArtifact(fieldsValues, parentFolderId: _rootFolderId)
			};

			// Act
			var appendResult = await sut.RunMassImportAsync(artifacts, appendSettings, CancellationToken.None, progress: null).ConfigureAwait(false);

			// Assert
			appendResult.Should().NotBeNull();
			using (new AssertionScope())
			{
				appendResult.ExceptionDetail.Should().BeNull();
				appendResult.ArtifactsCreated.Should().Be(1);
			}

			var fieldNames = new[] { _extractedTextField.DisplayName };
			var objects = await RdoHelper.ReadObjects(TestParameters, TestWorkspace, _artifactTypeId, _identifierField.DisplayName, fieldNames).ConfigureAwait(false);
			objects.Should().HaveCount(1);
			objects.Should().ContainKey(expectedIdentifier);
			objects[expectedIdentifier].Should().ContainKey(_extractedTextField.DisplayName);
			objects[expectedIdentifier][_extractedTextField.DisplayName].Should().Be(expectedExtractedText);
		}
	}
}