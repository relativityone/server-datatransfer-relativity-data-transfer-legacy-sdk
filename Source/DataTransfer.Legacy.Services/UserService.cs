using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	public class UserService : BaseService, IUserService
	{
		public UserService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
		}

		public Task<DataSetWrapper> RetrieveAllAssignableInCaseAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				UserManager manager = new UserManager();
				return manager.ExternalRetrieveAllAssignableInCase(GetBaseServiceContext(workspaceID));
			}, workspaceID, correlationID);
		}

		public Task LogoutAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task ClearCookiesBeforeLoginAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<bool> LoggedInAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<bool> LoginAsync(string emailAddress, string password, string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}

		public Task<string> GenerateDistributedAuthenticationTokenAsync(string correlationID)
		{
			throw new NotSupportedException("This should not be used when using Kepler endpoints");
		}
	}
}