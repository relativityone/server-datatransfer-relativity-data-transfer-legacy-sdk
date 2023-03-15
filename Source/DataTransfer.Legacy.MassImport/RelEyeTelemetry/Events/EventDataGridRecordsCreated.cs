namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events
{
	public class EventDataGridRecordsCreated : EventBase
	{
		public EventDataGridRecordsCreated(int totalRecords, int emptyFiles)
		{
			Attributes[TelemetryConstants.AttributeNames.JobRequestedCount] = totalRecords;
			Attributes[TelemetryConstants.AttributeNames.JobSkippedCount] = emptyFiles;
		}

		public override string EventName => TelemetryConstants.EventName.DataGridRecordsCreated;
	}
}
