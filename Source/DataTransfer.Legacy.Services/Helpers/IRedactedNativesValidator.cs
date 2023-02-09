using System.Threading;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public interface IRedactedNativesValidator
	{
		Task VerifyThatProductionDoesNotHaveRedactedNativesEnabledAsync(int workspaceID, int productionArtifactID, CancellationToken cancellationToken);
	}
}