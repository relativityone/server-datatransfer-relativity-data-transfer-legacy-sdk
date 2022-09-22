using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using NUnit.Framework;
using Relativity.Core.Service;
using Relativity.MassImport.DTO;

namespace MassImport.NUnit.Integration.FunctionalTests
{
	[TestFixture]
	public class ImageImportTest : MassImportTestBase
	{
		[Test]
		public async Task ShouldRunImageImport()
		{
			// Arrange
			const int expectedArtifactsCreated = 1;
			const int expectedArtifactsUpdated = 0;

			const bool inRepository = true;
			ImageLoadInfo imageLoadInfo = await this.CreateSampleImageLoadInfoAsync(expectedArtifactsCreated).ConfigureAwait(false);
			MassImportManager massImportManager = new MassImportManager();

			// Act
			MassImportManagerBase.MassImportResults result =  massImportManager.RunImageImport(this.CoreContext.ChicagoContext, imageLoadInfo, inRepository);

			// Assert
			this.ThenTheImportWasSuccessful(result, expectedArtifactsCreated, expectedArtifactsUpdated);
		}

		private async Task<ImageLoadInfo> CreateSampleImageLoadInfoAsync(int numberOfArtifactsToCreate)
		{
			string dataFileContent = GetBulkFileContent(this.TestWorkspace.WorkspaceId);
			string repositoryPath = Path.Combine(this.TestWorkspace.DefaultFileRepository, "EDDS" + this.TestWorkspace.WorkspaceId);
			
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

		private string GetBulkFileContent(int workspaceID)
		{
			const string controlNumber = @"MassImportImageTest_01";
			var sb = new StringBuilder();
			sb.Append(@"1,0,0,0,1,");
			sb.Append(controlNumber);
			sb.Append(",");
			sb.Append(controlNumber);
			sb.Append(",");
			sb.Append(@"1cf413e8-296f-49ed-b936-658399b3c4a0,");
			sb.Append(controlNumber);
			sb.Append(",0,0,81697,");
			sb.Append(this.TestWorkspace.DefaultFileRepository);
			sb.Append("EDDS" + workspaceID);
			sb.Append(@"\RV_0b64d846-7187-4c70-bf4a-6aa606110b67\1cf413e8-296f-49ed-b936-658399b3c4a0,.\001\");
			sb.Append(controlNumber);
			sb.Append(",,-1,þþKþþ");
			return sb.ToString();
		}
	}
}