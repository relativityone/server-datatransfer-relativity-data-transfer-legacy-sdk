using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassImport.NUnit.Integration.Helpers;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Core.Service;
using BaseContext = Relativity.Core.BaseContext;
using Context = kCura.Data.RowDataGateway.Context;

namespace MassImport.NUnit.Integration
{
	public abstract class MassImportTestBase
	{
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
		}

		[OneTimeSetUp]
		public async Task OneTimeBaseSetupAsync()
		{
			TestWorkspace = await AssemblySetup.TestWorkspaceAsync.ConfigureAwait(false);
			SettingsHelper.SetDefaultSettings();
			this.SetupCoreContextMock();
			this.SetupConfigMock();
		}

		protected void ThenTheImportWasSuccessful(MassImportManagerBase.MassImportResults result, int expectedArtifactsCreated, int expectedArtifactsUpdated)
		{
			Assert.IsNull(result.ExceptionDetail, $"An error occurred when running import: {result.ExceptionDetail?.ExceptionMessage}");
			Assert.AreEqual(expectedArtifactsCreated, result.ArtifactsCreated, "Invalid number of created artifacts");
			Assert.AreEqual(expectedArtifactsUpdated, result.ArtifactsUpdated, "Invalid number of updated artifacts");
		}

        protected string GetMetadata(string fieldDelimiter, DataTable fieldValues)
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

		private void SetupCoreContextMock()
		{
			Context = new Context(this.TestWorkspace.ConnectionString);
			Mock<BaseContext> baseContextMock = new Mock<BaseContext>();
			baseContextMock.Setup(x => x.DBContext).Returns(Context);
			baseContextMock.Setup(x => x.BeginTransaction()).Callback(() => Context.BeginTransaction());
			baseContextMock.Setup(x => x.CommitTransaction()).Callback(() => Context.CommitTransaction());
			baseContextMock.Setup(x => x.RollbackTransaction()).Callback(() => Context.RollbackTransaction());
			baseContextMock.Setup(x => x.GetBcpSharePath()).Returns(this.TestParameters.BcpSharePath);
			baseContextMock.Setup(x => x.AppArtifactID).Returns(this.TestWorkspace.WorkspaceId);
			baseContextMock.Setup(x => x.UserID).Returns(USER_ID);
			baseContextMock.Setup(x => x.AclUserID).Returns(USER_ID);
			baseContextMock.Setup(x => x.RequestOrigination).Returns("TestRequest");

			baseContextMock.Setup(x => x.ChicagoContext).Returns(baseContextMock.Object);
			this._coreContextMock.Setup(x => x.ChicagoContext).Returns(baseContextMock.Object);
		}

		private void SetupConfigMock()
		{
			kCura.Data.RowDataGateway.Config.SetConnectionString(this.TestWorkspace.ConnectionString);
		}
	}
}