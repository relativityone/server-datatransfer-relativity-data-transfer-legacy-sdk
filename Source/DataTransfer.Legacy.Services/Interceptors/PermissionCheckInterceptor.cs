using System;
using System.Collections.Generic;
using System.Linq;
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
		private const string WorkspaceIdArgumentName = "workspaceID";

		public PermissionCheckInterceptor(IServiceContextFactory serviceContextFactory, IRelativityPermissionHelper relativityPermissionHelper)
		{
			_serviceContextFactory = serviceContextFactory;
			_relativityPermissionHelper = relativityPermissionHelper;
		}

		public override void ExecuteBefore(IInvocation invocation)
		{
			var arguments = GetFunctionAttributes(invocation);

			if (TryGetValidWorkspaceIdPassedAsAnArgument(arguments, out var workspaceId))
			{
				EnsureUserHasPermissionsToUseWebApiReplacement(workspaceId);
			}
		}

		private static bool TryGetValidWorkspaceIdPassedAsAnArgument(Dictionary<string, string> arguments, out int workspaceId)
		{
			if (arguments.Keys.Any(x => x.Equals("workspaceid", StringComparison.InvariantCultureIgnoreCase)))
			{
				if (arguments.TryGetValue("workspaceid", out var outValue) && outValue != "null")
				{
					workspaceId = int.Parse(outValue);
					return true;
				}

				if (arguments.TryGetValue("workspaceId", out outValue) && outValue != "null")
				{
					workspaceId = int.Parse(outValue);
					return true;
				}

				if (arguments.TryGetValue("workspaceID", out outValue) && outValue != "null")
				{
					workspaceId = int.Parse(outValue);
					return true;
				}

				workspaceId = 0;
				return false;
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

		private static Dictionary<string, string> GetFunctionAttributes(IInvocation invocation)
		{
			var type = Type.GetType($"{invocation.TargetType.FullName}, {invocation.TargetType.Assembly.FullName}");
			var arguments = new Dictionary<string, string>();
			if (type == null)
			{
				return arguments;
			}

			var parameters = invocation.Method.GetParameters();
			if (parameters.Length != invocation.Arguments.Length)
			{
				return arguments;
			}

			for (var i = 0; i < parameters.Length; i++)
			{
				var value = invocation.Arguments[i]?.ToString() ?? "null";
				arguments.Add(parameters[i].Name, value);
			}

			return arguments;
		}
	}
}
