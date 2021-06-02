using Relativity.Core;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	public interface IRelativityPermissionHelper
	{
		bool HasAdminOperationPermission(ICoreContext coreContext, Permission permission);
	}
}