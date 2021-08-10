namespace DataTransfer.Legacy.PostInstallEventHandler
{
	using System;
	using System.Runtime.CompilerServices;
	using Polly;
	using Relativity.API;

	/// <inheritdoc />
	public class RetryPolicyProvider : IRetryPolicyProvider
	{
		private readonly IAPILog _logger;

		private readonly TimeSpan[] _retryTimes;

		/// <summary>
		/// Initializes a new instance of the <see cref="RetryPolicyProvider"/> class.
		/// </summary>
		/// <param name="logger">IAPILog.</param>
		public RetryPolicyProvider(IAPILog logger, TimeSpan[] retryTimes)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_retryTimes = retryTimes;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RetryPolicyProvider"/> class.
		/// </summary>
		/// <param name="logger">IAPILog.</param>
		public RetryPolicyProvider(IAPILog logger)
		{
			this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_retryTimes = new[]
			{
				TimeSpan.FromSeconds(2),
				TimeSpan.FromSeconds(4),
				TimeSpan.FromSeconds(61)
			};
		}

		/// <inheritdoc />
		public Policy GetAsyncRetryPolicy([CallerMemberName] string operationName = "")
		{
			return Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(
					_retryTimes,
					(exception, timeSpan, retryCount, context) =>
					{
						var waitTime = _retryTimes[retryCount-1].TotalSeconds;
						int numberOfRetries = this._retryTimes.Length;
						this._logger.LogWarning(
							exception,
							"Retrying {operationName} number {retryCount} of {numberOfRetries}, wait time: {waitTime}s",
							operationName,
							retryCount,
							numberOfRetries,
							waitTime);
					});
		}
	}
}