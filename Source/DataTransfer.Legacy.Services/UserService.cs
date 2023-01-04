using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class UserService : BaseService, IUserService
	{
		private readonly ITraceGenerator _traceGenerator;

		public UserService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<DataSetWrapper> RetrieveAllAssignableInCaseAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.User.RetrieveAllAssignableInCase", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var manager = new UserManager();
				var result = manager.ExternalRetrieveAllAssignableInCase(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
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