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
    [TestType.Installation]
    [TestLevel.L3]
    public class Tests
    {
	    [IdentifiedTest("A8096C19-D0E6-405A-BC64-1D286AA91AEB")]
	    [Description("Check that DataTransfer.Legacy is installed")]
	    public void ShouldGetApplicationAfterInstallation()
        {
            var objectService = RelativityFacade.Instance.Resolve<IObjectService>();

            var rap = objectService.GetAll<LibraryApplication>()
	            .FirstOrDefault(application => application.Name == "DataTransfer.Legacy");

            Assert.That(rap, Is.Not.Null);
            Assert.That(rap.Version, Is.Not.Null.Or.Empty);
        }

        [IdentifiedTest("A55B8078-9F57-4AAB-A3F9-B91BC91B7E8F")]
        [Description("Check that DataTransfer.Legacy is visible in database as application")]
        public void ShouldGetApplicationDataFromDatabase()
        {
            string libraryVersion;

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
            }

            Assert.That(libraryVersion, Is.Not.Empty);
        }
    }
}
