// <copyright file="InterceptorBase.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Base class for all interceptors.
	/// Orders of interceptor can be important.
	/// Interceptors are executed from top to bottom (attributes order).
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

		private void SafeExecuteBefore(IInvocation invocation)
		{
			SafeExecute(() => ExecuteBefore(invocation), invocation);
		}

		private async Task SafeExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			await SafeExecute(async () => await ExecuteAfter(invocation, returnValue), invocation);
		}

		private void SafeExecute(Action action, IInvocation invocation)
		{
			try
			{
				action.Invoke();
			}
            catch (Core.Exception.Permission permissionException)
            {
                Logger.LogError(permissionException, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, permissionException.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Permission error: {permissionException.Message}", permissionException);
				throw new PermissionDeniedException($"Error during interceptor action {GetType().Name} for {invocation.TargetType.Name}.{invocation.Method.Name} {InterceptorHelper.BuildErrorMessageDetails(permissionException)}", permissionException);
            }
			catch (ServiceException serviceException)
			{
                Logger.LogError(serviceException, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, serviceException.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Service error: {serviceException.Message}", serviceException);
				throw;
			}
			catch (Exception exception)
			{
                Logger.LogError(exception, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, exception.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Error: {exception.Message}", exception);
				throw new ServiceException($"Error during interceptor action {GetType().Name}. {InterceptorHelper.BuildErrorMessageDetails(exception)}", exception);
			}
		}

		private async Task SafeExecute(Func<Task> action, IInvocation invocation)
		{
			try
			{
				await action.Invoke();
			}
            catch (Core.Exception.Permission permissionException)
            {
                Logger.LogError(permissionException, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, permissionException.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Permission error: {permissionException.Message}", permissionException);
				throw new PermissionDeniedException($"Error during interceptor action {GetType().Name} for {invocation.TargetType.Name}.{invocation.Method.Name} {InterceptorHelper.BuildErrorMessageDetails(permissionException)}", permissionException);
            }
            catch (ServiceException serviceException)
            {
                Logger.LogError(serviceException, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, serviceException.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Service error: {serviceException.Message}", serviceException);
				throw;
			}
			catch (Exception exception)
			{
                Logger.LogError(exception, "Error during interceptor action {interceptor} - {type}.{method} - {message}", GetType().Name, invocation.TargetType.Name, invocation.Method.Name, exception.Message);
				TraceHelper.SetStatusError(Activity.Current, $"Error: {exception.Message}", exception);
				throw new ServiceException($"Error during interceptor action {GetType().Name}. {InterceptorHelper.BuildErrorMessageDetails(exception)}", exception);
			}
		}
	}
}