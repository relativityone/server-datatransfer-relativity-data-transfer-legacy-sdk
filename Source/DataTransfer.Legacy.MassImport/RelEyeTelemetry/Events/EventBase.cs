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
				[Constants.AttributeNames.R1TeamID] = Constants.Values.R1TeamID,
				[Constants.AttributeNames.ServiceName] = Constants.Values.ServiceName,
				[Constants.AttributeNames.ServiceNamespace] = Constants.Values.ServiceNamespace,
				[Constants.AttributeNames.ApplicationName] = Constants.Values.ApplicationName,
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