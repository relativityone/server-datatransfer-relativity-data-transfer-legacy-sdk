using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DataTransfer.Legacy.MassImport.Toggles;
using FluentAssertions;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.Api;
using Relativity.MassImport.DTO;
using Relativity.Toggles;
using Relativity.Toggles.Providers;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;
using MassImportManager = Relativity.MassImport.Api.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture(_documentArtifactTypeName)]
	[TestFixture(_rdoArtifactTypeName)]
	public class FieldsBatchingTests : MassImportTestBase
	{
		private const string _documentArtifactTypeName = "Document";
		private const string _rdoArtifactTypeName = "TestRDO";
		private const int _userId = 9;

		private const int _numberOfRecordsToImport = 10;
		private const int _maxNumberOfWholeNumberFields = 800;
		private const int _maxNumberOfLongTextFields = 100;
		private const int _maxNumberOfSingleChoiceFields = 20;
		private const int _maxNumberOfMultiChoiceFields = 20;
		private const int _maxNumberOfSingleObjectFields = 20;
		private const int _maxNumberOfMultiObjectFields = 20;

		private readonly string _artifactTypeName;

		private int _artifactTypeId;
		private int _rootFolderId;
		private int _associatedObjectArtifactTypeId;
		private FieldInfo _identifierField;
		private FieldInfo[] _fixedLengthTextFields;
		private FieldInfo[] _singleChoiceFields;
		private FieldInfo[] _multiChoiceFields;
		private FieldInfo[] _singleObjectFields;
		private FieldInfo[] _multiObjectFields;
		private FieldInfo[] _wholeNumberFields;

		private Dictionary<int, List<int>> _choiceFieldToValueMap;
		private Dictionary<int, List<int>> _multiObjectFieldToValueMap;

		private InMemoryToggleProvider _toggleProvider;

		private MassImportManager _sut;

		public FieldsBatchingTests(string artifactTypeName)
		{
			_artifactTypeName = artifactTypeName;
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			if (_artifactTypeName == _documentArtifactTypeName)
			{
				_artifactTypeId = (int)ArtifactType.Document;
			}
			else
			{
				_artifactTypeId = await RdoHelper.CreateObjectTypeAsync(TestParameters, TestWorkspace, _artifactTypeName).ConfigureAwait(false);
			}

			_toggleProvider = new InMemoryToggleProvider();
			ToggleProvider.Current = _toggleProvider;

			_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(TestParameters, TestWorkspace).ConfigureAwait(false);
			_identifierField = await FieldHelper.ReadIdentifierField(TestParameters, TestWorkspace, _artifactTypeId).ConfigureAwait(false);
			_associatedObjectArtifactTypeId = await RdoHelper.CreateObjectTypeAsync(TestParameters, TestWorkspace, $"Associated_{_artifactTypeName}").ConfigureAwait(false);

			await CreateFieldsAsync().ConfigureAwait(false);
		}

		[SetUp]
		public async Task SetUp()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, _associatedObjectArtifactTypeId).ConfigureAwait(false);
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, _artifactTypeId).ConfigureAwait(false);

			await CreateMultiObjectFieldsInstancesAsync().ConfigureAwait(false);

			var artifactManager = new ArtifactManager();
			var baseContext = CoreContext.ChicagoContext;

			_sut = new MassImportManager(AssemblySetup.TestLogger, artifactManager, baseContext, HelperMock.Object);
			//_sut = new MassImportManager(AssemblySetup.TestLogger, artifactManager, baseContext);

		}

		[TestCase(true, 100, 800)]
		[TestCase(true, 0, 0)]
		[TestCase(false, 100, 300)]
		[TestCase(false, 0, 0)]
		public async Task ShouldImportRecordsWithMultipleFieldsMapped(
			bool enableUpdateMetadataOptimizationToggleValue,
			int numberOfTextFields,
			int numberOfWholeNumberFields)
		{
			// arrange
			await _toggleProvider.SetAsync<EnableUpdateMetadataOptimization>(enabled: enableUpdateMetadataOptimizationToggleValue);

			await AppendRecordsWithoutMetadataAsync().ConfigureAwait(false);

			var overlaySettings = GetImportSettingsForOverlay(numberOfTextFields, numberOfWholeNumberFields);
			var recordsMetadata = GetRecordsToImport(overlaySettings.MappedFields);
			var massImportArtifacts = recordsMetadata
				.Select(x => ConvertToMassImportArtifact(overlaySettings.MappedFields, x.Value));

			var lastAuditId = AuditHelper.GetLastRelevantAuditId(TestWorkspace, AuditAction.Import, _userId);

			// act
			var overlayResult = await _sut.RunMassImportAsync(massImportArtifacts, overlaySettings, CancellationToken.None, null).ConfigureAwait(false);

			// assert
			VerifyOverlayResultIsSuccessful(overlayResult);
			var identifierToArtifactIdMapping = await VerifyRecordsImportedAsync(overlaySettings, recordsMetadata).ConfigureAwait(false);
			VerifyAudit(lastAuditId, recordsMetadata, identifierToArtifactIdMapping, overlaySettings);
		}

		private async Task AppendRecordsWithoutMetadataAsync()
		{
			var appendSettings = GetImportSettingsForAppend();
			var recordsToAppend = CreateRecordsToAppend();
			var appendResult = await _sut
				.RunMassImportAsync(recordsToAppend, appendSettings, CancellationToken.None, progress: null).ConfigureAwait(false);
			appendResult.Should().NotBeNull();
			appendResult.ArtifactsCreated.Should().Be(_numberOfRecordsToImport);
		}

		private MassImportSettings GetImportSettingsForAppend()
		{
			var fields = new List<FieldInfo> { _identifierField };

			return new MassImportSettings
			{
				ArtifactTypeID = _artifactTypeId,
				Overlay = OverwriteType.Append,
				MappedFields = fields.ToArray(),
			};
		}

		private IEnumerable<MassImportArtifact> CreateRecordsToAppend()
		{
			for (int i = 0; i < _numberOfRecordsToImport; i++)
			{
				var fieldsValues = new List<object> { $"Record_{i}" };
				yield return new MassImportArtifact(fieldsValues, parentFolderId: _rootFolderId);
			}
		}

		private MassImportSettings GetImportSettingsForOverlay(int numberOfTextFields, int numberOfWholeNumberFields)
		{
			var fields = new List<FieldInfo> { _identifierField };
			fields.AddRange(_fixedLengthTextFields.Take(numberOfTextFields));
			fields.AddRange(_wholeNumberFields.Take(numberOfWholeNumberFields));
			fields.AddRange(_singleChoiceFields);
			fields.AddRange(_multiChoiceFields);
			fields.AddRange(_singleObjectFields);
			fields.AddRange(_multiObjectFields);
			Shuffle(fields);

			return new MassImportSettings
			{
				ArtifactTypeID = _artifactTypeId,
				Overlay = OverwriteType.Overlay,
				MappedFields = fields.ToArray(),
			};
		}

		private Dictionary<string, Dictionary<string, object>> GetRecordsToImport(FieldInfo[] fields)
		{
			var records = new Dictionary<string, Dictionary<string, object>>();
			for (int i = 0; i < _numberOfRecordsToImport; i++)
			{
				var identifier = $"Record_{i}";
				records[identifier] = GetValuesForFields(identifier, fields);
			}

			return records;
		}

		private Dictionary<string, object> GetValuesForFields(string recordsIdentifier, FieldInfo[] fields)
		{
			var generator = new Random();
			var values = new Dictionary<string, object>
			{
				[_identifierField.DisplayName] = recordsIdentifier
			};

			foreach (var fieldInfo in fields.Where(x => x.ArtifactID != _identifierField.ArtifactID))
			{
				object value;
				switch (fieldInfo.Type)
				{
					case FieldTypeHelper.FieldType.Varchar:
						value = Guid.NewGuid().ToString();
						break;
					case FieldTypeHelper.FieldType.Integer:
						value = generator.Next();
						break;
					case FieldTypeHelper.FieldType.Code:
						value = _choiceFieldToValueMap[fieldInfo.ArtifactID].Single();
						break;
					case FieldTypeHelper.FieldType.MultiCode:
						value = _choiceFieldToValueMap[fieldInfo.ArtifactID].ToArray();
						break;
					case FieldTypeHelper.FieldType.Object:
						value = Guid.NewGuid().ToString();
						break;
					case FieldTypeHelper.FieldType.Objects:
						value = _multiObjectFieldToValueMap[fieldInfo.ArtifactID].ToArray();
						break;
					default:
						throw new InvalidOperationException("FieldType not supported");
				}

				values[fieldInfo.DisplayName] = value;
			}

			return values;
		}

		private MassImportArtifact ConvertToMassImportArtifact(FieldInfo[] fields, Dictionary<string, object> values)
		{
			var fieldsValues = fields.Select(field => values[field.DisplayName]).ToList();
			return new MassImportArtifact(fieldsValues, parentFolderId: _rootFolderId);
		}

		private static void VerifyOverlayResultIsSuccessful(MassImportResults overlayResult)
		{
			overlayResult.Should().NotBeNull();
			overlayResult.ExceptionDetail.Should().BeNull();
			overlayResult.ItemErrors.Should().BeNullOrEmpty();
			overlayResult.ArtifactsProcessed.Should().Be(_numberOfRecordsToImport);
			overlayResult.ArtifactsCreated.Should().Be(0);
			overlayResult.ArtifactsUpdated.Should().Be(_numberOfRecordsToImport);
		}

		private async Task<Dictionary<string, int>> VerifyRecordsImportedAsync(MassImportSettings overlaySettings, Dictionary<string, Dictionary<string, object>> recordsToOverlayMetadata)
		{
			var identifierToArtifactIdMapping = new Dictionary<string, int>();

			var fieldNames = overlaySettings.MappedFields.Where(x => x.ArtifactID != _identifierField.ArtifactID).Select(x => x.DisplayName).ToArray();
			var objects = await RdoHelper.ReadObjects(TestParameters, TestWorkspace, _artifactTypeId, _identifierField.DisplayName, fieldNames).ConfigureAwait(false);

			foreach (var recordMetadata in recordsToOverlayMetadata)
			{
				var expectedRecordIdentifier = recordMetadata.Key;
				var expectedFieldsValues = recordMetadata.Value;

				objects.Should().ContainKey(expectedRecordIdentifier);
				foreach (var expectedFieldsValue in expectedFieldsValues.Where(x => x.Key != _identifierField.DisplayName))
				{
					var expectedFieldName = expectedFieldsValue.Key;
					var expectedFieldValue = expectedFieldsValue.Value;

					objects[expectedRecordIdentifier].Should().ContainKey(expectedFieldName);
					objects[expectedRecordIdentifier][expectedFieldName].Should().BeEquivalentTo(expectedFieldValue);
				}

				identifierToArtifactIdMapping[expectedRecordIdentifier] = (int)objects[expectedRecordIdentifier][RdoHelper.ArtifactId];
			}

			return identifierToArtifactIdMapping;
		}

		private void VerifyAudit(
			int lastAuditId,
			Dictionary<string, Dictionary<string, object>> recordsMetadata,
			Dictionary<string, int> identifierToArtifactIdMapping,
			MassImportSettings overlaySettings)
		{
			var audits =
				AuditHelper.GetAuditDetailsXmlAndRecordArtifactId(TestWorkspace, AuditAction.Update_Import, lastAuditId, 1000,
					_userId);
			foreach (var recordMetadata in recordsMetadata)
			{
				var expectedRecordIdentifier = recordMetadata.Key;
				var expectedRecordArtifactId = identifierToArtifactIdMapping[expectedRecordIdentifier];
				var expectedFieldsValues = recordMetadata.Value;

				audits.Should().ContainKey(expectedRecordArtifactId, "because it should audit overlay");

				XDocument auditDetails;
				try
				{
					auditDetails = XDocument.Parse(audits[expectedRecordArtifactId]);
					auditDetails.Should().NotBeNull();
				}
				catch
				{
					Assert.Fail(
						$"Invalid audit details for record: {expectedRecordIdentifier}. Content: {audits[expectedRecordArtifactId]}");
					throw;
				}

				foreach (var fieldInfo in overlaySettings.MappedFields.Where(x => x.ArtifactID != _identifierField.ArtifactID))
				{
					int fieldId = fieldInfo.ArtifactID;
					auditDetails.Document.Descendants("field").Should().Contain(x => x.Attribute("id").Value == fieldId.ToString());
					var auditForField = auditDetails.Descendants("field")
						.SingleOrDefault(x => x.Attribute("id") != null && x.Attribute("id").Value == fieldId.ToString());

					auditForField.Should().NotBeNull();
					auditForField.Should().HaveAttribute("name", fieldInfo.DisplayName);

					switch (fieldInfo.Type)
					{
						case FieldTypeHelper.FieldType.Varchar:
							auditForField.Should().HaveAttribute("type", "0");
							auditForField.Should().HaveElement("oldValue").Which.Value.Should().BeNullOrEmpty();
							auditForField.Should().HaveElement("newValue").Which.Value.Should()
								.Be(expectedFieldsValues[fieldInfo.DisplayName].ToString());
							break;
						case FieldTypeHelper.FieldType.Integer:
							auditForField.Should().HaveAttribute("type", "1");
							auditForField.Should().HaveElement("oldValue").Which.Value.Should().BeNullOrEmpty();
							auditForField.Should().HaveElement("newValue").Which.Value.Should()
								.Be(expectedFieldsValues[fieldInfo.DisplayName].ToString());
							break;
						case FieldTypeHelper.FieldType.Code:
							auditForField.Should().HaveAttribute("type", "5");
							// MassImportManager.RunMassImportAsync does not set single choice IDs in RELNATTMP temp table
							// That does not impact the import result, but it impacts the audit
							// https://jira.kcura.com/browse/REL-903107
							break;
						case FieldTypeHelper.FieldType.MultiCode:
							auditForField.Should().HaveAttribute("type", "8");
							var setChoiceValues = auditForField.Descendants("setChoice").Select(x => x.Value).ToArray();
							var expectedSetChoicesValues = (expectedFieldsValues[fieldInfo.DisplayName] as int[])?.Select(x => x.ToString());
							setChoiceValues.Should().BeEquivalentTo(expectedSetChoicesValues);
							break;
						case FieldTypeHelper.FieldType.Object:
							auditForField.Should().HaveAttribute("type", "10");
							auditForField.Should().HaveElement("oldValue").Which.Value.Should().BeNullOrEmpty();
							auditForField.Should().HaveElement("newValue").Which.Value.Should().NotBeNullOrEmpty();
							int.TryParse(auditForField.Element("newValue").Value, out int _).Should()
								.BeTrue("because it should contain artifactID");
							break;
						case FieldTypeHelper.FieldType.Objects:
							auditForField.Should().HaveAttribute("type", "13");
							var setValues = auditForField.Descendants("set").Select(x => x.Value).ToArray();
							var expectedSetValues = (expectedFieldsValues[fieldInfo.DisplayName] as int[])?.Select(x => x.ToString());
							setValues.Should().BeEquivalentTo(expectedSetValues);
							break;
						default:
							throw new InvalidOperationException("FieldType not supported");
					}
				}
			}
		}

		private async Task CreateFieldsAsync()
		{
			_fixedLengthTextFields = new FieldInfo[_maxNumberOfLongTextFields];
			for (int i = 0; i < _maxNumberOfLongTextFields; i++)
			{
				_fixedLengthTextFields[i] = await FieldHelper.CreateFixedLengthTextField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}FixedLengthTextField{i}",
					_artifactTypeId).ConfigureAwait(false);
			}

			_choiceFieldToValueMap = new Dictionary<int, List<int>>();
			_singleChoiceFields = new FieldInfo[_maxNumberOfSingleChoiceFields];
			for (int i = 0; i < _maxNumberOfSingleChoiceFields; i++)
			{
				_singleChoiceFields[i] = await FieldHelper.CreateSingleChoiceField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}SingleChoiceField{i}",
					_artifactTypeId).ConfigureAwait(false);

				string choiceName = $"{_artifactTypeName}SingleChoice{i}";
				var createdChoiceValue = await ChoiceHelper.CreateChoiceValuesAsync(
					TestParameters,
					TestWorkspace,
					_singleChoiceFields[i].ArtifactID,
					new[] { choiceName }).ConfigureAwait(false);
				_choiceFieldToValueMap[_singleChoiceFields[i].ArtifactID] = new List<int> { createdChoiceValue[choiceName] };
			}

			_multiChoiceFields = new FieldInfo[_maxNumberOfMultiChoiceFields];
			for (int i = 0; i < _maxNumberOfMultiChoiceFields; i++)
			{
				_multiChoiceFields[i] = await FieldHelper.CreateMultiChoiceField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}MultiChoiceField{i}",
					_artifactTypeId).ConfigureAwait(false);

				var createdChoiceValue = await ChoiceHelper.CreateChoiceValuesAsync(
					TestParameters,
					TestWorkspace,
					_multiChoiceFields[i].ArtifactID,
					new[] { $"{_artifactTypeName}MultiChoice{i}-1", $"{_artifactTypeName}MultiChoice{i}-2" }).ConfigureAwait(false);
				_choiceFieldToValueMap[_multiChoiceFields[i].ArtifactID] = createdChoiceValue.Values.ToList();
			}

			_wholeNumberFields = new FieldInfo[_maxNumberOfWholeNumberFields];
			for (int i = 0; i < _maxNumberOfWholeNumberFields; i++)
			{
				_wholeNumberFields[i] = await FieldHelper.CreateWholeNumberField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}WholeNumber{i}",
					_artifactTypeId).ConfigureAwait(false);
			}

			_singleObjectFields = new FieldInfo[_maxNumberOfSingleObjectFields];
			for (int i = 0; i < _maxNumberOfSingleObjectFields; i++)
			{
				_singleObjectFields[i] = await FieldHelper.CreateSingleObjectField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}SingleObject{i}",
					_artifactTypeId,
					_associatedObjectArtifactTypeId).ConfigureAwait(false);
			}

			_multiObjectFields = new FieldInfo[_maxNumberOfMultiObjectFields];
			for (int i = 0; i < _maxNumberOfMultiObjectFields; i++)
			{
				_multiObjectFields[i] = await FieldHelper.CreateMultiObjectField(
					TestParameters,
					TestWorkspace,
					$"{_artifactTypeName}MultiObject{i}",
					_artifactTypeId,
					_associatedObjectArtifactTypeId).ConfigureAwait(false);
			}
		}

		private async Task CreateMultiObjectFieldsInstancesAsync()
		{
			var random = new Random(Seed: 542);
			_multiObjectFieldToValueMap = new Dictionary<int, List<int>>();
			for (int i = 0; i < _maxNumberOfMultiObjectFields; i++)
			{
				var numberOfInstances = random.Next(1, 3);
				var objectsNames = Enumerable.Range(0, numberOfInstances).Select(x => Guid.NewGuid().ToString()).ToList();
				var createdObjects = await RdoHelper.CreateObjectsAsync(
					TestParameters,
					TestWorkspace,
					_associatedObjectArtifactTypeId,
					objectsNames).ConfigureAwait(false);
				_multiObjectFieldToValueMap[_multiObjectFields[i].ArtifactID] = createdObjects.Values.ToList();
			}
		}

		public static void Shuffle<T>(IList<T> list)
		{
			Random rng = new Random(Seed: 9821);
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}
		}
	}
}