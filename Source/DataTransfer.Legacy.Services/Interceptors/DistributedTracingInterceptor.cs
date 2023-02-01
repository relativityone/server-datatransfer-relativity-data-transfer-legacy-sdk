// <copyright file="DistributedTracingInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Castle.DynamicProxy;
	using global::DataTransfer.Legacy.MassImport.RelEyeTelemetry;
	using Relativity.API;
	using Relativity.DataTransfer.Legacy.Services.Observability;

	public class DistributedTracingInterceptor : InterceptorBase
	{
		private const string CorrelationIDArgumentName = "correlationID";
		private const string WorkspaceIDArgumentName = "workspaceID";

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
		}

		/// <inheritdoc />
		public override void ExecuteBefore(IInvocation invocation)
		{
			try
			{
				System.Reflection.ParameterInfo[] parameters = invocation.Method.GetParameters();
				if (parameters.Length != invocation.Arguments.Length)
				{
					return;
				}

				for (var i = parameters.Length-1; i >= 0; i--)
				{
					var currentParameter = parameters[i];
					var invocationArgument = invocation.Arguments[i];

					if (currentParameter.Name == WorkspaceIDArgumentName)
					{
						currentActivity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, invocationArgument?.ToString());
						continue;
					}

					if (currentParameter.Name == CorrelationIDArgumentName)
					{
						if (ActivityContext.TryParse(invocationArgument?.ToString(), null, out var parentContext))
						{
							currentActivity = _traceGenerator.StartActivity($"{invocation.TargetType.Name}-{invocation.Method.Name}", ActivityKind.Server, parentContext);
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Cannot Start Activity: {invocation.TargetType.Name}-{invocation.Method.Name}");
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