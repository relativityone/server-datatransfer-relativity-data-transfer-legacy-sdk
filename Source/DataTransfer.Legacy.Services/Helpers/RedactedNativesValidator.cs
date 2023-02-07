using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;
using Relativity.DataTransfer.Legacy.Services.Toggles;
using Relativity.Services.Exceptions;
using Relativity.Toggles;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	internal class RedactedNativesValidator : IRedactedNativesValidator
	{
		private readonly IAPILog _logger;
		private readonly IToggleProvider _toggleProvider;
		private readonly Lazy<IProductionExternalService> _productionExternalServiceLazy;

		public RedactedNativesValidator(IAPILog logger, IToggleProvider toggleProvider, Func<IProductionExternalService> productionExternalServiceProvider)
		{
			_logger = logger;
			_toggleProvider = toggleProvider;
			_productionExternalServiceLazy = new Lazy<IProductionExternalService>(productionExternalServiceProvider);
		}

		public async Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(int workspaceID, int productionArtifactID, CancellationToken cancellationToken)
		{
			if (!await _toggleProvider.IsEnabledAsync<EnabledBlockingRedactedNativesExportForProduction>())
			{
				return;
			}

			bool hasRedactedNativesEnabled = false;
			try
			{
				hasRedactedNativesEnabled = await _productionExternalServiceLazy.Value.HasRedactedNativesEnabledAsync(workspaceID, productionArtifactID, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to determine if productions has native redactions. Allowing to export.");
			}

			if (hasRedactedNativesEnabled)
			{
				_logger.LogWarning("Unsupported production detected - it contains redacted natives");
				throw new NotFoundException(
					"Production cannot be exported because it contains native redactions. Please use \"Import\\Export\".");
			}
		}
	}
}
