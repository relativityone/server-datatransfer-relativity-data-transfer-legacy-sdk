using Polly;
using Polly.Retry;
using Relativity.API;
using System;
using System.Collections.Generic;
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

        public RetryPolicy CreateDeadlockRetryPolicy([CallerMemberName] string callerName = "")
        {
            return CreateDeadlockRetryPolicy(DefaultMaxNumberOfRetries, DefaultBackoffBase, callerName);
        }

        public RetryPolicy CreateDeadlockRetryPolicy(int maxNumberOfRetries, int backoffBase, [CallerMemberName] string callerName = "")
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

        private static Func<int, TimeSpan> GetExponentialBackoffSleepDurationProvider(int backoffBase)
        {
            return retryNumber => TimeSpan.FromSeconds(Math.Pow(backoffBase, retryNumber));
        }

        private bool IsDeadlockException(Exception exception)
        {
            Exception lastException = exception;
            var exceptions = GetAllExceptionsInChain(exception);
            bool isDeadlock = exceptions.Any(kCura.Data.RowDataGateway.Helper.IsDeadLock);

            _logger.LogWarning(exception, "Will not retry - exception was not caused by SQL deadlock.");

            return isDeadlock;
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
