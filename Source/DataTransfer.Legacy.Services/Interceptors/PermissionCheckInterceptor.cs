using System.Collections.Generic;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	using System;
	using Relativity.Core.Exception;
	using Relativity.Services.Exceptions;
	using Permission = Relativity.Core.Permission;

	public class PermissionCheckInterceptor : InterceptorBase
	{
		private readonly IServiceContextFactory _serviceContextFactory;
		private readonly IRelativityPermissionHelper _relativityPermissionHelper;

		public PermissionCheckInterceptor(IAPILog logger, IServiceContextFactory serviceContextFactory, IRelativityPermissionHelper relativityPermissionHelper) : base(logger)
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
			var importPermission = false;
			var exportPermission = false;

			try
			{
				var baseServiceContext = _serviceContextFactory.GetBaseServiceContext(workspaceID);
				importPermission =
					_relativityPermissionHelper.HasAdminOperationPermission(baseServiceContext,
						Permission.AllowDesktopClientImport);
				exportPermission =
					_relativityPermissionHelper.HasAdminOperationPermission(baseServiceContext,
						Permission.AllowDesktopClientExport);
			}
			catch (WorkspaceStatusException exception)
			{
				Logger.LogError(exception, "There was an error during call {type}.{method} - {message} because of workspace upgrading.", nameof(PermissionCheckInterceptor), nameof(EnsureUserHasPermissionsToUseWebApiReplacement), exception.Message);
				
				throw new NotFoundException($"Error during call {nameof(PermissionCheckInterceptor)}.{nameof(EnsureUserHasPermissionsToUseWebApiReplacement)}. {InterceptorHelper.BuildErrorMessageDetails(exception)}", exception);
			}

			if (!importPermission && !exportPermission)
			{
				throw new PermissionDeniedException("User does not have permissions to use WebAPI Kepler replacement");
			}
		}
	}
}
