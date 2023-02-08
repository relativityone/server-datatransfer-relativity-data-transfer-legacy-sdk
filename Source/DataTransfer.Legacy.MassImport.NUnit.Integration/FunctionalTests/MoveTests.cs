using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Assertions;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;
using Relativity.MassImport.Api;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	using Moq;
	using Relativity.API;

	[TestFixture]
	public class MoveTests : MassImportTestBase
	{
		private IMassImportManager _sut;
		private int _rootFolderId;
		private MassImportField _identifierField;
		private Dictionary<string, Dictionary<string, object>> _importedDocuments;
		private int _testDestinationFolderId;
		private string _testDestinationFolderName;
		private int _testSourceFolderId;
		private string _testSourceFolderName;
		private int _testDeeperDestinationFolderId;
		private string _testDeeperDestinationFolderName;

		private const string TestDestinationFolder = "TestDestinationFolder";
		private const string TestSourceFolder = "TestSourceFolder";

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace)
				.ConfigureAwait(false);
			_identifierField =
				await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace).ConfigureAwait(false);

			List<int> folderIds = await FolderHelper.CreateFoldersAsync(this.TestParameters, TestWorkspace,
				new List<string> {TestDestinationFolder, TestSourceFolder}, _rootFolderId).ConfigureAwait(false);

			_testDestinationFolderId = folderIds.First();
			_testDestinationFolderName = TestWorkspace.WorkspaceName + " | " + TestDestinationFolder;

			_testSourceFolderId = folderIds.Skip(1).First();
			_testSourceFolderName = TestWorkspace.WorkspaceName + " | " + TestSourceFolder;

			List<int> deeperFolderIds = await FolderHelper.CreateFoldersAsync(this.TestParameters, TestWorkspace,
				new List<string> {TestSourceFolder}, _testDestinationFolderId).ConfigureAwait(false);

			_testDeeperDestinationFolderId = deeperFolderIds.First();
			_testDeeperDestinationFolderName = _testDestinationFolderName + " | " + TestSourceFolder;
		}

		[SetUp]
		public async Task SetUpAsync()
		{
			var artifactManager = new ArtifactManager();
			var baseContext = CoreContext.ChicagoContext;

			var settings = GetBasicSettings();

			_sut = new MassImportManager(AssemblySetup.TestLogger, artifactManager, baseContext, new Mock<IHelper>().Object);

			await _sut.RunMassImportAsync(GetInitialRecords(), settings, CancellationToken.None, null)
				.ConfigureAwait(false);

			_importedDocuments = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, (int) ArtifactType.Document, _identifierField.DisplayName,
					new string[] { })
				.ConfigureAwait(false);
		}

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, (int) ArtifactType.Document).ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await FolderHelper.DeleteUnusedFoldersAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
		}

		[Test]
		public async Task ShouldMoveDocumentsFromFoldersToDifferentFoldersWhenOverlayAsync()
		{
			//arrange
			var settings = GetBasicSettings();
			settings.Overlay = OverwriteType.Overlay;
			settings.MoveDocumentsInAppendOverlayMode = true;

			var lastRelevantAuditId = AuditHelper.GetLastRelevantAuditId(TestWorkspace, AuditAction.Move, UserId);
			var expectedAuditDetails = GetExpectedAudit();

			MassImportArtifact[] documentsToImport = GetRecords(
				_testDestinationFolderId,
				_testDeeperDestinationFolderId,
				_testDestinationFolderId,
				_testDeeperDestinationFolderId);

			//act
			var result = await TransactionHelper.WrapInTransaction(ActTask(documentsToImport, settings), Context).ConfigureAwait(false);

			//assert
			Assert.That(result.ExceptionDetail, Is.Null);
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(4));
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(4));
			AuditAssertions.ThenTheAuditIsCorrectAsync(TestWorkspace, UserId, expectedAuditDetails, 4, lastRelevantAuditId, AuditAction.Move);

			var documents = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, (int) ArtifactType.Document, _identifierField.DisplayName,
					new string[] { })
				.ConfigureAwait(false);

			Assert.That(documents.Count, Is.EqualTo(4));
			Assert.That(documents["DOC_1"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDestinationFolderId));
			Assert.That(documents["DOC_2"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDeeperDestinationFolderId));
			Assert.That(documents["DOC_3"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDestinationFolderId));
			Assert.That(documents["DOC_4"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDeeperDestinationFolderId));
		}

		[Test]
		public async Task ShouldMoveDocumentsFromFoldersToDifferentFoldersWhenAppendOverlayAsync()
		{
			//arrange
			var settings = GetBasicSettings();
			settings.Overlay = OverwriteType.Both;
			settings.MoveDocumentsInAppendOverlayMode = true;

			var lastRelevantAuditId = AuditHelper.GetLastRelevantAuditId(TestWorkspace, AuditAction.Move, UserId);
			var expectedAuditDetails = GetExpectedAudit();

			MassImportArtifact[] documentsToImport = GetRecords(
				_testDestinationFolderId,
				_testDeeperDestinationFolderId,
				_testDestinationFolderId,
				_testDeeperDestinationFolderId);

			//act
			var result = await TransactionHelper.WrapInTransaction(ActTask(documentsToImport, settings), Context).ConfigureAwait(false);

			//assert
			Assert.That(result.ExceptionDetail, Is.Null);
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(4));
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(4));
			AuditAssertions.ThenTheAuditIsCorrectAsync(TestWorkspace, UserId, expectedAuditDetails, 4, lastRelevantAuditId, AuditAction.Move);

			var documents = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, (int)ArtifactType.Document, _identifierField.DisplayName,
					new string[] { })
				.ConfigureAwait(false);

			Assert.That(documents.Count, Is.EqualTo(4));
			Assert.That(documents["DOC_1"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDestinationFolderId));
			Assert.That(documents["DOC_2"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDeeperDestinationFolderId));
			Assert.That(documents["DOC_3"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDestinationFolderId));
			Assert.That(documents["DOC_4"][RdoHelper.ParentArtifactId], Is.EqualTo(_testDeeperDestinationFolderId));
		}

		[Test]
		public async Task ShouldNotMoveDocumentsFromFoldersToDifferentFoldersWhenOverlayAsync()
		{
			//arrange
			var settings = GetBasicSettings();
			settings.Overlay = OverwriteType.Overlay;
			settings.MoveDocumentsInAppendOverlayMode = false;

			var lastRelevantAuditId = AuditHelper.GetLastRelevantAuditId(TestWorkspace, AuditAction.Move, UserId);
			var expectedAuditDetails = new List<Dictionary<string, string>>();

			MassImportArtifact[] documentsToImport = GetRecords(
				_testDestinationFolderId,
				_testDeeperDestinationFolderId,
				_testDestinationFolderId,
				_testDeeperDestinationFolderId);

			//act
			var result = await TransactionHelper.WrapInTransaction(ActTask(documentsToImport, settings), Context).ConfigureAwait(false);

			//assert
			Assert.That(result.ExceptionDetail, Is.Null);
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(4));
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(4));
			AuditAssertions.ThenTheAuditIsCorrectAsync(TestWorkspace, UserId, expectedAuditDetails, 4, lastRelevantAuditId, AuditAction.Move);

			var documents = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, (int)ArtifactType.Document, _identifierField.DisplayName,
					new string[] { })
				.ConfigureAwait(false);

			Assert.That(documents.Count, Is.EqualTo(4));
			Assert.That(documents["DOC_1"][RdoHelper.ParentArtifactId], Is.EqualTo(_rootFolderId));
			Assert.That(documents["DOC_2"][RdoHelper.ParentArtifactId], Is.EqualTo(_rootFolderId));
			Assert.That(documents["DOC_3"][RdoHelper.ParentArtifactId], Is.EqualTo(_testSourceFolderId));
			Assert.That(documents["DOC_4"][RdoHelper.ParentArtifactId], Is.EqualTo(_testSourceFolderId));
		}

		[Test]
		public async Task ShouldNotMoveDocumentsFromFoldersToDifferentFoldersWhenAppendOverlayAsync()
		{
			//arrange
			var settings = GetBasicSettings();
			settings.Overlay = OverwriteType.Both;
			settings.MoveDocumentsInAppendOverlayMode = false;

			var lastRelevantAuditId = AuditHelper.GetLastRelevantAuditId(TestWorkspace, AuditAction.Move, UserId);
			var expectedAuditDetails = new List<Dictionary<string, string>>();

			MassImportArtifact[] documentsToImport = GetRecords(
				_testDestinationFolderId,
				_testDeeperDestinationFolderId,
				_testDestinationFolderId,
				_testDeeperDestinationFolderId);

			//act
			var result = await TransactionHelper.WrapInTransaction(ActTask(documentsToImport, settings), Context).ConfigureAwait(false);

			//assert
			Assert.That(result.ExceptionDetail, Is.Null);
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(4));
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(4));
			AuditAssertions.ThenTheAuditIsCorrectAsync(TestWorkspace, UserId, expectedAuditDetails, 4, lastRelevantAuditId, AuditAction.Move);

			var documents = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, (int)ArtifactType.Document, _identifierField.DisplayName,
					new string[] { })
				.ConfigureAwait(false);

			Assert.That(documents.Count, Is.EqualTo(4));
			Assert.That(documents["DOC_1"][RdoHelper.ParentArtifactId], Is.EqualTo(_rootFolderId));
			Assert.That(documents["DOC_2"][RdoHelper.ParentArtifactId], Is.EqualTo(_rootFolderId));
			Assert.That(documents["DOC_3"][RdoHelper.ParentArtifactId], Is.EqualTo(_testSourceFolderId));
			Assert.That(documents["DOC_4"][RdoHelper.ParentArtifactId], Is.EqualTo(_testSourceFolderId));
		}

		private MassImportSettings GetBasicSettings()
		{
			return new MassImportSettings
			{
				ArtifactTypeID = (int) ArtifactType.Document,
				Overlay = OverwriteType.Append,
				MappedFields = new FieldInfo[]
				{
					_identifierField
				},
			};
		}

		private List<Dictionary<string, string>> GetExpectedAudit()
		{
			return new List<Dictionary<string, string>>
			{
				new Dictionary<string, string>
				{
					["destinationFolderName"] = _testDestinationFolderName,
					["sourceFolderArtifactID"] = _rootFolderId.ToString(),
					["sourceFolderName"] = TestWorkspace.WorkspaceName,
					["destinationFolderArtifactID"] = _testDestinationFolderId.ToString(),
					["ArtifactID"] = _importedDocuments["DOC_1"][RdoHelper.ArtifactId].ToString(),
				},
				new Dictionary<string, string>
				{
					["destinationFolderName"] = _testDeeperDestinationFolderName,
					["sourceFolderArtifactID"] = _rootFolderId.ToString(),
					["sourceFolderName"] = TestWorkspace.WorkspaceName,
					["destinationFolderArtifactID"] = _testDeeperDestinationFolderId.ToString(),
					["ArtifactID"] = _importedDocuments["DOC_2"][RdoHelper.ArtifactId].ToString(),
				},
				new Dictionary<string, string>
				{
					["destinationFolderName"] = _testDestinationFolderName,
					["sourceFolderArtifactID"] = _testSourceFolderId.ToString(),
					["sourceFolderName"] = _testSourceFolderName,
					["destinationFolderArtifactID"] = _testDestinationFolderId.ToString(),
					["ArtifactID"] = _importedDocuments["DOC_3"][RdoHelper.ArtifactId].ToString(),
				},
				new Dictionary<string, string>
				{
					["destinationFolderName"] = _testDeeperDestinationFolderName,
					["sourceFolderArtifactID"] = _testSourceFolderId.ToString(),
					["sourceFolderName"] = _testSourceFolderName,
					["destinationFolderArtifactID"] = _testDeeperDestinationFolderId.ToString(),
					["ArtifactID"] = _importedDocuments["DOC_4"][RdoHelper.ArtifactId].ToString(),
				},
			};
		}

		private MassImportArtifact[] GetRecords(
			int firstDocParent,
			int secondDocParent,
			int thirdDocParent,
			int fourthDocParent)
		{
			return new[]
			{
				new MassImportArtifact(
					new List<object>
					{
						"DOC_1"
					},
					parentFolderId: firstDocParent),
				new MassImportArtifact(
					new List<object>
					{
						"DOC_2"
					},
					parentFolderId: secondDocParent),
				new MassImportArtifact(
					new List<object>
					{
						"DOC_3"
					},
					parentFolderId: thirdDocParent),
				new MassImportArtifact(
					new List<object>
					{
						"DOC_4"
					},
					parentFolderId: fourthDocParent),
			};
		}

		private MassImportArtifact[] GetInitialRecords()
		{
			return GetRecords(_rootFolderId, _rootFolderId, _testSourceFolderId, _testSourceFolderId);
		}

		private Func<Task<MassImportResults>> ActTask(MassImportArtifact[] documentsToImport, MassImportSettings settings)
		{
			return () => _sut
				.RunMassImportAsync(documentsToImport, settings, CancellationToken.None, null);
		}
	}
}