using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public sealed class MethodRunnerWithPermissionCheck : IMethodRunner
	{
		private readonly IMethodRunner _methodRunner;
		private readonly IServiceContextFactory _serviceContextFactory;

		public MethodRunnerWithPermissionCheck(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory)
		{
			_methodRunner = methodRunner;
			_serviceContextFactory = serviceContextFactory;
		}

		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, [CallerMemberName] string callerMemberName = "")
		{
			if (workspaceId.HasValue)
			{
				EnsureUserHasPermissionsToUseWebApiReplacement(workspaceId.Value);
			}

			return await _methodRunner.ExecuteAsync(func, workspaceId, correlationId).ConfigureAwait(false);
		}

		private void EnsureUserHasPermissionsToUseWebApiReplacement(int workspaceID)
		{
			BaseServiceContext baseServiceContext = _serviceContextFactory.GetBaseServiceContext(workspaceID);
			bool importPermission = PermissionsHelper.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport);
			bool exportPermission = PermissionsHelper.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport);

			if (!importPermission && !exportPermission)
			{
				throw new PermissionDeniedException("User does not have permissions to use WebAPI Kepler replacement");
			}
		}
	}
}