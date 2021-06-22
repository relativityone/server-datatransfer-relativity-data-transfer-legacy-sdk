using System;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Query;

namespace Relativity.DataTransfer.Legacy.Services
{
	public abstract class BaseService : IDisposable
	{
		private readonly IServiceContextFactory _serviceContextFactory;
		protected static int AdminWorkspace = -1;

		protected BaseService(IServiceContextFactory serviceContextFactory)
		{
			_serviceContextFactory = serviceContextFactory;
		}

		protected BaseServiceContext GetBaseServiceContext(int workspaceID)
		{
			return _serviceContextFactory.GetBaseServiceContext(workspaceID);
		}

		protected IPermissionsMatrix GetUserAclMatrix(int workspaceID)
		{
			return new UserPermissionsMatrix(GetBaseServiceContext(workspaceID));
		}

		public void Dispose()
		{
		}
	}
}