using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.FunctionalTests.Helpers;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.RingSetup;
using Relativity.Testing.Identification;
using FieldType = Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models.FieldType;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Performance
{
	[TestLevel.L3]
	[TestType.Performance]
	[TestExecutionCategory.CD]
	[IdentifiedTestFixture("a87afad8-ebe4-4136-b278-c77d38f4d06a")]
	[Feature.DataTransfer.ImportApi.Operations.ImportDocuments]
	public class NativeImportTest : TestSetup
	{
		public NativeImportTest()
			: base("DataTransfer.Legacy", desiredNumberOfDocuments: 0, importImages: false)
		{ }

		private DataTable _expectedFieldValues;
		private List<FieldRef> _expectedFieldsNames;

		private const int ImportObjectType = (int)ArtifactType.Document;
		private const int MaxItemsToFetch = 5000;
		private const int NumberOfDocuments = 1000;
		private const int NumberOfCustomFields = 25;
		private const int CustomFieldLength = 255;
		private const string FieldDelimiter = "þþKþþ";

		private SDK.ImportExport.V1.Models.FieldInfo _identifierField;
		private Dictionary<string, int> _customFieldsDictionary = new Dictionary<string, int>();
		private int _rootFolderId;



		[SetUp]
		public async Task SetUp()
		{
			await StopwatchHelper.RunWithStopwatchAsync(
					 () => ObjectsHelper.DeleteAllObjectsByTypeAsync(_workspace, ImportObjectType),
					s => TestContext.Out.WriteLineAsync(s)
					, "DeleteAllObjectsByTypeAsync before test")
				.ConfigureAwait(false);
		}

		[TearDown]
		public async Task TearDown()
		{
			await StopwatchHelper.RunWithStopwatchAsync(
				() => ObjectsHelper.DeleteAllObjectsByTypeAsync(_workspace, ImportObjectType),
				s => TestContext.Out.WriteLineAsync(s)
					, "DeleteAllObjectsByTypeAsync after test")
				.ConfigureAwait(false);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
		{
			async Task InitializeFieldsAsync()
			{
				_rootFolderId = await FolderHelper.ReadRootFolderIdAsync(_workspace.ArtifactID).ConfigureAwait(false);
				_identifierField = await Helpers.FieldHelper.ReadIdentifierField(_workspace.ArtifactID).ConfigureAwait(false);

				for (int i = 0; i < NumberOfCustomFields; i++)
				{
					var field = await Helpers.FieldHelper.CreateFixedLengthTextFieldAsync($"IApiField{i}", ImportObjectType, true, CustomFieldLength, _workspace.ArtifactID).ConfigureAwait(false);
					_customFieldsDictionary.Add($"IApiField{i}", field);
				}
			}

			await StopwatchHelper.RunWithStopwatchAsync(
					InitializeFieldsAsync,
					s => TestContext.Out.WriteLineAsync(s)
					, $"Create fields before tests, Count: {NumberOfCustomFields}")
				.ConfigureAwait(false);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			await StopwatchHelper.RunWithStopwatchAsync(
				async () =>
				{
					foreach (var customFieldId in _customFieldsDictionary.Values)
					{
						await Helpers.FieldHelper.DeleteFieldAsync(_workspace.ArtifactID, customFieldId).ConfigureAwait(false);
					}
				},
					s => TestContext.Out.WriteLineAsync(s)
					, "Remove fields after tests")
				.ConfigureAwait(false);
		}


		[IdentifiedTest("b0be55c3-3922-49be-a03d-6d2659888ebc")]
		[TestExecutionCategory.RAPCD.Verification.NonFunctional]
		public async Task ShouldRunNativeImport()
		{
			//Arrange
			SDK.ImportExport.V1.Models.NativeLoadInfo nativeLoadInfo = await CreateSampleNativeLoadInfoAsync(NumberOfDocuments).ConfigureAwait(false);


			MassImportResults result;
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (IBulkImportService bulkImportService = serviceFactory.GetServiceProxy<IBulkImportService>())
			{
				var stopwatch = new Stopwatch();
				//Act
				result = await StopwatchHelper.RunWithStopwatchAsync(
						async () => await bulkImportService.BulkImportNativeAsync(_workspace.ArtifactID, nativeLoadInfo, true, false, Guid.NewGuid().ToString()).ConfigureAwait(false),
					s => TestContext.Out.WriteLineAsync(s),
					"BulkImportNativeAsync",
						stopwatch)
					.ConfigureAwait(false);

				//Assert
				Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(15)), "BulkImport should take less then 15 seconds");
			}

			await TestContext.Out.WriteLineAsync($"BulkImportNativeAsync Finished RunId: {result.RunID}, ArtifactsCreated: {result.ArtifactsCreated}").ConfigureAwait(false);

			// Assert
			Assert.That(result.ExceptionDetail, Is.Null, $"An error occurred when running import: {result.ExceptionDetail?.ExceptionMessage}");
			Assert.That(result.ArtifactsCreated, Is.EqualTo(NumberOfDocuments));
			Assert.That(result.ArtifactsUpdated, Is.EqualTo(0));
			Assert.That(result.FilesProcessed, Is.EqualTo(0));

			await StopwatchHelper.RunWithStopwatchAsync(async () =>
			{
				await GetAndAssertAllDocuments().ConfigureAwait(false);
			}, s => TestContext.Out.WriteLineAsync(s), $"Assert all {NumberOfDocuments} documents").ConfigureAwait(false);

		}

		private async Task GetAndAssertAllDocuments()
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (IObjectManager objectManager = serviceFactory.GetServiceProxy<IObjectManager>())
			{
				QueryRequest queryRequest = new QueryRequest
				{
					Fields = _expectedFieldsNames,
					ObjectType = new ObjectTypeRef { ArtifactTypeID = ImportObjectType },
				};
				var objects = await objectManager
					.QueryAsync(_workspace.ArtifactID, queryRequest, 0, MaxItemsToFetch).ConfigureAwait(false);

				Assert.That(objects.TotalCount, Is.EqualTo(NumberOfDocuments));
				ThenTheFieldsHaveCorrectValues(_expectedFieldValues, objects.Objects);
			}
		}

		private static void ThenTheFieldsHaveCorrectValues(DataTable expected, List<RelativityObject> actualObjects)
		{
			List<string> fieldNames = expected.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
			var rows = expected.Rows.Cast<DataRow>().ToArray();

			foreach (var relativityObject in actualObjects)
			{
				var controlNumber =
					relativityObject.FieldValues.SingleOrDefault(x => x.Field.Name == GetFieldName(WellKnownFields.ControlNumber));
				Assert.That(controlNumber, Is.Not.Null);
				var actualDictionary =
					relativityObject.FieldValues.ToDictionary(x => x.Field.Name, y => y.Value.ToString());

				var expectedRow = rows.SingleOrDefault(x => x[WellKnownFields.ControlNumber].ToString() == controlNumber.Value.ToString());
				Assert.That(expectedRow, Is.Not.Null);
				ThenTheRowHasCorrectValues(fieldNames, expectedRow, actualDictionary);
			}
		}

		private static void ThenTheRowHasCorrectValues(List<string> columnNames, DataRow expected, Dictionary<string, string> actual)
		{
			foreach (string columnName in columnNames)
			{
				Assert.That(actual.ContainsKey(GetFieldName(columnName)), $"Actual values dictionary does not contains key: {GetFieldName(columnName)}");
				
				string actualValue = actual[GetFieldName(columnName)];
				string expectedValue = expected[columnName].ToString();

				if (bool.TryParse(actualValue, out var boolValue))
				{
					actualValue = boolValue ? "1" : "0";
				}

				Assert.AreEqual(expectedValue, actualValue, $"Incorrect value in {columnName} field");
			}
		}

		private static string GetFieldName(string columnName)
		{
			//Object manager returns Control Number with space as displayName
			return columnName == WellKnownFields.ControlNumber ? "Control Number" : columnName;
		}


		private async Task<SDK.ImportExport.V1.Models.NativeLoadInfo> CreateSampleNativeLoadInfoAsync(
			int numberOfArtifactsToCreate)
		{
			List<SDK.ImportExport.V1.Models.FieldInfo> fields = new List<SDK.ImportExport.V1.Models.FieldInfo>();
			fields.Add(_identifierField);
			foreach (var field in _customFieldsDictionary)
			{
				fields.Add(new SDK.ImportExport.V1.Models.FieldInfo()
				{
					ArtifactID = field.Value,
					Category = SDK.ImportExport.V1.Models.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = field.Key,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = CustomFieldLength,
					Type = FieldType.Varchar,
				});
			}

			this._expectedFieldValues = RandomHelper.GetFieldValues(fields, numberOfArtifactsToCreate);
			this._expectedFieldsNames = fields.Select(x => new FieldRef() { ArtifactID = x.ArtifactID }).ToList();
			
			string loadFileContent = GetLoadFileContent(_expectedFieldValues);

			return new SDK.ImportExport.V1.Models.NativeLoadInfo
			{
				AuditLevel = SDK.ImportExport.V1.Models.ImportAuditLevel.FullAudit,
				Billable = true,
				BulkLoadFileFieldDelimiter = FieldDelimiter,
				CodeFileName = await BcpFileHelper.CreateEmptyAsync(_workspace.ArtifactID).ConfigureAwait(false),
				DataFileName = await BcpFileHelper.CreateAsync(loadFileContent, _workspace.ArtifactID).ConfigureAwait(false),
				DataGridFileName = await BcpFileHelper.CreateEmptyAsync(_workspace.ArtifactID).ConfigureAwait(false),
				DisableUserSecurityCheck = false,
				ExecutionSource = SDK.ImportExport.V1.Models.ExecutionSource.ImportAPI,
				KeyFieldArtifactID = 1003667,
				LinkDataGridRecords = false,
				LoadImportedFullTextFromServer = false,
				MappedFields = fields.ToArray(),
				MoveDocumentsInAppendOverlayMode = false,
				ObjectFileName = await BcpFileHelper.CreateEmptyAsync(_workspace.ArtifactID).ConfigureAwait(false),
				OnBehalfOfUserToken = null,
				Overlay = OverwriteType.Append,
				OverlayArtifactID = -1,
				OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
				Range = null,
				Repository = _workspace.DefaultFileRepository.Name,
				RootFolderID = _rootFolderId,
				RunID = Guid.NewGuid().ToString().Replace('-', '_'),
				UploadFiles = false,
				UseBulkDataImport = true,
			};
		}

		private string GetLoadFileContent(DataTable fieldValues)
		{
			StringBuilder metadataBuilder = new StringBuilder();

			for (int i = 0; i < fieldValues.Rows.Count; i++)
			{
				string values = string.Join(FieldDelimiter, fieldValues.Rows[i].ItemArray.Select(item => item.ToString()));
				metadataBuilder
					.Append("0").Append(FieldDelimiter)                 //kCura_Import_ID
					.Append("0").Append(FieldDelimiter)                 //kCura_Import_Status
					.Append("0").Append(FieldDelimiter)                 //kCura_Import_IsNew
					.Append("0").Append(FieldDelimiter)                 //ArtifactID
					.Append(i).Append(FieldDelimiter)                   //kCura_Import_OriginalLineNumber
					.Append(FieldDelimiter)                             //kCura_Import_FileGuid
					.Append(FieldDelimiter)                             //kCura_Import_Filename
					.Append(FieldDelimiter)                             //kCura_Import_Location
					.Append(FieldDelimiter)                             //kCura_Import_OriginalFileLocation
					.Append("0").Append(FieldDelimiter)                 //kCura_Import_FileSize
					.Append(_rootFolderId).Append(FieldDelimiter)       //kCura_Import_ParentFolderID
					.Append(values).Append(FieldDelimiter)				//ControlNumber, field1, field2, field3 .......
					.Append(FieldDelimiter)                             //kCura_Import_ParentFolderPath
					.Append(FieldDelimiter)                             //kCura_Import_DataGridException
					.Append(FieldDelimiter)                             //kCura_Import_ErrorData
					.Append(Environment.NewLine);
			}

			return metadataBuilder.ToString();
		}
	}
}
