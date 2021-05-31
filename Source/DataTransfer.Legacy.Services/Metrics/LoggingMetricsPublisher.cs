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
			this._logger = logger;
		}

		/// <summary>
		/// Log metrics.
		/// </summary>
		/// <param name="metrics"></param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task Publish(Dictionary<string, object> metrics)
		{
			this._logger.LogInformation("Metrics: {@metrics}", metrics);

			return Task.CompletedTask;
		}
	}
}