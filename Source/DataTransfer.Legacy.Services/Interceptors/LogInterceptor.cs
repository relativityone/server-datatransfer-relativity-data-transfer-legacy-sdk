// <copyright file="LogInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Read and log all useful information from intercepted method context.
	/// </summary>
	public class LogInterceptor : InterceptorBase
	{
		private List<IDisposable> _contextPushPropertiesHandlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogInterceptor"/> class.
		/// </summary>
		/// <param name="logger">Logger.</param>
		public LogInterceptor(IAPILog logger) : base(logger)
		{
		}

		/// <inheritdoc />
		public override void ExecuteBeforeInner(IInvocation invocation)
		{
			const string Controller = "Controller";
			const string EndpointCalled = "EndpointCalled";
			var arguments = InterceptorHelper.GetFunctionArgumentsFrom(invocation);

			_contextPushPropertiesHandlers = new List<IDisposable>
			{
				Logger.LogContextPushProperty(Controller, invocation.TargetType.Name),
				Logger.LogContextPushProperty(EndpointCalled, invocation.Method.Name)
			};

			_contextPushPropertiesHandlers.AddRange(arguments.Select(argument => Logger.LogContextPushProperty(argument.Key, argument.Value)));
		}

		/// <inheritdoc />
		public override Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			DisposeLoggerContext(_contextPushPropertiesHandlers);
			return Task.CompletedTask;
		}

		private static void DisposeLoggerContext(List<IDisposable> loggers)
		{
			foreach (var handler in loggers.Where(x => x != null))
			{
				handler.Dispose();
			}
		}
	}
}