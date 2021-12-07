using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport;

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
			FieldInfo[] fields =
			{
				new FieldInfo
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
				},
				new FieldInfo
				{
					ArtifactID = 1034247,
					Category = FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.SupportedByViewer,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 0,
					Type = FieldTypeHelper.FieldType.Boolean,
				},
				new FieldInfo
				{
					ArtifactID = 1034248,
					Category = FieldCategory.AutoCreate,
					CodeTypeID = 0,
					DisplayName = WellKnownFields.RelativityNativeType,
					EnableDataGrid = false,
					FormatString = null,
					ImportBehavior = null,
					IsUnicodeEnabled = false,
					TextLength = 255,
					Type = FieldTypeHelper.FieldType.Varchar,
				},
			};

			this._expectedFieldValues = RandomHelper.GetFieldValues(fields, numberOfArtifactsToCreate);
			string dataFileContent = GetMetadata(fieldDelimiter, _expectedFieldValues);

			return new NativeLoadInfo
			{
				AuditLevel = ImportAuditLevel.FullAudit,
				Billable = true,
				BulkLoadFileFieldDelimiter = fieldDelimiter,
				CodeFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters).ConfigureAwait(false),
				DataFileName = await BcpFileHelper.CreateAsync(this.TestParameters, dataFileContent).ConfigureAwait(false),
				DataGridFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters).ConfigureAwait(false),
				DisableUserSecurityCheck = false,
				ExecutionSource = ExecutionSource.ImportAPI,
				KeyFieldArtifactID = 1003667,
				LinkDataGridRecords = false,
				LoadImportedFullTextFromServer = false,
				MappedFields = fields,
				MoveDocumentsInAppendOverlayMode = false,
				ObjectFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters).ConfigureAwait(false),
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

		private string GetMetadata(string fieldDelimiter, DataTable fieldValues)
		{
			StringBuilder metadataBuilder = new StringBuilder();
			string postfix = $"{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{Environment.NewLine}";

			for (int i = 0; i < fieldValues.Rows.Count; i++)
			{
				string prefix = $"0{fieldDelimiter}0{fieldDelimiter}0{fieldDelimiter}0{fieldDelimiter}{i}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}0{fieldDelimiter}1003697{fieldDelimiter}";
				metadataBuilder.Append(prefix);

				string values = string.Join(fieldDelimiter, fieldValues.Rows[i].ItemArray.Select(item => item.ToString()));
				metadataBuilder.Append(values);

				metadataBuilder.Append(postfix);
			}

			return metadataBuilder.ToString();
		}
	}
}