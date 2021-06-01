// <copyright file="MetricsInterceptor.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary> 
	/// Measure execution time of intercepted method and publish all gathered in the meantime metrics. Only Task[Response] and ValueResponse return type can be intercepted. 
	/// </summary> 
	public class MetricsInterceptor : InterceptorBase
	{
		private readonly IAPILog _logger;
		private readonly Func<IMetricsContext> _metricsContextFactory;
		private Stopwatch _stopwatch;

		/// <summary> 
		/// Initializes a new instance of the <see cref="MetricsInterceptor"/> class. 
		/// </summary> 
		/// <param name="logger"></param> 
		/// <param name="metricsContextFactory"></param> 
		public MetricsInterceptor(IAPILog logger, Func<IMetricsContext> metricsContextFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_metricsContextFactory = metricsContextFactory;
		}

		/// <inheritdoc /> 
		public override void ExecuteBefore(IInvocation invocation)
		{
			_stopwatch = Stopwatch.StartNew();
		}

		/// <inheritdoc /> 
		public override async Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			_stopwatch.Stop();

			_metricsContextFactory.Invoke().PushProperty(
				$"Action:{invocation.TargetType.Name}.{invocation.Method.Name}",
				_stopwatch.ElapsedMilliseconds);

			await _metricsContextFactory.Invoke().Publish();

			using (_logger.LogContextPushProperty("CallDuration", _stopwatch.ElapsedMilliseconds))
			{
				_logger.LogInformation(
					"DataTransfer.Legacy service Kepler call {@controller} {@method} finished",
					invocation.TargetType.Name,
					invocation.Method.Name);
			}
		}
	}
}