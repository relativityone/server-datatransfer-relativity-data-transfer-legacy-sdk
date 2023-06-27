using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;
using ExecutionSource = Relativity.MassImport.DTO.ExecutionSource;
using ImageLoadInfo = Relativity.MassImport.DTO.ImageLoadInfo;
using ImportAuditLevel = Relativity.MassImport.DTO.ImportAuditLevel;
using MassImportManager = Relativity.Core.Service.MassImportManager;
using Moq;
using Relativity.API;
using Relativity.Productions.Services.Private.V1;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class ImageImportTest : MassImportTestBase
	{
		private DataTable _expectedFieldValues;

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, (int)ArtifactType.Document).ConfigureAwait(false);
		}

		[Test]
		public async Task ShouldRunImageImport()
		{
			// Arrange
			const int expectedArtifactsCreated = 1;
			const int expectedArtifactsUpdated = 0;

			const bool inRepository = true;
			ImageLoadInfo imageLoadInfo = await this.CreateSampleImageLoadInfoAsync(expectedArtifactsCreated).ConfigureAwait(false);
			MassImportManager massImportManager = new MassImportManager(false, HelperMock.Object);

			// Act
			MassImportManagerBase.MassImportResults result =  massImportManager.RunImageImport(this.CoreContext.ChicagoContext, imageLoadInfo, inRepository);

			// Assert
			Validator.ThenTheFieldsHaveCorrectValues(this.TestWorkspace, this._expectedFieldValues);
			this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
		}

		private async Task<ImageLoadInfo> CreateSampleImageLoadInfoAsync(int numberOfArtifactsToCreate)
		{
			string repositoryPath = Path.Combine(this.TestWorkspace.DefaultFileRepository, "EDDS" + this.TestWorkspace.WorkspaceId);

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
				}
			};
			this._expectedFieldValues = RandomHelper.GetFieldValues(fields, numberOfArtifactsToCreate);
			string dataFileContent = GetBulkFileContent(this.TestWorkspace.WorkspaceId, this._expectedFieldValues);

			return new ImageLoadInfo
			{
				AuditLevel = ImportAuditLevel.FullAudit,
				Billable = true,
				DataGridFileName = await BcpFileHelper.CreateEmptyAsync(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false),
				DisableUserSecurityCheck = false,
				ExecutionSource = ExecutionSource.ImportAPI,
				KeyFieldArtifactID = 1003667,
				Overlay = OverwriteType.Append,
				OverlayArtifactID = -1,
				Repository = repositoryPath,
				RunID = Guid.NewGuid().ToString().Replace('-', '_'),
				UseBulkDataImport = true,
				BulkFileName = await BcpFileHelper.CreateAsync(this.TestParameters, this.TestWorkspace.WorkspaceId, dataFileContent).ConfigureAwait(false),
				HasPDF = false,
				UploadFullText = false,
				DestinationFolderArtifactID = 1003697,
			};
		}

		private string GetBulkFileContent(int workspaceID, DataTable fieldValues)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < fieldValues.Rows.Count; i++)
			{
				string controlNumber = string.Join(",", fieldValues.Rows[i].ItemArray.First().ToString());
				string anyFileSize = "81697";
				string postfix = $@",0,0,{anyFileSize},{this.TestWorkspace.DefaultFileRepository}EDDS{workspaceID}\RV_0b64d846-7187-4c70-bf4a-6aa606110b67\1cf413e8-296f-49ed-b936-658399b3c4a0,.\001\{controlNumber},,þþKþþ{Environment.NewLine}";
				var prefix = $"1,0,0,0,{i},";
				sb.Append(prefix);
				sb.Append(controlNumber);
				sb.Append(",");
				sb.Append(controlNumber);
				sb.Append(",");
				sb.Append(@"1cf413e8-296f-49ed-b936-658399b3c4a0,");
				sb.Append("FileNameThatShouldBeShorterThen200chars");
				sb.Append(postfix);
			}

			return sb.ToString();
		}
	}
}