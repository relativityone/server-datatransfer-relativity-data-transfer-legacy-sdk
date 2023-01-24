// <copyright file="IEventsBuilder.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
using Relativity.Core.Service;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders
{
	/// <summary>
	/// Interface for event builders.
	/// </summary>
	public interface IEventsBuilder
	{
		/// <summary>
		/// Job start event.
		/// </summary>
		/// <returns></returns>
		EventJobStart BuildJobStartEvent(Relativity.MassImport.DTO.ImageLoadInfo settings, string importType);

		/// <summary>
		/// Job start event.
		/// </summary>
		/// <returns></returns>
		EventJobStart BuildJobStartEvent(Relativity.MassImport.DTO.NativeLoadInfo settings, string importType);

		/// <summary>
		/// Job batch completed event.
		/// </summary>
		/// <returns></returns>
		EventJobBatchCompleted BuildJobBatchCompletedEvent(MassImportManagerBase.MassImportResults results, string importType);

		/// <summary>
		/// General statistics event.
		/// </summary>
		/// <returns></returns>
		EventGeneralStatistics BuildGeneralStatisticsEvent(string runID, int workspaceID);
	}
}