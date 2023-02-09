using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;
using ExecutionSource = Relativity.MassImport.DTO.ExecutionSource;
using ImportAuditLevel = Relativity.MassImport.DTO.ImportAuditLevel;
using NativeLoadInfo = Relativity.MassImport.DTO.NativeLoadInfo;
using Moq;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class NativeImportTest : MassImportTestBase
	{
		private DataTable _expectedFieldValues;
		private string[] _expectedFolders;

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
			MassImportManager massImportManager = new MassImportManager(false, new Mock<IHelper>().Object);

			// Act
			MassImportManagerBase.MassImportResults result =  massImportManager.RunNativeImport(this.CoreContext, nativeLoadInfo, inRepository, includeExtractedTextEncoding);

			// Assert
			this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
			Validator.ThenTheFieldsHaveCorrectValues(this.TestWorkspace, this._expectedFieldValues);
			Validator.ThenTheFoldersHaveCorrectValues(this.TestWorkspace, this._expectedFolders);
		}

		private async Task<NativeLoadInfo> CreateSampleNativeLoadInfoAsync(int numberOfArtifactsToCreate)
		{
			string fieldDelimiter = "þþKþþ";
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
			string dataFileContent = GetMetadata(fieldDelimiter, _expectedFieldValues, _expectedFolders);

			return new NativeLoadInfo
			{
				AuditLevel = ImportAuditLevel.FullAudit,
				Billable = true,
				BulkLoadFileFieldDelimiter = fieldDelimiter,
				CodeFileName =  await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),

				DataFileName = await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, dataFileContent).ConfigureAwait(false),
				DataGridFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
				DisableUserSecurityCheck = false,
				ExecutionSource = ExecutionSource.ImportAPI,
				KeyFieldArtifactID = 1003667,
				LinkDataGridRecords = false,
				LoadImportedFullTextFromServer = false,
				MappedFields = fields,
				MoveDocumentsInAppendOverlayMode = false,
				ObjectFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
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
	}
}