using System;
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
	}
}
