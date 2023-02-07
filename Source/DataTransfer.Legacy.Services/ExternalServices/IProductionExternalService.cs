using System.Threading;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal interface IProductionExternalService
	{
		Task<bool> HasRedactedNativesEnabledAsync(
			int workspaceID,
			int productionArtifactID,
			CancellationToken cancellationToken = default);
	}
}
