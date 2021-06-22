using Castle.Core;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	public interface IPermissionCheckInterceptorTestClass
	{
		void RunWithWorkspaceRelativityCase(int? workspaceID);
		void RunWithWorkspaceLowerCase(int? workspaceid);
		void RunWithWorkspaceCamelCase(int? workspaceId);
		void RunWithWorkspaceUnrecognizedCase(int? WorkspaceId);
		void Run();
	}

	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class PermissionCheckInterceptorTestClass : IPermissionCheckInterceptorTestClass
	{
		public void RunWithWorkspaceRelativityCase(int? workspaceID)
		{
		}

		public void RunWithWorkspaceLowerCase(int? workspaceid)
		{
		}

		public void RunWithWorkspaceCamelCase(int? workspaceId)
		{
		}

		public void RunWithWorkspaceUnrecognizedCase(int? WorkspaceId)
		{
		}

		public void Run()
		{
		}
	}
}