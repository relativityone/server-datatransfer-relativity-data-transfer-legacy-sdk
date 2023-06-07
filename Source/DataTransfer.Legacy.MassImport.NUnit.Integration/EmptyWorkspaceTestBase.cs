using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using NUnit.Framework;

namespace MassImport.NUnit.Integration
{
	public abstract class EmptyWorkspaceTestBase
	{
		private string _databaseName;

		protected BaseContext EddsdboContext { get; private set; }

		private static IntegrationTestParameters TestParameters => OneTimeSetup.TestParameters;

		private static string ConnectionString => $"Data Source={TestParameters.SqlInstanceName};" +
											 "Persist Security Info=False;" +
											 "User ID=sa;" +
											 $"Password={TestParameters.SqlEddsdboPassword};" +
											 "Packet Size=4096;";

		[OneTimeSetUp]
		public async Task OneTimeSetUpAsync()
		{
			string dateTime = DateTime.UtcNow.ToString(format: "yyyyMMddTHHmmssffff");
			_databaseName = $"MassImportTest{dateTime}";
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				await connection.OpenAsync().ConfigureAwait(false);

				var queriesToExecute = new[]
				{
					$"CREATE DATABASE {_databaseName};",
					$"USE {_databaseName};",
					"CREATE SCHEMA [Resource];",
					"CREATE SCHEMA [EDDSDBO];",
					"CREATE USER EDDSDBO FOR LOGIN EDDSDBO WITH DEFAULT_SCHEMA = [EDDSDBO];",
					"EXEC sp_addrolemember N'db_owner', N'EDDSDBO'"
				};
				foreach (var query in queriesToExecute)
				{
					SqlCommand command = new SqlCommand(query, connection);
					await command.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}

			EddsdboContext = new Context(
				TestParameters.SqlInstanceName,
				database: _databaseName,
				TestParameters.SqlEddsdboUserName,
				TestParameters.SqlEddsdboPassword);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDownAsync()
		{
			var useMasterDbQuery = new QueryInformation
			{ Statement = "USE master;" }; // we need to make sure that no-one is connected to the test's db.
			await EddsdboContext.ExecuteNonQueryAsync(useMasterDbQuery).ConfigureAwait(false);
			EddsdboContext.ReleaseConnection(); // it does not release connection properly, so query above is needed.

			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				await connection.OpenAsync().ConfigureAwait(false);
				string createDbQuery = $"DROP DATABASE {_databaseName};";
				SqlCommand command = new SqlCommand(createDbQuery, connection);
				await command.ExecuteNonQueryAsync().ConfigureAwait(false);
			}
		}
	}
}
