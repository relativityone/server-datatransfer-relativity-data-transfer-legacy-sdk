using System.Diagnostics.CodeAnalysis;
using Relativity.Core;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	[ExcludeFromCodeCoverage]
	public class RelativityPermissionHelper : IRelativityPermissionHelper
	{
		public bool HasAdminOperationPermission(ICoreContext coreContext, Permission permission)
		{
			return PermissionsHelper.HasAdminOperationPermission(coreContext, permission);
		}
	}
}