using System;
using Polly;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class RetryOnExceptionDecoratorStage<TInput, TOutput> : DecoratorStage<TInput, TOutput>
	{
		private const int DefaultExponentialWaitTimeBase = 2;
		private const int MaxNumberOfRetries = 8; // 2^8 + 2^7 + ... + 2^1 = 510 seconds;

		private readonly string _actionName;
		private readonly MassImportContext _context;
		private readonly int _exponentialWaitTimeBase;
		private readonly int _numberOfRetries;

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
				kCura.Data.RowDataGateway.Config.NumberOfTriesOnDeadlock)
		{
		}

		public RetryOnExceptionDecoratorStage(
			IPipelineStage<TInput, TOutput> innerStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context,
			string actionName,
			int exponentialWaitTimeBase,
			int numberOfRetries) : base(pipelineExecutor, innerStage)
		{
			_actionName = actionName;
			_context = context;
			_exponentialWaitTimeBase = exponentialWaitTimeBase;

			_numberOfRetries = numberOfRetries;
			if (_numberOfRetries > MaxNumberOfRetries)
			{
				_context.Logger.LogInformation(
					"Requested number of retries ('{requested}') is greater than maximum number of retries ('{maximum}').",
					numberOfRetries,
					MaxNumberOfRetries);
				_numberOfRetries = MaxNumberOfRetries;
			}
		}

		public override TOutput Execute(TInput input)
		{
			Polly.Retry.RetryPolicy executionPolicy = GetRetryPolicy();
			return executionPolicy.Execute(() => base.Execute(input));
		}

		private Polly.Retry.RetryPolicy GetRetryPolicy()
		{
			return Policy
				.Handle<MassImportExecutionException>(IsRetryableException)
				.WaitAndRetry(
					_numberOfRetries,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(_exponentialWaitTimeBase, retryAttempt)),
					onRetry: (exception, span) =>
					{
						_context.ImportMeasurements.IncrementCounter($"Retry-'{_actionName}'");

						var convertedException = exception as MassImportExecutionException;
						if (convertedException != null && convertedException.IsDeadlock())
						{
							_context.Logger.LogError(exception, "Deadlock occurred while {action}.", _actionName);
						}
						else if (convertedException != null && convertedException.IsTimeout())
						{
							_context.Logger.LogError(exception, "Timeout occurred while {action}.", _actionName);
						}
					});
		}

		private bool IsRetryableException(MassImportExecutionException exception)
		{
			bool isRetryAbleException = exception.IsRetryable();
			if (!isRetryAbleException)
			{
				_context.Logger.LogError(exception, "Error occurred while {action}", _actionName);
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
