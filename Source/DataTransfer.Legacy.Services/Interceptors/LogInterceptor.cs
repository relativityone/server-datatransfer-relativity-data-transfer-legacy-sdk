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
		private readonly IAuthenticationMgr _authenticationMgr;
		private List<IDisposable> _contextPushPropertiesHandlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogInterceptor"/> class.
		/// </summary>
		/// <param name="logger">Logger.</param>
		/// <param name="authenticationMgr">Authentication Manager.</param>
		public LogInterceptor(IAPILog logger, IAuthenticationMgr authenticationMgr) : base(logger)
		{
			_authenticationMgr = authenticationMgr;
		}

		/// <inheritdoc />
		public override void ExecuteBefore(IInvocation invocation)
		{
			const string Controller = "Controller";
			const string EndpointCalled = "EndpointCalled";
			const string UserId = "UserId";
			const string WorkspaceUserId = "WorkspaceUserId";

			var arguments = InterceptorHelper.GetFunctionArgumentsFrom(invocation);
			var (userId, workspaceUserId) = GetUserId();

			_contextPushPropertiesHandlers = new List<IDisposable>
			{
				Logger.LogContextPushProperty(Controller, invocation.TargetType.Name),
				Logger.LogContextPushProperty(EndpointCalled, invocation.Method.Name),
				Logger.LogContextPushProperty(UserId, userId),
				Logger.LogContextPushProperty(WorkspaceUserId, workspaceUserId),
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

		private (int? userId, int? workspaceUserId) GetUserId()
		{
			try
			{
				return (_authenticationMgr.UserInfo?.ArtifactID, _authenticationMgr.UserInfo?.WorkspaceUserArtifactID);
			}
			catch (Exception ex)
			{
				Logger.LogWarning(ex, "Error occurred while getting user ID.");
				return (null, null);
			}
		}
	}
}