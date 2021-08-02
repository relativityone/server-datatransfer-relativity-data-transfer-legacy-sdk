using System;
using System.Linq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{
    [TestFixture]
    [TestExecutionCategory.CI]
    [TestType.Installation]
    [TestLevel.L3]
    public class Tests
    {
	    private const string ApplicationName = "DataTransfer.Legacy";

        [IdentifiedTest("A8096C19-D0E6-405A-BC64-1D286AA91AEB")]
	    [Description("Check that DataTransfer.Legacy is installed")]
	    public void ShouldGetApplicationAfterInstallation()
        {
	        var keplerServiceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
	        using (var applicationManager = keplerServiceFactory.GetServiceProxy<ILibraryApplicationManager>())
	        {
		        var apps = applicationManager.ReadAllAsync(-1).GetAwaiter().GetResult();

			    var rap = apps.FirstOrDefault(x => x.Name.Equals(ApplicationName, StringComparison.Ordinal));
		        
			    Assert.NotNull(rap, $"{ApplicationName} is null");
		        Assert.AreNotEqual(string.Empty, rap.Version, $"{ApplicationName} version in library should be not be empty");
            }
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



        [IdentifiedTest("11DE3768-9288-4DBC-B988-473FD654287D")]
        [Description("Check that DataTransfer.Legacy health check is working")]
        public void ShouldRunHealthCheck()
        {
	        var keplerServiceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
	        using (var healthCheckService = keplerServiceFactory.GetServiceProxy<IHealthCheckService>())
	        {
		        var response = healthCheckService.HealthCheckAsync();
                Assert.That(response.Result.IsHealthy, Is.True);
                Assert.That(response.Result.Message, Is.EqualTo($"{ApplicationName} is Healthy"));
	        }
        }

    }
}
