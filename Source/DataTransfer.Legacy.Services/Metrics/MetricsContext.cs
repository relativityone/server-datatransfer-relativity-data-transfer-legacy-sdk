// <copyright file="MetricsContext.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	/// <inheritdoc />
	public class MetricsContext : IMetricsContext
	{
		private readonly IEnumerable<IMetricsPublisher> _metricsPublishers;
		private readonly Dictionary<string, object> _metrics = new Dictionary<string, object>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MetricsContext"/> class.
		/// </summary>
		/// <param name="metricsPublishers"></param>
		public MetricsContext(IEnumerable<IMetricsPublisher> metricsPublishers)
		{
			_metricsPublishers = metricsPublishers;
		}

		/// <inheritdoc />
		public void PushProperty(string metric, object value)
		{
			if (!_metrics.TryGetValue(metric, out object dictValue))
			{
				_metrics.Add(metric, value);
			}
			else
			{
				_metrics[metric] = (long)dictValue + (long)value;

				var callsCountKey = $"{metric}:CallsCount";

				if (!_metrics.TryGetValue(callsCountKey, out object count))
				{
					_metrics[callsCountKey] = 2L;
				}
				else
				{
					_metrics[callsCountKey] = (long)count + 1L;
				}
			}
		}

		/// <inheritdoc />
		public async Task Publish()
		{
			foreach (var metricsPublisher in _metricsPublishers)
			{
				await metricsPublisher.Publish(_metrics);
			}
		}
	}
}