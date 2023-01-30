// <copyright file="IRelEyeMetricsService.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;

	/// <summary>
	/// RelEyeMetricsService interface.
	/// </summary>
	public interface IRelEyeMetricsService
	{
		/// <summary>
		/// Publishes event.
		/// </summary>
		/// <param name="event"></param>
		void PublishEvent(EventBase @event);
	}
}