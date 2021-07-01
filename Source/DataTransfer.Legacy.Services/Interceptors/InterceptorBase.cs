﻿// <copyright file="InterceptorBase.cs" company="Relativity ODA LLC">
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
	/// Base class for all interceptors.
	/// </summary>
	public abstract class InterceptorBase : IInterceptor
	{
		protected readonly IAPILog Logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="InterceptorBase"/> class.
		/// </summary>
		/// <param name="logger">Logger.</param>
		protected InterceptorBase(IAPILog logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Wrap intercepted method with custom actions.
		/// </summary>
		/// <param name="invocation"></param>
		public virtual void Intercept(IInvocation invocation)
		{
			this.SafeExecuteBefore(invocation);

			invocation.Proceed();

			Type type = invocation.ReturnValue?.GetType();

			if (type != null && (type.IsSubclassOf(typeof(Task)) || type == typeof(Task)))
			{
				invocation.ReturnValue = Continuation((dynamic)invocation.ReturnValue, invocation);
			}
			else
			{
				// synchronous method cannot be changed to asynchronous
				this.SafeExecuteAfter(invocation, invocation.ReturnValue).Wait();
			}
		}

		/// <summary>
		/// Custom action executed before intercepted method.
		/// </summary>
		/// <param name="invocation"></param>
		private void SafeExecuteBefore(IInvocation invocation)
		{
			SafeExecute(() => ExecuteBefore(invocation));
		}

		/// <summary>
		/// Custom action executed before intercepted method.
		/// </summary>
		/// <param name="invocation"></param>
		public virtual void ExecuteBefore(IInvocation invocation)
		{
		}

		/// <summary>
		/// Custom action executed after intercepted method return value.
		/// </summary>
		/// <param name="invocation"></param>
		/// <param name="returnValue"></param>
		/// /// <returns></returns>
		private Task SafeExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			return SafeExecute(() => ExecuteAfter(invocation, returnValue));
		}

		/// <summary>
		/// Custom action executed after intercepted method return value.
		/// </summary>
		/// <param name="invocation"></param>
		/// <param name="returnValue"></param>
		/// /// <returns></returns>
		public virtual Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Custom continuation of intercepted method.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="invocation"></param>
		/// <returns></returns>
		public virtual async Task Continuation(Task task, IInvocation invocation)
		{
			await task;
			await this.SafeExecuteAfter(invocation, null);
		}

		/// <summary>
		/// Custom continuation of intercepted method.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="invocation"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual async Task<T> Continuation<T>(Task<T> task, IInvocation invocation)
		{
			var returnValue = await task;
			await this.SafeExecuteAfter(invocation, returnValue);
			return returnValue;
		}

		private void SafeExecute(Action action)
		{
			try
			{
				action?.Invoke();
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, serviceException.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError(exception, exception.Message);
				throw new ServiceException($"Error during call {GetType().Name}. {InterceptorHelper.BuildErrorMessageDetails(exception)}", exception);
			}
		}

		private Task SafeExecute(Func<Task> action)
		{
			try
			{
				return action?.Invoke();
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, serviceException.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogError(exception, exception.Message);
				throw new ServiceException($"Error during call {GetType().Name}. {InterceptorHelper.BuildErrorMessageDetails(exception)}", exception);
			}
		}
	}
}