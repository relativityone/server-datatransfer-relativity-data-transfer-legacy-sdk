using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using Relativity.Constant;
using Relativity.Core;
using Relativity.Core.Authentication;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public class ServiceContextFactory : IServiceContextFactory
	{
		public BaseServiceContext GetBaseServiceContext(int workspaceID)
		{
			BaseServiceContext baseServiceContext;

			if (IsUpgradeLogin)
			{
				BaseServiceContext context = ClaimsPrincipal.Current.GetNewAPIServiceContext(-1);
				baseServiceContext = new ServiceContext(context.Identity, context.RequestOrigination, workspaceID, true, false);
			}
			else
			{
				baseServiceContext = ClaimsPrincipal.Current.GetNewAPIServiceContext(workspaceID);
			}

			return baseServiceContext;
		}

		private static bool IsUpgradeLogin => CallContext.LogicalGetData(HttpHeaders.UPGRADE_HEADER_NAME) != null;
	}
}