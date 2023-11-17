using System;
using System.Globalization;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V2;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services.V2
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class RelativityService : BaseService, IRelativityService
	{
		public RelativityService(IServiceContextFactory serviceContextFactory) : base(serviceContextFactory) { }

		public Task<string> RetrieveCurrencySymbolAsync(string correlationID)
		{
			var result = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			return Task.FromResult(result);
		}
	}
}