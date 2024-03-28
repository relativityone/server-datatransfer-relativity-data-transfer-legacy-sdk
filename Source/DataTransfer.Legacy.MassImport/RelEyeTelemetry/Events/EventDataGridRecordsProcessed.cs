using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	public class EventDataGridRecordsProcessed : EventBase
	{
		public EventDataGridRecordsProcessed(string runID, int workspaceID, int toProcessCount, int processedCount, int emptyCount)
		{
			Attributes[TelemetryConstants.AttributeNames.RunID] = runID;
			Attributes[TelemetryConstants.AttributeNames.DataSourceID] = runID;
			Attributes[TelemetryConstants.AttributeNames.R1WorkspaceID] = workspaceID;
			Attributes[TelemetryConstants.AttributeNames.JobRequestedCount] = toProcessCount;
			Attributes[TelemetryConstants.AttributeNames.JobSuccessfulCount] = processedCount;
			Attributes[TelemetryConstants.AttributeNames.JobSkippedCount] = emptyCount;
		}

		public override string EventName => TelemetryConstants.EventName.DataGridRecordsProcessed;
	}
}
