using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.Productions.Services.Interfaces.V1.DTOs;
using Relativity.Productions.Services.V1;

namespace MassImport.NUnit.Integration.Helpers
{


	internal static class ProductionHelper
	{
		public static async Task<int> CreateProductionSet(IntegrationTestParameters parameters, int workspaceId)
		{
			var production = new Production
			{
				Details = new ProductionDetails
				{
					BrandingFontSize = 10,
					ScaleBrandingFont = false,
				},
				Name = "DataTransferLegacyTest"+ Guid.NewGuid(),
				Numbering = new DocumentLevelNumbering
				{
					NumberingType = NumberingType.DocumentLevel,
					BatesPrefix = "DTL_Test",
					BatesStartNumber = 0,
					NumberOfDigitsForDocumentNumbering = 4,
					IncludePageNumbers = false,
				},
			};

			using (var client = ServiceHelper.GetServiceProxy<IProductionManager>(parameters))
			{
				return await client.CreateSingleAsync(workspaceId, production).ConfigureAwait(false);
			}
		}

		public static int GetNumberOfImportedFiles(TestWorkspace testWorkspace, int productionSetArtifactId, int fileType)
		{
			using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					
					command.CommandText =
						$@"SELECT COUNT(*)
						 FROM [EDDS{testWorkspace.WorkspaceId}].[eddsdbo].[File]
						 WHERE [Identifier] LIKE '{productionSetArtifactId}_%' AND [Type] = {fileType}";

					var fileCount = Convert.ToInt32(command.ExecuteScalar());
					return fileCount;
				}
			}
		}

		public static int GetNumberOfProductionInformationEntries(TestWorkspace testWorkspace, int productionSetArtifactId, string postfix)
		{
			using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{

					command.CommandText =
						$@"SELECT COUNT(*)
						 FROM [EDDS{testWorkspace.WorkspaceId}].[eddsdbo].[ProductionInformation]
						 WHERE [Name] LIKE '{productionSetArtifactId}_%' AND {postfix}";

					var prodInformationCount = Convert.ToInt32(command.ExecuteScalar());
					return prodInformationCount;
				}
			}
		}
	}
}
