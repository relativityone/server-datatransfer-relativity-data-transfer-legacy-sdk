using Relativity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	public abstract class EventBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EventBase"/> class.
		/// </summary>
		protected EventBase()
		{
			Attributes = new Dictionary<string, object>
			{
				[TelemetryConstants.AttributeNames.R1TeamID] = TelemetryConstants.Values.R1TeamID,
				[TelemetryConstants.AttributeNames.ServiceName] = TelemetryConstants.Values.ServiceName,
				[TelemetryConstants.AttributeNames.ServiceNamespace] = TelemetryConstants.Values.ServiceNamespace,
				[TelemetryConstants.AttributeNames.ApplicationName] = TelemetryConstants.Values.ApplicationName,
			};
		}

		/// <summary>
		/// Gets the name of an event.
		/// </summary>
		public abstract string EventName { get; }

		/// <summary>
		/// Gets or sets dictionary containing attributes of an event.
		/// </summary>
		public Dictionary<string, object> Attributes { get; set; }
	}
}
