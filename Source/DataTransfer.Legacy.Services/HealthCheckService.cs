using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class HealthCheckService : BaseService, IHealthCheckService
	{
		private const string ApplicationName = "DataTransfer.Legacy";

		private readonly IEnumerable<IMetricsPublisher> _metricsPublishers;

		public HealthCheckService(IServiceContextFactory serviceContextFactory, IEnumerable<IMetricsPublisher> metricsPublishers) : base(serviceContextFactory)
		{
			_metricsPublishers = metricsPublishers;
		}

		public async Task<HealthCheckResult> HealthCheckAsync()
		{
			foreach (var publisher in _metricsPublishers)
			{
				await publisher.PublishHealthCheckResult(true, $"{ApplicationName} is Healthy").ConfigureAwait(false);
			}

			return new HealthCheckResult(true, $"{ApplicationName} is Healthy");
		}
	}
}