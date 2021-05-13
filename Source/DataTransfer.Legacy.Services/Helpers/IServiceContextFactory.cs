using Relativity.Core;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public interface IServiceContextFactory
	{
		BaseServiceContext GetBaseServiceContext(int workspaceID);
	}
}