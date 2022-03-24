using System.Collections.Generic;
using Relativity.Core.Service;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal interface IMassImportMetricsService
	{
		void SendJobStarted(Relativity.MassImport.DTO.NativeLoadInfo settings, string importType, string system);
		void SendJobStarted(Relativity.MassImport.DTO.ImageLoadInfo settings, string importType, string system);
		void SendPreImportStagingTableStatistics(string correlationId, Dictionary<string, object> customData);
		void SendBatchCompleted(string correlationId, long importTimeInMilliseconds, string importType, string system, MassImportManagerBase.MassImportResults result, ImportMeasurements importMeasurements);
	}
}