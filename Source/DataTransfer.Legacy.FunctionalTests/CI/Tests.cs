using System.Linq;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.ObjectManagement;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{
    [TestFixture]
    [TestExecutionCategory.CI]
    public class Tests
    {
        [Test]
        public void ApiTest()
        {
            var objectService = RelativityFacade.Instance.Resolve<IObjectService>();

            var rapTemplate = objectService.GetAll<LibraryApplication>()
	            .FirstOrDefault(application => application.Name == "DataTransfer.Legacy");

            Assert.That(rapTemplate, Is.Not.Null);
            Assert.That(rapTemplate.Version, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void SqlTest()
        {
            string libraryVersion;
            int? workspaceArtifactId;

            var sqlServer = RelativityFacade.Instance.Config.RelativityInstance.SqlServer;
            var sqlUserName = RelativityFacade.Instance.Config.RelativityInstance.SqlUsername;
            var sqlPassword = RelativityFacade.Instance.Config.RelativityInstance.SqlPassword;

            using (var connection = new System.Data.SqlClient.SqlConnection())
            {
                connection.ConnectionString = $"Data Source={sqlServer};Initial Catalog=EDDS;Persist Security Info=False;Packet Size=4096;Workstation ID=localhost;User Id={sqlUserName};Password={sqlPassword}";
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT [Version] FROM [LibraryApplication] WHERE [Name] = 'DataTransfer.Legacy.rap'";
                libraryVersion = (string)command.ExecuteScalar();
                command.CommandText = "SELECT TOP 1 [ArtifactID] FROM [EDDS1015024].[EDDSDBO].[RelativityApplication] WHERE [Name] = 'RAPTemplate'";
                workspaceArtifactId = (int?)command.ExecuteScalar();
            }

            Assert.That(libraryVersion, Is.Not.Empty);
            Assert.That(workspaceArtifactId, Is.Not.Null);
        }
    }
}
