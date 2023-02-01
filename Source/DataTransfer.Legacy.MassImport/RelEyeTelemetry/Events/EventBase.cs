// <copyright file="EventBase.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	using System.Collections.Generic;

	/// <summary>
	/// Base class for events.
	/// </summary>
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