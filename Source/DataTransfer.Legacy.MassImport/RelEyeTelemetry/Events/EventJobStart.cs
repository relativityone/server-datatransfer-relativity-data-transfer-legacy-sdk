// <copyright file="EventJobStart.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	/// <inheritdoc/>
	public class EventJobStart : EventBase
	{
		/// <inheritdoc/>
		public override string EventName => Constants.EventName.JobStart;
	}
}