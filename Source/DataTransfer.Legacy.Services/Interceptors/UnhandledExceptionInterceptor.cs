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
		public UnhandledExceptionInterceptor(IAPILog logger) : base(logger)
		{
		}

		/// <inheritdoc />
		public override void Intercept(IInvocation invocation)
		{
			try
			{
				base.Intercept(invocation);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during call {method} - {message}", invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during call {invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}

		/// <inheritdoc />
		public override async Task Continuation(Task task, IInvocation invocation)
		{
			try
			{
				await base.Continuation(task, invocation);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during custom continuation of call {method} - {message}", invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during custom continuation of call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}

		/// <inheritdoc />
		public override async Task<T> Continuation<T>(Task<T> task, IInvocation invocation)
		{
			try
			{
				return await base.Continuation(task, invocation);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during continuation of call {method} - {message}", invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during continuation of call {method} - {message}", invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}
	}
}