// ----------------------------------------------------------------------------
// <copyright file="RetryableErrorsRetryPolicyFactory.cs" company="Relativity ODA LLC">
//   © Relativity All Rights Reserved.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Relativity.API;
using Relativity.Kepler.Exceptions;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies
{
	/// <inheritdoc />
	internal class RetryableKeplerErrorsRetryPolicyFactory : IKeplerRetryPolicyFactory
	{
		private const int DefaultNumberOfRetries = 3;
		private const int DefaultWaitTimeBaseInSeconds = 2;

		private static readonly IReadOnlyList<Type> FatalKeplerExceptionCandidates = new List<Type>(
			new[]
			{
				typeof(NotAuthorizedException),
				typeof(WireProtocolMismatchException),
				typeof(NotFoundException),
				typeof(PermissionDeniedException),
				typeof(ServiceNotFoundException),
			});

		private static readonly string[] FatalKeplerExceptionMessagesFragments =
		{
			"InvalidAppArtifactID",
			"Bearer token should not be null or empty",
		};

		private readonly int _maxNumberOfRetries;
		private readonly int _waitTimeBase;
		private readonly IAPILog _logger;

		public RetryableKeplerErrorsRetryPolicyFactory(IAPILog logger)
			: this(logger, DefaultNumberOfRetries, DefaultWaitTimeBaseInSeconds)
		{
		}

		public RetryableKeplerErrorsRetryPolicyFactory(IAPILog logger, int maxNumberOfRetries, int waitTimeBaseInSeconds)
		{
			_logger = logger;
			_maxNumberOfRetries = maxNumberOfRetries;
			_waitTimeBase = waitTimeBaseInSeconds;
		}

		/// <inheritdoc />
		public IAsyncPolicy CreateRetryPolicy()
		{
			return Policy
				.Handle((Func<Exception, bool>)IsRetryableException)
				.WaitAndRetryAsync(
					_maxNumberOfRetries,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(_waitTimeBase, retryAttempt)),
					OnRetry);
		}

		/// <inheritdoc />
		public IAsyncPolicy<T> CreateRetryPolicy<T>()
		{
			return Policy<T>
				.Handle((Func<Exception, bool>)IsRetryableException)
				.WaitAndRetryAsync(
					_maxNumberOfRetries,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(_waitTimeBase, retryAttempt)),
					OnRetry);
		}

		private static bool IsRetryableException(Exception exception)
		{
			return !IsFatalKeplerException(exception)
				   && !(exception is OperationCanceledException);
		}

		private static bool IsFatalKeplerException(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception));
			}

			var exceptionType = exception.GetType();
			if (FatalKeplerExceptionCandidates.Any(
					exceptionCandidateType => exceptionCandidateType.IsAssignableFrom(exceptionType)))
			{
				return true;
			}

			if (FatalKeplerExceptionMessagesFragments.Any(fragment => exception.Message.Contains(fragment)))
			{
				return true;
			}

			return false;
		}

		private Task OnRetry<TResult>(DelegateResult<TResult> result, TimeSpan timeSpan, int retryCount, Context context)
		{
			return this.OnRetry(result.Exception, timeSpan, retryCount, context);
		}

		private Task OnRetry(Exception exception, TimeSpan duration, int retryCount, Context context)
		{
			this._logger.LogWarning(
				exception,
				"RetryableErrorsRetryPolicyFactory: Call to Kepler service failed due to {ExceptionType}. Currently on attempt {RetryCount} out of {MaxRetries} and waiting {WaitSeconds} seconds before the next retry attempt.",
				exception.GetType(),
				retryCount,
				_maxNumberOfRetries,
				duration.TotalSeconds);

			return Task.CompletedTask;
		}
	}
}