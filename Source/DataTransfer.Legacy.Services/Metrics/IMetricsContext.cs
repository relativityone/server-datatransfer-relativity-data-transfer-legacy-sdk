// <copyright file="IMetricsContext.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	/// <summary>
	/// Single place to gather all metrics before pushing them to any sink.
	/// </summary>
	public interface IMetricsContext
	{
		/// <summary>
		/// Push key-value metric.
		/// </summary>
		/// <param name="metric"></param>
		/// <param name="value"></param>
		void PushProperty(string metric, object value);

		/// <summary>
		/// Publish all gathered metrics to all registered sinks.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task Publish();
	}
}