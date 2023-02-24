// <copyright file="RelEyeMetricsService.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
	using System.Diagnostics;

	/// <inheritdoc />
	public class RelEyeMetricsService : IRelEyeMetricsService
	{
		private readonly ITelemetryPublisher _telemetryPublisher;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelEyeMetricsService"/> class.
		/// </summary>
		/// <param name="telemetryPublisher"></param>
		public RelEyeMetricsService(ITelemetryPublisher telemetryPublisher)
		{
			this._telemetryPublisher = telemetryPublisher;
		}

		/// <inheritdoc />
		public void PublishEvent(EventBase @event)
		{
			this._telemetryPublisher.PublishEvent(@event.EventName, @event.Attributes);

			var activity = Activity.Current;
			foreach (var attribute in @event.Attributes)
			{
				activity?.SetTag(attribute.Key, attribute.Value);
			}
		}
	}
}