using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
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
