using System;
using Polly;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;

namespace Relativity.DataTransfer.Legacy.Services.SQL
{
	public class SqlRetryPolicy : ISqlRetryPolicy
	{
		private readonly int _exponentialBaseInSeconds = 2;
		private const int RetryCount = 5; // 2 + 4 + 8 + 16 + 32 = 62s max
		private readonly IAPILog _logger;
		private readonly Policy _syncPolicy;
		private readonly Policy _asyncPolicy;

		public SqlRetryPolicy(IAPILog logger)
		{
			_logger = logger;
			_syncPolicy = GetSyncRetryPolicy();
			_asyncPolicy = GetAsyncRetryPolicy();
		}

		// For Unit Tests Only
		public SqlRetryPolicy(IAPILog logger, int exponentialBaseInSeconds) : this(logger)
		{
			_exponentialBaseInSeconds = exponentialBaseInSeconds;
		}

		public async Task<T> ExecuteAsync<T>(Func<Task<T>> function, [CallerMemberName] string operationName = "")
		{
			return await _asyncPolicy.ExecuteAsync(function, new Context(operationName));
		}

		public async Task ExecuteAsync(Func<Task> function, [CallerMemberName] string operationName = "")
		{
			await _asyncPolicy.ExecuteAsync(function, new Context(operationName));
		}

		public void Execute(Action function, [CallerMemberName] string operationName = "")
		{
			_syncPolicy.Execute(function, new Context(operationName));
		}

		public T Execute<T>(Func<T> function, [CallerMemberName] string operationName = "")
		{
			return _syncPolicy.Execute(function, new Context(operationName));
		}

		private Policy GetAsyncRetryPolicy()
		{
			return Policy
				.Handle<SqlException>(this.IsTransient)
				.Or<InvalidOperationException>(x =>
					x.Message.Contains(SqlErrorConstants.InternalConnectionFatalError) ||
					x.Message.Contains(SqlErrorConstants.ConnectionClosedError) ||
					x.Message.Contains(SqlErrorConstants.ConnectionNotAvailableError))
				.WaitAndRetryAsync(
					RetryCount,
					ExponentialRetryTime,
					(exception, timeSpan, retryCount, context) =>
						this.LogMessage(exception, retryCount, context, timeSpan, RetryCount));
		}
		
		private Policy GetSyncRetryPolicy()
		{
			return Policy
				.Handle<SqlException>(this.IsTransient)
				.Or<InvalidOperationException>(x =>
					x.Message.Contains(SqlErrorConstants.InternalConnectionFatalError) ||
					x.Message.Contains(SqlErrorConstants.ConnectionClosedError) ||
					x.Message.Contains(SqlErrorConstants.ConnectionNotAvailableError))
				.WaitAndRetry(
					RetryCount,
					ExponentialRetryTime,
					(exception, timeSpan, retryCount, context) =>
						this.LogMessage(exception, retryCount, context, timeSpan, RetryCount));
		}

		private TimeSpan ExponentialRetryTime(int retryAttempt)
		{
			return TimeSpan.FromSeconds(Math.Pow(_exponentialBaseInSeconds, retryAttempt));
		}

		private bool IsTransient(SqlException exception)
		{
			var isTransient = SqlErrorCodes.Any(x => x.Value.Contains(exception.Number));

			if (isTransient)
			{
				this._logger.LogWarning(
					exception,
					"Transient SQL error has occurred. Number: {errorNumber}, category: {errorCategory}",
					exception.Number,
					GetErrorCategory(exception));
			}
			else
			{
				this._logger.LogError("Non-retryable SQL error has occurred. Number: {errorNumber}", exception.Number);
			}

			return isTransient;
		}

		private static string GetErrorCategory(SqlException exception) => SqlErrorCodes.FirstOrDefault(x => x.Value.Contains(exception.Number)).Key;

		private void LogMessage(Exception exception, int retryAttempt, Context context, TimeSpan waitTime, int maxRetryCount)
		{
			this._logger.LogWarning(
				exception,
				"Retrying {operationName} number {retryAttempt} of {numberOfRetries}, wait time: {waitTime}s",
				context.ExecutionKey,
				retryAttempt,
				maxRetryCount,
				waitTime);
		}

		private static readonly int[] TemporaryIssueErrorCodes =
{
			SqlErrorConstants.ConnectionError,
			SqlErrorConstants.NetworkPathNotFound,
			SqlErrorConstants.SpecifiedNetworkNameNoLongerAvailable,
			SqlErrorConstants.SessionInKillState,
			SqlErrorConstants.DatabaseCannotAutoStartDuringShutdownOrStartup,
			SqlErrorConstants.DatabaseNameDoesNotExist,
			SqlErrorConstants.DatabaseNotFoundByInternalId,
			SqlErrorConstants.DatabaseOccupiedInSingleUserMode,
			SqlErrorConstants.DatabaseCannotBeOpenedMarkedSuspect,
			SqlErrorConstants.DatabaseInRestoreProcess,
			SqlErrorConstants.DatabaseOffline,
			SqlErrorConstants.DatabaseInTransition,
			SqlErrorConstants.DatabaseReplicaNotInCorrectRole,
			SqlErrorConstants.NoHighAvailabilityNodeQuorum,
			SqlErrorConstants.UserNameOrPasswordIncorrect,
			SqlErrorConstants.DatabaseSnapshotCannotBeCreated,
			SqlErrorConstants.CannotChangeState,
			SqlErrorConstants.UserSessionStateHasChanged,
			SqlErrorConstants.CannotOpenUserDefaultDatabase,
			SqlErrorConstants.ShutdownInProgress,
			SqlErrorConstants.ExistingConnectionForciblyClosedByRemoteHost,
			SqlErrorConstants.UserAccountIsDisabled,
		};

		// Error codes copied from 'kCura.Data.RowDataGateway.Helper (created by selecting sys.message containing 'timeout' in message.
		private static readonly int[] DeadlockErrorCodes =
		{
			SqlErrorConstants.Deadlocked,
			2755,
			3635,
			3928,
			5231,
			5252,
			17888,
			21870,
			22840,
		};

		private static readonly int[] TimeoutErrorCodes =
		{
			SqlErrorConstants.ConnectionTimeout,
			258,
			121,
		};

		private static readonly Dictionary<string, int[]> SqlErrorCodes = new Dictionary<string, int[]>
		{
			{ "Timeout", TimeoutErrorCodes },
			{ "Deadlock", DeadlockErrorCodes },
			{ "Temporary", TemporaryIssueErrorCodes },
		};
	}
}
