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
using Relativity.Services.Objects.DataContracts;
using ExecutionSource = Relativity.MassImport.DTO.ExecutionSource;
using ImageLoadInfo = Relativity.MassImport.DTO.ImageLoadInfo;
using ImportAuditLevel = Relativity.MassImport.DTO.ImportAuditLevel;
using MassImportManager = Relativity.Core.Service.MassImportManager;

namespace MassImport.NUnit.Integration.FunctionalTests
{

	[TestFixture]
	public class ProductionImportTest : MassImportTestBase
	{
		private DataTable _expectedFieldValues;
		private string ConrolNumber;

		[TearDown]
		public async Task TearDownAsync()
		{
			await RdoHelper.DeleteAllObjectsByTypeAsync(TestParameters, TestWorkspace, (int)ArtifactType.Document).ConfigureAwait(false);
		}
		[TestCase(false)]
		[TestCase(true)]
		public async Task ShouldRunProductionImport(bool hasPDF)
		{
			// Arrange
			const int expectedArtifactsCreated = 1;
			const int expectedArtifactsUpdated = 0;

			const bool inRepository = true;
			var imageLoadInfo = await this.CreateSampleImageLoadInfoAsync(expectedArtifactsCreated, hasPDF).ConfigureAwait(false);
			

			MassImportManager massImportManager = new MassImportManager();
			var productionSetArtifactId = await ProductionHelper.CreateProductionSet(this.TestParameters, this.TestWorkspace.WorkspaceId).ConfigureAwait(false);

			// Act
			MassImportManagerBase.MassImportResults result = massImportManager.RunProductionImageImport(this.CoreContext.ChicagoContext, imageLoadInfo, productionSetArtifactId, inRepository);

			// Assert
			var document = await GetDocumentByControlNumber(ConrolNumber).ConfigureAwait(false);

			var hasImagesField = ChoiceHelper.GetChoiceField(document, WellKnownFields.HasImages);

			Assert.That(hasImagesField.Name, Is.EqualTo("No"));

			Validator.ThenTheFieldsHaveCorrectValues(this.TestWorkspace, this._expectedFieldValues);
			this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
			Assert.AreEqual(expectedArtifactsUpdated, 0);
		}

		private async Task<ImageLoadInfo> CreateSampleImageLoadInfoAsync(int numberOfArtifactsToCreate, bool hasPDF)
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
			this._expectedFieldValues = this._expectedFieldValues ?? RandomHelper.GetFieldValues(fields, numberOfArtifactsToCreate);
			string dataFileContent = GetBulkFileContent(this.TestWorkspace.WorkspaceId, this._expectedFieldValues);

			return new ImageLoadInfo
			{
				AuditLevel = ImportAuditLevel.NoAudit,
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
				HasPDF = hasPDF,
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

				ConrolNumber = controlNumber;
			}

			return sb.ToString();
		}

		private async Task<RelativityObject> GetDocumentByControlNumber(string controlNumber)
		{
			string[] fieldsToValidate = {
				WellKnownFields.ArtifactId,
				WellKnownFields.ControlNumber,
				WellKnownFields.HasImages};

			var documents = await RdoHelper.QueryDocuments(TestParameters, TestWorkspace, fieldsToValidate)
				.ConfigureAwait(false);

			return documents.First(x => x.FieldValues[1].Value.ToString() == controlNumber);
		}
	}
}