// <copyright file="EventJobBatchCompleted.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	/// <inheritdoc/>
	public class EventJobBatchCompleted : EventBase
	{
		/// <summary>
		/// Gets name of event.
		/// </summary>
		public override string EventName => TelemetryConstants.EventName.JobBatchCompleted;
	}
}