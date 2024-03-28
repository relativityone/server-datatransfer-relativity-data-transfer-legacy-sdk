using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	public interface IRelEyeMetricsService
	{
		/// <summary>
		/// Publishes event.
		/// </summary>
		/// <param name="event"></param>
		void PublishEvent(EventBase @event);
	}
}
