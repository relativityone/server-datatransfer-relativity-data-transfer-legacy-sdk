using Polly;
using Polly.Retry;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
    public class RetryPolicyFactory
    {
        private const int DefaultMaxNumberOfRetries = 3;
        private const int DefaultBackoffBase = 2;

        private readonly IAPILog _logger;

        public RetryPolicyFactory(IAPILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This policy retries deadlock exceptions.
        /// </summary>
        public RetryPolicy CreateDeadlockExceptionRetryPolicy([CallerMemberName] string callerName = "")
        {
            return CreateDeadlockExceptionRetryPolicy(DefaultMaxNumberOfRetries, DefaultBackoffBase, callerName);
        }

        /// <summary>
        /// This policy retries deadlock exceptions.
        /// </summary>
        public RetryPolicy CreateDeadlockExceptionRetryPolicy(int maxNumberOfRetries, int backoffBase, [CallerMemberName] string callerName = "")
        {
            return Policy
                .Handle<Exception>(IsDeadlockException)
                .WaitAndRetry(maxNumberOfRetries, GetExponentialBackoffSleepDurationProvider(backoffBase), OnRetry);

            void OnRetry(Exception ex, TimeSpan waitTime, int retryNumber, Context context)
            {
                _logger.LogWarning(ex, "Deadlock occured when executing '{callerName}'. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
                    callerName,
                    retryNumber,
                    maxNumberOfRetries,
                    waitTime);
            }
        }

        /// <summary>
        /// This policy retries deadlock exceptions or results which contain word "deadlock".
        /// </summary>
        public RetryPolicy<object> CreateDeadlockExceptionAndResultRetryPolicy([CallerMemberName] string callerName = "")
        {
            return CreateDeadlockExceptionAndResultRetryPolicy(DefaultMaxNumberOfRetries, DefaultBackoffBase, callerName);
        }

        /// <summary>
        /// This policy retries deadlock exceptions or results which contain word "deadlock".
        /// </summary>
        public RetryPolicy<object> CreateDeadlockExceptionAndResultRetryPolicy(int maxNumberOfRetries, int backoffBase, [CallerMemberName] string callerName = "")
        {
            return Policy
                .Handle<Exception>(IsDeadlockException)
                .OrResult<object>(IsDeadlockErrorMessage)
                .WaitAndRetry(maxNumberOfRetries, GetExponentialBackoffSleepDurationProvider(backoffBase), OnRetry);

            void OnRetry(DelegateResult<object> result, TimeSpan waitTime, int retryNumber, Context context)
            {
                _logger.LogWarning(result.Exception, "Deadlock occured when executing '{callerName}'. Result: '{result}'. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
                    callerName,
                    result.Result,
                    retryNumber,
                    maxNumberOfRetries,
                    waitTime);
            }
        }

        private static Func<int, TimeSpan> GetExponentialBackoffSleepDurationProvider(int backoffBase)
        {
            return retryNumber => TimeSpan.FromSeconds(Math.Pow(backoffBase, retryNumber));
        }

        private bool IsDeadlockException(Exception exception)
        {
            var exceptions = GetAllExceptionsInChain(exception);
            bool isDeadlock = exceptions.Any(kCura.Data.RowDataGateway.Helper.IsDeadLock);

            if (!isDeadlock)
            {
                _logger.LogWarning(exception, "Will not retry - exception was not caused by SQL deadlock.");
            }

            return isDeadlock;
        }

        private bool IsDeadlockErrorMessage(object result)
        {
            const string expectedMessagePart = "deadlock";

            if (result is string resultAsString)
            {
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(resultAsString, expectedMessagePart, CompareOptions.IgnoreCase) >= 0;
            }

            return false;
        }

        private static IEnumerable<Exception> GetAllExceptionsInChain(Exception exception)
        {
            const int MaxChainLength = 100;

            int chainLength = 0;
            Exception currentException = exception;
            while (currentException != null && chainLength < MaxChainLength)
            {
                yield return currentException;

                chainLength++;
                currentException = currentException.InnerException;
            }
        }
    }
}
