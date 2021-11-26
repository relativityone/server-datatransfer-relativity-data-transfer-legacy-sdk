using System.Collections.Generic;
using Relativity.Core.Service;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal interface IMassImportMetricsService
	{
		void SendJobStarted(NativeLoadInfo settings, string importType, string system);
		void SendJobStarted(ImageLoadInfo settings, string importType, string system);
		void SendPreImportStagingTableStatistics(string correlationId, Dictionary<string, object> customData);
		void SendBatchCompleted(string correlationId, long importTimeInMilliseconds, string importType, string system, IMassImportManagerInternal.MassImportResults result, ImportMeasurements importMeasurements);
	}
}