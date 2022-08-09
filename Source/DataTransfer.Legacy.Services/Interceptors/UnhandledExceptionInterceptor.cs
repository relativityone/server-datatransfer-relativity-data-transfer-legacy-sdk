// <copyright file="UnhandledExceptionInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Catch all unhandled exceptions from intercepted method.
	/// This interceptor should be first one on the list of attributes to catch all unhandled exception from other interceptors.
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
			catch (Core.Exception.Permission permissionException)
			{
				Logger.LogError(permissionException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, permissionException.Message);
				throw new PermissionDeniedException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(permissionException)}", permissionException);
			}
			catch (Core.Exception.BaseException baseException)
			{
				Logger.LogError(baseException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, baseException.Message);
				throw new NotFoundException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(baseException)}", baseException);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}

		/// <inheritdoc />
		public override async Task Continuation(Task task, IInvocation invocation)
		{
			try
			{
				await base.Continuation(task, invocation);
			}
			catch (Core.Exception.Permission permissionException)
			{
				Logger.LogError(permissionException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, permissionException.Message);
				throw new PermissionDeniedException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(permissionException)}", permissionException);
			}
			catch (Core.Exception.BaseException baseException)
			{
				Logger.LogError(baseException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, baseException.Message);
				throw new NotFoundException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(baseException)}", baseException);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during custom continuation of call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during custom continuation of call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}

		/// <inheritdoc />
		public override async Task<T> Continuation<T>(Task<T> task, IInvocation invocation)
		{
			try
			{
				return await base.Continuation(task, invocation);
			}
			catch (Core.Exception.Permission permissionException)
			{
				Logger.LogError(permissionException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, permissionException.Message);
				throw new PermissionDeniedException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(permissionException)}", permissionException);
			}
			catch (Core.Exception.BaseException baseException)
			{
				Logger.LogError(baseException, "There was an error during call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, baseException.Message);
				throw new NotFoundException($"Error during call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(baseException)}", baseException);
			}
			catch (ServiceException serviceException)
			{
				Logger.LogError(serviceException, "There was an error during continuation of call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, serviceException.Message);
				throw;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "There was an error during continuation of call {type}.{method} - {message}", invocation.TargetType.Name, invocation.Method.Name, e.Message);
				throw new ServiceException($"Error during custom continuation of call {invocation.TargetType.Name}.{invocation.Method.Name}. {InterceptorHelper.BuildErrorMessageDetails(e)}", e);
			}
		}
	}
}