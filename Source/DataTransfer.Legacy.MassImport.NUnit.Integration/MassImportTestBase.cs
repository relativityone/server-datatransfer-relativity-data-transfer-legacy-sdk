using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Data.AuditIngestion;
using Relativity.Kepler.Transport;
using Relativity.Productions.Services.Private.V1;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;
using BaseContext = Relativity.Core.BaseContext;
using Context = kCura.Data.RowDataGateway.Context;

namespace MassImport.NUnit.Integration
{
	public abstract class MassImportTestBase
	{
		public Mock<IHelper> HelperMock;
		private const int USER_ID = 9;
		private readonly Mock<ICoreContext> _coreContextMock = new Mock<ICoreContext>();

		protected int UserId => USER_ID;
		protected IntegrationTestParameters TestParameters { get; }
		protected TestWorkspace TestWorkspace { get; private set; }
		protected Context Context { get; private set; }
		protected ICoreContext CoreContext => this._coreContextMock.Object;

		protected MassImportTestBase()
		{
			this.TestParameters = AssemblySetup.TestParameters;
			HelperMock = new Mock<IHelper>();
			Mock<IServicesMgr> serviceManagerMock = new Mock<IServicesMgr>();
			serviceManagerMock.Setup(x => x.CreateProxy<IInternalProductionImportExportManager>(ExecutionIdentity.CurrentUser)).Returns(ServiceHelper.GetServiceProxy<IInternalProductionImportExportManager>(TestParameters));
			HelperMock.Setup(x => x.GetServicesManager()).Returns(serviceManagerMock.Object);
		}

		[OneTimeSetUp]
		public async Task OneTimeBaseSetupAsync()
		{
			TestWorkspace = await AssemblySetup.TestWorkspaceAsync.ConfigureAwait(false);
			SettingsHelper.SetDefaultSettings();
			this.SetupCoreContextMock();
			this.SetupConfigMock();

			const string pathToRAPFolder = @"\\bld-pkgs\Packages\Productions\master";
			string pathToDataTransferLegacyRap = GetPathToLatestRAPFrom(pathToRAPFolder);
			var libraryApplicationManager = ServiceHelper.GetServiceProxy < Relativity.Services.Interfaces.LibraryApplication.ILibraryApplicationManager>(TestParameters);
			
			var libraryApplicationService = ServiceHelper.GetServiceProxy<IApplicationInstallManager>(TestParameters);
			if (!string.IsNullOrEmpty(pathToDataTransferLegacyRap))
			{

				try
				{
					PackageDetailsResponse packageSetailsResponse;
					using (System.IO.Stream stream = System.IO.File.OpenRead(pathToDataTransferLegacyRap))
					{
						packageSetailsResponse = await libraryApplicationManager.UploadPackageAsync(-1, new KeplerStream(stream));
					}

					LibraryApplicationResponse libraryApplicationResponse =
						(await libraryApplicationManager.ReadAllAsync(-1)).FirstOrDefault(x =>
							x.Guids.FirstOrDefault() == packageSetailsResponse.ApplicationGUID);

					if (libraryApplicationResponse == null || IsNewerVersion(packageSetailsResponse.Version, libraryApplicationResponse.Version))
					{
						UpdateLibraryApplicationResponse updateLibraryApplicationResponse = await libraryApplicationManager.UpdateAsync(-1, packageSetailsResponse.PackageGUID, new UpdateLibraryApplicationRequest { CreateIfMissing = true });


						InstallApplicationAllRequest request = new InstallApplicationAllRequest
						{
							Mode = ApplicationInstallTargetOption.ForceInstall,
							UnlockApplications = false
						};
						var installationApplicationResponse = await libraryApplicationService.InstallApplicationAllAsync(-1, packageSetailsResponse.ApplicationGUID, request);
						var installationStatusResponse = await libraryApplicationService.GetStatusAsync(TestWorkspace.WorkspaceId, packageSetailsResponse.ApplicationGUID);
						List<InstallStatusCode> terminalStates = new List<InstallStatusCode>() { InstallStatusCode.Canceled, InstallStatusCode.Completed, InstallStatusCode.Failed };
						InstallStatusCode installationStatus = installationStatusResponse.InstallStatus.Code;
						while (!terminalStates.Contains(installationStatus))
						{
							await Task.Delay(new TimeSpan(0, 0, 0, 1));
							installationStatusResponse = await libraryApplicationService.GetStatusAsync(TestWorkspace.WorkspaceId, packageSetailsResponse.ApplicationGUID);
							installationStatus = installationStatusResponse.InstallStatus.Code;
						}
					}
				}
				catch (Exception ex)
				{
					string exception = $"An error occurred: {ex.Message}";
					Console.WriteLine(exception);
				}
			}
		}

		protected void ThenTheImportWasSuccessful(MassImportManagerBase.MassImportResults result, int expectedArtifactsCreated, int expectedArtifactsUpdated)
		{
			Assert.IsNull(result.ExceptionDetail, $"An error occurred when running import: {result.ExceptionDetail?.ExceptionMessage}");
			Assert.AreEqual(expectedArtifactsCreated, result.ArtifactsCreated, "Invalid number of created artifacts");
			Assert.AreEqual(expectedArtifactsUpdated, result.ArtifactsUpdated, "Invalid number of updated artifacts");
		}

		protected string GetMetadata(string fieldDelimiter, DataTable fieldValues, string[] folders)
		{
			StringBuilder metadataBuilder = new StringBuilder();
			string folderId = folders != null ? "-9" : "1003697";

			for (int i = 0; i < fieldValues.Rows.Count; i++)
			{
				string prefix = $"0{fieldDelimiter}0{fieldDelimiter}0{fieldDelimiter}0{fieldDelimiter}{i}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}0{fieldDelimiter}{folderId}{fieldDelimiter}";
				metadataBuilder.Append(prefix);

				string values = string.Join(fieldDelimiter, fieldValues.Rows[i].ItemArray.Select(item => item.ToString()));
				metadataBuilder.Append(values);

				string postfix = $"{fieldDelimiter}{GetFolderName(folders, i)}{fieldDelimiter}{fieldDelimiter}{fieldDelimiter}{Environment.NewLine}";
				metadataBuilder.Append(postfix);
			}

			return metadataBuilder.ToString();
		}

		private string GetFolderName(string[] folders, int index)
		{
			return folders != null ? folders[index] : "";
		}

		private void SetupCoreContextMock()
		{
			Context = new Context(this.TestWorkspace.ConnectionString);
			Mock<BaseContext> baseContextMock = new Mock<BaseContext>();
			Mock<IAuditRepository> auditRepositoryMock = new Mock<IAuditRepository>();
			auditRepositoryMock.Setup(x => x.BeginTransaction()).Callback(() => Context.BeginTransaction());

			baseContextMock.Setup(x => x.DBContext).Returns(Context);
			baseContextMock.Setup(x => x.BeginTransaction()).Callback(() => Context.BeginTransaction());
			baseContextMock.Setup(x => x.CommitTransaction()).Callback(() => Context.CommitTransaction());
			baseContextMock.Setup(x => x.RollbackTransaction()).Callback(() => Context.RollbackTransaction());
			baseContextMock.Setup(x => x.GetBcpSharePath()).Returns(this.TestParameters.BcpSharePath);
			baseContextMock.Setup(x => x.AppArtifactID).Returns(this.TestWorkspace.WorkspaceId);
			baseContextMock.Setup(x => x.UserID).Returns(USER_ID);
			baseContextMock.Setup(x => x.AclUserID).Returns(USER_ID);
			baseContextMock.Setup(x => x.RequestOrigination).Returns("TestRequest");
			baseContextMock.Protected().Setup<IAuditRepository>("AuditRepository").Returns(auditRepositoryMock.Object);
			baseContextMock.Setup(x => x.ChicagoContext).Returns(baseContextMock.Object);
			this._coreContextMock.Setup(x => x.ChicagoContext).Returns(baseContextMock.Object);
		}

		private void SetupConfigMock()
		{
			kCura.Data.RowDataGateway.Config.SetConnectionString(this.TestWorkspace.ConnectionString);
		}

		private string GetPathToLatestRAPFrom(string path)
		{
			const string rapName = "Relativity.Productions.rap";

			var dirInfo = new DirectoryInfo(path);
			if (dirInfo.Exists)
			{
				DirectoryInfo directoryInfo = (from f in dirInfo.GetDirectories() orderby f.LastWriteTime descending select f).FirstOrDefault();
				if (directoryInfo != null)
				{
					FileInfo file = (from f in directoryInfo.GetFiles(rapName) select f).FirstOrDefault();
					return file?.FullName;
				}
			}

			return null;
		}

		private bool IsNewerVersion(string uploadedApplicationVersion, string existingApplicationVersion)
		{
			var packageVersion = new Version(uploadedApplicationVersion);
			var applicationVersion = new Version(existingApplicationVersion);

			if (packageVersion.CompareTo(applicationVersion) > 0)
			{
				return true;
			}

			return false;
		}
	}
}