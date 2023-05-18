using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport;
using Relativity.MassImport.Api;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture(false)]
	[TestFixture(true)]
	public class ChoicesTests : MassImportTestBase
	{
		private readonly bool _massImportImprovementsToggle;
		
		private int _rootFolderId;
		private MassImportField _identifierField;

		private IMassImportManager _sut;
		private MassImportField _singleChoiceField;
		private Dictionary<string, int> _singleChoiceNameToArtifactIdMap;
		private MassImportField _multiChoiceField;
		private Dictionary<string, int> _multiChoiceNameToArtifactIdMap;
		
		private const int ImportObjectType = (int)ArtifactType.Document;

		public ChoicesTests(bool massImportImprovementsToggle)
		{
			_massImportImprovementsToggle = massImportImprovementsToggle;
		}
		
		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			SettingsHelper.SetToggle<Relativity.MassImport.Toggles.EnableMassImportImprovementsInMassImportManager>(_massImportImprovementsToggle);

			_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
			_identifierField = await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace).ConfigureAwait(false);

			_singleChoiceField = await FieldHelper
				.CreateSingleChoiceField(TestParameters, TestWorkspace, "SingleChoice", ImportObjectType)
				.ConfigureAwait(false);

			string[] singleChoiceValues = { "A", "B", "C" };

			_singleChoiceNameToArtifactIdMap = await ChoiceHelper
				.CreateChoiceValuesAsync(TestParameters, TestWorkspace, _singleChoiceField.ArtifactID, singleChoiceValues)
				.ConfigureAwait(false);

			_multiChoiceField = await FieldHelper
				.CreateMultiChoiceField(TestParameters, TestWorkspace, "MultiChoice", ImportObjectType)
				.ConfigureAwait(false);

			string[] multiChoiceValues = { "X", "Y", "Z" };

			_multiChoiceNameToArtifactIdMap = await ChoiceHelper
				.CreateChoiceValuesAsync(TestParameters, TestWorkspace, _multiChoiceField.ArtifactID, multiChoiceValues)
				.ConfigureAwait(false);
		}

		[SetUp]
		public void SetUp()
		{
			var artifactManager = new ArtifactManager();
			var baseContext = CoreContext.ChicagoContext;

			_sut = new MassImportManager(OneTimeSetup.TestLogger, artifactManager, baseContext);
		}

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, (int)ArtifactType.Document).ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await FieldHelper.DeleteFieldAsync(TestParameters, TestWorkspace, _singleChoiceField.ArtifactID).ConfigureAwait(false);
			await FieldHelper.DeleteFieldAsync(TestParameters, TestWorkspace, _multiChoiceField.ArtifactID).ConfigureAwait(false);
		}

		[Test]
		public async Task ShouldImportNewDocumentsWithSingleAndMultiChoices()
		{
			// arrange
			var settings = new MassImportSettings
			{
				ArtifactTypeID = ImportObjectType,
				Overlay = OverwriteType.Append,
				MappedFields = new FieldInfo[]
				{
					_identifierField,
					_singleChoiceField,
					_multiChoiceField
				},
			};

			MassImportArtifact[] recordsToImport =
			{
				new MassImportArtifact(
					new List<object>
					{
						"DOC_1",
						_singleChoiceNameToArtifactIdMap["A"],
						new[]{_multiChoiceNameToArtifactIdMap["Y"], _multiChoiceNameToArtifactIdMap["Z"] }
					}, 
					parentFolderId: _rootFolderId),
				new MassImportArtifact(
					new List<object>
					{
						"DOC_2", 
						_singleChoiceNameToArtifactIdMap["B"],
						new[]{ _multiChoiceNameToArtifactIdMap["X"], _multiChoiceNameToArtifactIdMap["Y"] }
					}, 
					parentFolderId: _rootFolderId),
				new MassImportArtifact(
					new List<object>
					{
						"DOC_3",
						_singleChoiceNameToArtifactIdMap["C"],
						new[]{ _multiChoiceNameToArtifactIdMap["X"], _multiChoiceNameToArtifactIdMap["Z"] }
					}, 
					parentFolderId: _rootFolderId),
			};

			// act
			var result = await _sut.RunMassImportAsync(recordsToImport, settings, CancellationToken.None, null).ConfigureAwait(false);

			// assert
			// assert - import report
			Assert.That(result.ExceptionDetail, Is.Null);
			Assert.That(result.ArtifactsProcessed, Is.EqualTo(3));
			Assert.That(result.ArtifactsCreated, Is.EqualTo(3));

			string[] fieldsToValidate = { _singleChoiceField.DisplayName, _multiChoiceField.DisplayName };
			var documents = await RdoHelper
				.ReadObjects(TestParameters, TestWorkspace, ImportObjectType, _identifierField.DisplayName, fieldsToValidate)
				.ConfigureAwait(false);

			// assert - single choice
			(string docId, int choiceId)[] expectedSingleChoiceMapping =
			{
				("DOC_1", _singleChoiceNameToArtifactIdMap["A"]),
				("DOC_2", _singleChoiceNameToArtifactIdMap["B"]),
				("DOC_3", _singleChoiceNameToArtifactIdMap["C"]),
			};

			(string docId, int choiceId)[] actualSingleChoiceMapping = documents
				.Select(x => (x.Key, (int)x.Value[_singleChoiceField.DisplayName]))
				.ToArray();

			CollectionAssert.AreEquivalent(expectedSingleChoiceMapping, actualSingleChoiceMapping, "All single choices should be imported");

			// assert - multi choice
			(string docId, int[] choiceIds)[] expectedMultiChoiceMapping =
			{
				("DOC_1", new [] { _multiChoiceNameToArtifactIdMap["Y"], _multiChoiceNameToArtifactIdMap["Z"]}),
				("DOC_2", new [] { _multiChoiceNameToArtifactIdMap["X"], _multiChoiceNameToArtifactIdMap["Y"]}),
				("DOC_3", new [] { _multiChoiceNameToArtifactIdMap["X"], _multiChoiceNameToArtifactIdMap["Z"]}),
			};

			(string docId, int[] choiceIds)[] actualMultiChoiceMapping = documents
				.Select(x => (x.Key, (int[])x.Value[_multiChoiceField.DisplayName]))
				.ToArray();

			CollectionAssert.AreEquivalent(expectedMultiChoiceMapping, actualMultiChoiceMapping, "All multi choices should be imported");
		}
	}
}
