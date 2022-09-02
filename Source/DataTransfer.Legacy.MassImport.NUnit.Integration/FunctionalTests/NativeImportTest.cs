﻿using System;
using System.Data;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class NativeImportTest : MassImportTestBase
	{
		private DataTable _expectedFieldValues;

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
			string dataFileContent = GetMetadata(fieldDelimiter, _expectedFieldValues);

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
    }
}