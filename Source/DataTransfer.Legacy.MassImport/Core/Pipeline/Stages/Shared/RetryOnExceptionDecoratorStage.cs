using System;
using Polly;
using Polly.Wrap;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class RetryOnExceptionDecoratorStage<TInput, TOutput> : DecoratorStage<TInput, TOutput>
	{
		private const int DefaultExponentialWaitTimeBase = 2; // 2^8 + 2^7 + ... + 2^1 = 510 seconds;
		private const int DefaultNumberOfRetriesOnDeadlock = 3;
		private const int DefaultNumberOfRetriesOnBcpError = 5;

		private readonly string _actionName;
		private readonly MassImportContext _context;
		private readonly int _exponentialWaitTimeBase;
		private readonly int _numberOfRetriesForDeadlockAndTimeout;
		private readonly int _numberOfRetriesForBcpError;

		public RetryOnExceptionDecoratorStage(
			IPipelineStage<TInput, TOutput> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context,
			string actionName) :
			this(
				innerStage,
				pipelineExecutor,
				context,
				actionName,
				DefaultExponentialWaitTimeBase,
				DefaultNumberOfRetriesOnDeadlock,
				DefaultNumberOfRetriesOnBcpError)
		{
		}

		public RetryOnExceptionDecoratorStage(
			IPipelineStage<TInput, TOutput> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context,
			string actionName,
			int exponentialWaitTimeBase,
			int numberOfRetriesForDeadlockAndTimeout,
			int numberOfRetriesForBcpError) : base(pipelineExecutor, innerStage)
		{
			_actionName = actionName;
			_context = context;
			_exponentialWaitTimeBase = exponentialWaitTimeBase;
			_numberOfRetriesForDeadlockAndTimeout = numberOfRetriesForDeadlockAndTimeout;
			_numberOfRetriesForBcpError = numberOfRetriesForBcpError;
		}

		public override TOutput Execute(TInput input)
		{
			PolicyWrap executionPolicy = GetRetryPolicyForTimeoutAndDeadlock().Wrap(GetRetryPolicyForBcp());
			return executionPolicy.Execute(() => base.Execute(input));
		}

		private Polly.Retry.RetryPolicy GetRetryPolicyForTimeoutAndDeadlock()
		{
			return Policy
				.Handle<MassImportExecutionException>(IsRetryableException)
				.WaitAndRetry(
					retryCount: _numberOfRetriesForDeadlockAndTimeout,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(_exponentialWaitTimeBase, retryAttempt)),
					onRetry: (exception, waitTime, retryNumber, context) =>
					{
						LogExceptionInternal(exception, retryNumber, waitTime, _numberOfRetriesForDeadlockAndTimeout);
					});
		}

		private void LogExceptionInternal(Exception exception, int retryNumber, TimeSpan waitTime, int numberOfRetries)
		{
			_context.ImportMeasurements.IncrementCounter($"Retry-'{_actionName}'");

			if (exception is MassImportExecutionException convertedException)
			{
				_context.Logger.LogWarning(exception,
					"{Category} error occurred while executing {action}. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
					convertedException.ErrorCategory,
					_actionName,
					retryNumber,
					numberOfRetries,
					waitTime);
			}
			else
			{
				_context.Logger.LogWarning(exception,
					"Retryable error while executing {action}. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
					_actionName,
					retryNumber,
					numberOfRetries,
					waitTime);
			}
		}

		private Polly.Retry.RetryPolicy GetRetryPolicyForBcp()
		{
			return Policy
				.Handle<MassImportExecutionException>(ex => ex.IsBcp())
				.WaitAndRetry(
					_numberOfRetriesForBcpError,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(_exponentialWaitTimeBase, retryAttempt)),
					onRetry: (exception, waitTime, retryNumber, context) =>
					{
						LogExceptionInternal(exception, retryNumber, waitTime, _numberOfRetriesForBcpError);
					});
		}

		private bool IsRetryableException(MassImportExecutionException exception)
		{
			// Is retryable does not contain BCP Exception category
			bool isRetryAbleException = exception.IsRetryable();
			if (!isRetryAbleException)
			{
				_context.Logger.LogError(exception, "Non Retryable Error {category} occurred while {action}", exception.ErrorCategory, _actionName);
			}
			return isRetryAbleException;
		}
	}

	internal static class RetryOnExceptionDecoratorStage
	{
		public static RetryOnExceptionDecoratorStage<T1, T2> New<T1, T2>(
			IPipelineStage<T1, T2> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context,
			string actionName)
		{
			return new RetryOnExceptionDecoratorStage<T1, T2>(innerStage, pipelineExecutor, context, actionName);
		}
	}
}
