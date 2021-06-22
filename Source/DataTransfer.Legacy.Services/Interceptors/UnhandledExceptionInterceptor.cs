// <copyright file="UnhandledExceptionInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Catch all unhandled exceptions from intercepted method.
	/// </summary>
	public class UnhandledExceptionInterceptor : InterceptorBase
	{
		private readonly IAPILog _logger;

		public UnhandledExceptionInterceptor(IAPILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public override void Intercept(IInvocation invocation)
		{
			try
			{
				base.Intercept(invocation);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "There was an error during call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during call {invocation.Method.Name} - {e.Message}", e);
			}
		}

		/// <inheritdoc />
		public override async Task Continuation(Task task, IInvocation invocation)
		{
			try
			{
				await base.Continuation(task, invocation);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "There was an error during custom continuation of call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.Method.Name} - {e.Message}", e);
			}
		}

		/// <inheritdoc />
		public override async Task<T> Continuation<T>(Task<T> task, IInvocation invocation)
		{
			try
			{
				return await base.Continuation(task, invocation);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "There was an error during continuation of call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.Method.Name} - {e.Message}", e);
			}
		}
	}
}