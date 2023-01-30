// <copyright file="ITelemetryPublisher.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	using System.Collections.Generic;

	/// <summary>
	/// Publish metrics to any sink.
	/// </summary>
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