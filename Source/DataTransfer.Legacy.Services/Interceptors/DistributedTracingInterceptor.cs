// <copyright file="DistributedTracingInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Castle.DynamicProxy;
	using Relativity.API;
	using Relativity.DataTransfer.Legacy.Services.Observability;

	public class DistributedTracingInterceptor : InterceptorBase
	{
		private readonly IAPILog _logger;
		private readonly ITraceGenerator _traceGenerator;
		private Activity currentActivity;
			
		/// <summary>
		/// Initializes a new instance of the <see cref="DistributedTracingInterceptor"/> class.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="traceGenerator"></param>
		public DistributedTracingInterceptor(IAPILog logger, ITraceGenerator traceGenerator) : base(logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		/// <inheritdoc />
		public override void ExecuteBefore(IInvocation invocation)
		{
			try
			{
				var parameters = invocation.Method.GetParameters();
				if (parameters.Length != invocation.Arguments.Length)
				{
					_logger.LogInformation($"ParentContext.TraceId: NONE");
					return;
				}

				for (var i = 0; i < parameters.Length; i++)
				{
					var currentParameter = parameters[i];
					var invocationArgument = invocation.Arguments[i];

					if (currentParameter.Name == "correlationID")
					{
						var parentContext = TraceHelper.DeserializeContext(invocationArgument?.ToString());
						_logger.LogInformation($"ParentContext.TraceId: {parentContext.TraceId}");
						currentActivity = _traceGenerator.StartActivity($"{invocation.TargetType.Name}-{invocation.Method.Name}", ActivityKind.Server, parentContext);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"ParentContext.TraceId: ERROR", ex);
			}			
		}

		/// <inheritdoc />
		public override Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			currentActivity?.Stop();
			return Task.CompletedTask;
		}
	}
}