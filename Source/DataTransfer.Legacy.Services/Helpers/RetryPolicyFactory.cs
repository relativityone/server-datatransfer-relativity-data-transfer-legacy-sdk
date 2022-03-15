using Polly;
using Polly.Retry;
using Relativity.API;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
    public class RetryPolicyFactory
    {
        private readonly IAPILog _logger;

        public RetryPolicyFactory(IAPILog logger)
        {
            _logger = logger;
        }

        public RetryPolicy CreateDeadlockRetryPolicy([CallerMemberName] string callerName = "")
        {
            const int MaxNumberOfRetries = 3;
            const int BackoffBase = 2;

            return Policy
                .Handle<Exception>(IsDeadlockException)
                .WaitAndRetry(MaxNumberOfRetries, GetExponentialBackoffSleepDurationProvider(BackoffBase), OnRetry);

            void OnRetry(Exception ex, TimeSpan waitTime, int retryNumber, Context context)
            {
                _logger.LogWarning(ex, "Deadlock occured when executing '{callerName}'. Retry '{retryNumber}' out of '{maxNumberOfRetries}'. Waiting for {waitTime} before next retry attempt.",
                    callerName,
                    retryNumber,
                    MaxNumberOfRetries,
                    waitTime);
            }
        }

        private static Func<int, TimeSpan> GetExponentialBackoffSleepDurationProvider(int backoffBase)
        {
            return retryNumber => TimeSpan.FromSeconds(Math.Pow(backoffBase, retryNumber));
        }

        private static bool IsDeadlockException(Exception exception)
        {
            var exceptions = new[]
            {
                exception.GetBaseException(),
                exception,
                exception.InnerException
            };
            return exceptions.Any(kCura.Data.RowDataGateway.Helper.IsDeadLock);
        }
    }
}
