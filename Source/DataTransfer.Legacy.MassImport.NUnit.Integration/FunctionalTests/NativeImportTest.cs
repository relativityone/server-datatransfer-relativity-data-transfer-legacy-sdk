using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataTransfer.Legacy.MassImport.NUnit.Integration.Helpers;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;
using ExecutionSource = Relativity.MassImport.DTO.ExecutionSource;
using ImportAuditLevel = Relativity.MassImport.DTO.ImportAuditLevel;
using NativeLoadInfo = Relativity.MassImport.DTO.NativeLoadInfo;
using Relativity.MassImport.Api;
using DataTable = System.Data.DataTable;
using MassImportManager = Relativity.Core.Service.MassImportManager;
using FieldHelper = MassImport.NUnit.Integration.Helpers.FieldHelper;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class NativeImportTest : MassImportTestBase
	{
		private DataTable _expectedFieldValues;
		private string[] _expectedFolders;
		private string _fieldDelimiter = "þþKþþ";

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, (int)ArtifactType.Document).ConfigureAwait(false);
		}

		[Test]
		public async Task ShouldRunNativeImport()
		{
			// Arrange
			const int expectedArtifactsCreated = 5;
			const int expectedArtifactsUpdated = 0;

			const bool inRepository = true;
			const bool includeExtractedTextEncoding = false;
			NativeLoadInfo nativeLoadInfo = await this.CreateSampleNativeLoadInfoAsync(expectedArtifactsCreated).ConfigureAwait(false);
			MassImportManager massImportManager = new MassImportManager();

			// Act
			MassImportManagerBase.MassImportResults result =  massImportManager.RunNativeImport(this.CoreContext, nativeLoadInfo, inRepository, includeExtractedTextEncoding);

			// Assert
			this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
			Validator.ThenTheFieldsHaveCorrectValues(this.TestWorkspace, this._expectedFieldValues);
			Validator.ThenTheFoldersHaveCorrectValues(this.TestWorkspace, this._expectedFolders);
		}

        [Test]
        public async Task ShouldRunNativeImportAndNotExecuteSqlInjectionWhenMappedMultiObjectFieldWhichNameIsMaliciouslyCrafted()
        {
            // Test for REL-878172: SQLi vulnerability in Import if a field display name is maliciously crafted

            // Arrange
            string parentObjectName = "Alpha";
            string childObjectName = "Beta";
            int parentObjectArtifactTypeId = await RdoHelper.CreateObjectTypeAsync(TestParameters, TestWorkspace, parentObjectName).ConfigureAwait(false); ;
            int childObjectArtifactTypeId = await RdoHelper.CreateObjectTypeAsync(TestParameters, TestWorkspace, childObjectName, parentObjectArtifactTypeId).ConfigureAwait(false);

            string multiObjectFieldName = "AB '; CREATE TABLE Derp (ID int) --";

            MassImportField multiobjectField = await FieldHelper.CreateMultiObjectField(
                TestParameters,
                TestWorkspace,
                fieldName: multiObjectFieldName,
                destinationRdoArtifactTypeId: WellKnownFields.DocumentArtifactTypeId,
                associativeRdoArtifactTypeId: childObjectArtifactTypeId).ConfigureAwait(false);

            const int numberOfImportArtifacts = 5;
            const bool inRepository = true;
            const bool includeExtractedTextEncoding = false;

            NativeLoadInfo nativeLoadInfo = await this.CreateSampleNativeLoadInfoForMultiObjectAsync(multiobjectField, numberOfImportArtifacts).ConfigureAwait(false);
            MassImportManager massImportManager = new MassImportManager(false);

            // Act
            MassImportManagerBase.MassImportResults result = massImportManager.RunNativeImport(this.CoreContext, nativeLoadInfo, inRepository, includeExtractedTextEncoding);

            // Assert
            this.ThenTheImportWasSuccessful(result, 0, 0);
            Validator.ThenTableIsNotCreated(this.TestWorkspace, "Derp");

            long importStatus = (long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsChild;
            string[] expectedResult = new string[]
            {
                $"ControlNumber_0||{importStatus}||{multiObjectFieldName}",
                $"ControlNumber_1||{importStatus}||{multiObjectFieldName}",
                $"ControlNumber_2||{importStatus}||{multiObjectFieldName}",
                $"ControlNumber_3||{importStatus}||{multiObjectFieldName}",
                $"ControlNumber_4||{importStatus}||{multiObjectFieldName}",
            };
            Validator.ThenImportStatusAndErrorDataIsSetInNativeTempTable(this.TestWorkspace, result.RunID, expectedResult);
        }

        private async Task<NativeLoadInfo> CreateSampleNativeLoadInfoAsync(int numberOfArtifactsToCreate)
		{
			Relativity.FieldInfo[] fields =
			{
				new Relativity.FieldInfo
				{
					ArtifactID = 1003667,
					Category = Relativity.FieldCategory.Identifier,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.ControlNumber,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = true,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Varchar,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = 1034247,
					Category = Relativity.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.SupportedByViewer,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 0,
					Type = Relativity.FieldTypeHelper.FieldType.Boolean,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = 1034248,
					Category = Relativity.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.RelativityNativeType,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Varchar,
				},
			};

			this._expectedFieldValues = RandomHelper.GetFieldValues(fields, numberOfArtifactsToCreate);
			this._expectedFolders = GetFolders(numberOfArtifactsToCreate);
			string dataFileContent = GetMetadata(_fieldDelimiter, _expectedFieldValues, _expectedFolders);

			return await CreateLoadFileInfo(_fieldDelimiter, dataFileContent, string.Empty, fields);
		}

		private async Task<NativeLoadInfo> CreateSampleNativeLoadInfoForMultiObjectAsync(MassImportField multiObjectField, int numberOfArtifactsToCreate)
		{
			Relativity.FieldInfo[] fields =
			{
				new Relativity.FieldInfo
				{
					ArtifactID = 1003667,
					Category = Relativity.FieldCategory.Identifier,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.ControlNumber,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = true,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Varchar,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = 1034247,
					Category = Relativity.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.SupportedByViewer,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 0,
					Type = Relativity.FieldTypeHelper.FieldType.Boolean,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = 1034248,
					Category = Relativity.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.RelativityNativeType,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Varchar,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = 1034248,
					Category = Relativity.FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.HasNative,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Varchar,
				},
				new Relativity.FieldInfo
				{
					ArtifactID = multiObjectField.ArtifactID,
					Category = Relativity.FieldCategory.Generic,
					CodeTypeID = 0,
					DisplayName = multiObjectField.DisplayName,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 255,
					Type = Relativity.FieldTypeHelper.FieldType.Objects,
				},
			};

			this._expectedFieldValues = GetExpectedFieldsValues(fields);
			this._expectedFolders = null;

			var objectsFile = GetObjectFileContent(this._expectedFieldValues, multiObjectField.ArtifactID, multiObjectField.AssociativeArtifactTypeID);
			string objectsFileContent = ObjectHelper.GetObjectsFile(_fieldDelimiter, objectsFile);

			string dataFileContent = GetMetadata(_fieldDelimiter, _expectedFieldValues, _expectedFolders);

			return await CreateLoadFileInfo(_fieldDelimiter, dataFileContent, objectsFileContent, fields);
		}

		private async Task<NativeLoadInfo> CreateLoadFileInfo(string fieldDelimiter, string dataFileContent, string objectsFileContent, Relativity.FieldInfo[] fields)
		{
			string objectFileName = string.IsNullOrEmpty(objectsFileContent)
				? await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false)
				: await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, objectsFileContent).ConfigureAwait(false);

			return new NativeLoadInfo
			{
				AuditLevel = ImportAuditLevel.FullAudit,
				Billable = true,
				BulkLoadFileFieldDelimiter = fieldDelimiter,
				CodeFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),

				DataFileName = await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, dataFileContent).ConfigureAwait(false),
				DataGridFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
				DisableUserSecurityCheck = false,
				ExecutionSource = ExecutionSource.ImportAPI,
				KeyFieldArtifactID = 1003667,
				LinkDataGridRecords = false,
				LoadImportedFullTextFromServer = false,
				MappedFields = fields,
				MoveDocumentsInAppendOverlayMode = false,
				ObjectFileName = objectFileName,
				OnBehalfOfUserToken = null,
				Overlay = OverwriteType.Append,
				OverlayArtifactID = -1,
				OverlayBehavior = OverlayBehavior.UseRelativityDefaults,
				Range = null,
				Repository = this.TestWorkspace.DefaultFileRepository,
				RootFolderID = 1003697,
				RunID = Guid.NewGuid().ToString().Replace('-', '_'),
				UploadFiles = false,
				UseBulkDataImport = true,
			};
		}

		private string[] GetFolders(int numberOfArtifactsToCreate)
		{
			var folders = new List<string>();
			for (int i = 0; i < numberOfArtifactsToCreate; i++)
			{
				folders.Add($"Folder{i}");
			}
			return folders.ToArray();
		}

		private DataTable GetExpectedFieldsValues(Relativity.FieldInfo[] fields)
		{
			DataTable expectedValues = new DataTable();
			DataColumn[] columns = new DataColumn[fields.Length];

			for (int i = 0; i < fields.Length; i++)
			{
				columns[i] = new DataColumn(fields[i].DisplayName);
			}

			expectedValues.Columns.AddRange(columns);

			for (int i = 0; i < fields.Length; i++)
			{
				expectedValues.Rows.Add($"ControlNumber_{i}", 0, "", 0, $"Beta_{i}");
			}

			return expectedValues;
		}

		private ObjectHelper.ObjectStagingTableRow[] GetObjectFileContent(DataTable expectedFieldsValues, int fieldId, int objectTypeId)
		{
			var objectsFile = new ObjectHelper.ObjectStagingTableRow[expectedFieldsValues.Rows.Count];

			for (int i = 0; i < expectedFieldsValues.Rows.Count; i++)
			{
				var row = _expectedFieldValues.Rows[i];
				string controlNumber = row[0].ToString();
				string multiObjectValue = row[4].ToString();

				objectsFile[i] = new ObjectHelper.ObjectStagingTableRow
				{
					PrimaryObjectName = controlNumber,
					ObjectTypeID = objectTypeId,
					FieldID = fieldId,
					SecondaryObjectName = multiObjectValue
				};
			}

			return objectsFile;
		}
	}
}