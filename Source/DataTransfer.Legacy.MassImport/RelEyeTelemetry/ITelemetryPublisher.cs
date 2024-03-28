using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	public interface ITelemetryPublisher
	{
		/// <summary>
		/// Publishes event.
		/// </summary>
		/// <param name="name">Name of the event.</param>
		/// <param name="attributes">Collection of the attributes.</param>
		void PublishEvent(string name, Dictionary<string, object> attributes);
	}
}
