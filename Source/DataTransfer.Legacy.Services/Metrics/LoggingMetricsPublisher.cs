// <copyright file="LoggingMetricsPublisher.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	/// <summary> 
	/// Log metrics. 
	/// </summary> 
	public class LoggingMetricsPublisher : IMetricsPublisher
	{
		private readonly IAPILog _logger;

		/// <summary> 
		/// Initializes a new instance of the <see cref="LoggingMetricsPublisher"/> class. 
		/// </summary> 
		/// <param name="logger"></param> 
		public LoggingMetricsPublisher(IAPILog logger)
		{
			_logger = logger;
		}

		/// <summary> 
		/// Log metrics. 
		/// </summary> 
		/// <param name="metrics"></param> 
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns> 
		public Task Publish(Dictionary<string, object> metrics)
		{
			_logger.LogWarning("DataTransfer.Legacy Metrics: {@metrics}", metrics);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Log Health Check result
		/// </summary>
		/// <param name="isHealthy"></param>
		/// <param name="message"></param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns> 
		public Task PublishHealthCheckResult(bool isHealthy, string message)
		{
			if (isHealthy)
			{
				_logger.LogInformation("Health Check Result: {@isHealthy}, message: {@message}", isHealthy, message);
			}
			else
			{
				_logger.LogError("Health Check Result: {@isHealthy}, message: {@message}", isHealthy, message);
			}

			return Task.CompletedTask;
		}
	}
}