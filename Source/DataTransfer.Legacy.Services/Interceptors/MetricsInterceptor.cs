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
		private readonly Func<IMetricsContext> _metricsContextFactory;
		private Stopwatch _stopwatch;

		/// <summary> 
		/// Initializes a new instance of the <see cref="MetricsInterceptor"/> class. 
		/// </summary> 
		/// <param name="logger"></param> 
		/// <param name="metricsContextFactory"></param> 
		public MetricsInterceptor(IAPILog logger, Func<IMetricsContext> metricsContextFactory) : base(logger)
		{
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
			var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
			var metrics = _metricsContextFactory.Invoke();
			
			var arguments = InterceptorHelper.GetFunctionArgumentsFrom(invocation);
			foreach (var argument in arguments)
			{
				metrics.PushProperty(argument.Key, argument.Value);
			}
			metrics.PushProperty($"TargetType", invocation.TargetType.Name);
			metrics.PushProperty($"Method", invocation.Method.Name);
			metrics.PushProperty($"ElapsedMilliseconds", elapsedMilliseconds);

			await metrics.Publish();

			using (Logger.LogContextPushProperty("CallDuration", elapsedMilliseconds))
			{
				Logger.LogInformation(
					"DataTransfer.Legacy service Kepler call {@controller} {@method} finished",
					invocation.TargetType.Name,
					invocation.Method.Name);
			}
		}
	}
}