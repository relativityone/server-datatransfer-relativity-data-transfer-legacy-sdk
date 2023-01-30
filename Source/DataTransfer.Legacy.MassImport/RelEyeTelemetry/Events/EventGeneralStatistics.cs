// <copyright file="EventGeneralStatistics.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	/// <inheritdoc/>
	public class EventGeneralStatistics : EventBase
	{
		/// <inheritdoc/>
		public override string EventName => Constants.EventName.GeneralStatistics;
	}
}