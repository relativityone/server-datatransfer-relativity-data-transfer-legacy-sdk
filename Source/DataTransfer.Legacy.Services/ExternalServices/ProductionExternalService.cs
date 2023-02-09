using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Productions.Services.V2;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal class ProductionExternalService : IProductionExternalService
	{
		private readonly IAPILog _logger;
		private readonly Lazy<IAsyncPolicy<Productions.Services.Interfaces.V2.DTOs.Production>> _productionRetryPolicyLazy;

		private readonly IProductionManager _productionManager;

		public ProductionExternalService(
			IAPILog logger,
			IKeplerRetryPolicyFactory retryPolicyFactory,
			IProductionManager productionManager)
		{
			_logger = logger;

			_productionRetryPolicyLazy = new Lazy<IAsyncPolicy<Productions.Services.Interfaces.V2.DTOs.Production>>(
				retryPolicyFactory.CreateRetryPolicy<Productions.Services.Interfaces.V2.DTOs.Production>);

			_productionManager = productionManager;
		}

		public async Task<bool> HasRedactedNativesEnabledAsync(
			int workspaceID,
			int productionArtifactID,
			CancellationToken cancellationToken = default)
		{
			Productions.Services.Interfaces.V2.DTOs.Production production = await ReadSingleProductionAsync(workspaceID, productionArtifactID);
			var dataSourcesWithRedactedNativesCount = production.DataSources.Count(x => x.BurnNativeRedactions);

			if (dataSourcesWithRedactedNativesCount > 0)
			{
				_logger.LogInformation(
					"Production has {totalCount} data sources, {redactedCount} out of them has redacted natives",
					production.DataSources.Count,
					dataSourcesWithRedactedNativesCount);
			}

			return dataSourcesWithRedactedNativesCount > 0;
		}

		private async Task<Productions.Services.Interfaces.V2.DTOs.Production> ReadSingleProductionAsync(int workspaceID, int productionID)
		{
			return await _productionRetryPolicyLazy.Value.ExecuteAsync(() =>
				_productionManager.ReadSingleAsync(workspaceID, productionID));
		}
	}
}
