using System.Collections.Generic;
using Castle.DynamicProxy;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	public class PermissionCheckInterceptor : InterceptorBase
	{
		private readonly IServiceContextFactory _serviceContextFactory;
		private readonly IRelativityPermissionHelper _relativityPermissionHelper;

		public PermissionCheckInterceptor(IServiceContextFactory serviceContextFactory, 
			IRelativityPermissionHelper relativityPermissionHelper)
		{
			_serviceContextFactory = serviceContextFactory;
			_relativityPermissionHelper = relativityPermissionHelper;
		}

		public override void ExecuteBefore(IInvocation invocation)
		{
			var arguments = InterceptorHelper.GetFunctionArgumentsFrom(invocation);

			if (TryGetValidWorkspaceIdPassedAsAnArgument(arguments, out var workspaceId))
			{
				EnsureUserHasPermissionsToUseWebApiReplacement(workspaceId);
			}
		}

		private static bool TryGetValidWorkspaceIdPassedAsAnArgument(IReadOnlyDictionary<string, string> arguments, out int workspaceId)
		{
			if (arguments.TryGetValue("workspaceID", out var outValue) && outValue != "null")
			{
				workspaceId = int.Parse(outValue);
				return true;
			}

			workspaceId = 0;
			return false;
		}

		private void EnsureUserHasPermissionsToUseWebApiReplacement(int workspaceID)
		{
			var baseServiceContext = _serviceContextFactory.GetBaseServiceContext(workspaceID);
			var importPermission = _relativityPermissionHelper.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport);
			var exportPermission = _relativityPermissionHelper.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport);

			if (!importPermission && !exportPermission)
			{
				throw new PermissionDeniedException("User does not have permissions to use WebAPI Kepler replacement");
			}
		}
	}
}
