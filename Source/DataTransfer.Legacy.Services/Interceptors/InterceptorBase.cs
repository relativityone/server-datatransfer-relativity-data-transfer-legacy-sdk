// <copyright file="InterceptorBase.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Base class for all interceptors.
	/// </summary>
	public abstract class InterceptorBase : IInterceptor
	{
		/// <summary>
		/// Wrap intercepted method with custom actions.
		/// </summary>
		/// <param name="invocation"></param>
		public virtual void Intercept(IInvocation invocation)
		{
			this.ExecuteBefore(invocation);

			invocation.Proceed();

			Type type = invocation.ReturnValue?.GetType();

			if (type != null && (type.IsSubclassOf(typeof(Task)) || type == typeof(Task)))
			{
				invocation.ReturnValue = Continuation((dynamic)invocation.ReturnValue, invocation);
			}
			else
			{
				// synchronous method cannot be changed to asynchronous
				this.ExecuteAfter(invocation, invocation.ReturnValue).Wait();
			}
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
			await this.ExecuteAfter(invocation, null);
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
			await this.ExecuteAfter(invocation, returnValue);
			return returnValue;
		}
	}
}