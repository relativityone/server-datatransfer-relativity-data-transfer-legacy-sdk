using System.Linq;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.ObjectManagement;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace RAPTemplate.FunctionalTests.CI
{
    [TestFixture]
    [TestExecutionCategory.CI]
    public class Tests
    {
        [Test]
        public void ApiTest()
        {
            IWorkspaceService _workspaceService = RelativityFacade.Instance.Resolve<IWorkspaceService>();
            IObjectService _objectService = RelativityFacade.Instance.Resolve<IObjectService>();
            ILibraryApplicationService applicationService = RelativityFacade.Instance.Resolve<ILibraryApplicationService>();

            LibraryApplication rapTemplate = _objectService.GetAll<LibraryApplication>().Where(application => application.Name == "RAPTemplate").FirstOrDefault();

            int relativityStarterTemplateArtifactID = 1015024;

            Assert.IsTrue(applicationService.IsInstalledInWorkspace(relativityStarterTemplateArtifactID, rapTemplate.ArtifactID));
            Assert.That(rapTemplate.Version, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void SqlTest()
        {
            string libraryVersion = string.Empty;
            int? workspaceArtifactID = 0;

            string sqlServer = RelativityFacade.Instance.Config.RelativityInstance.SqlServer;
            string sqlUserName = RelativityFacade.Instance.Config.RelativityInstance.SqlUsername;
            string sqlPassword = RelativityFacade.Instance.Config.RelativityInstance.SqlPassword;

            using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection())
            {
                connection.ConnectionString = $"Data Source={sqlServer};Initial Catalog=EDDS;Persist Security Info=False;Packet Size=4096;Workstation ID=localhost;User Id={sqlUserName};Password={sqlPassword}";
                connection.Open();
                System.Data.SqlClient.SqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT [Version] FROM [LibraryApplication] WHERE [Name] = 'RAPTemplate'";
                libraryVersion = (string)command.ExecuteScalar();
                command.CommandText = "SELECT TOP 1 [ArtifactID] FROM [EDDS1015024].[EDDSDBO].[RelativityApplication] WHERE [Name] = 'RAPTemplate'";
                workspaceArtifactID = (int?)command.ExecuteScalar();
            }

            Assert.That(libraryVersion, Is.Not.Empty);
            Assert.That(workspaceArtifactID, Is.Not.Null);
        }
    }
}
