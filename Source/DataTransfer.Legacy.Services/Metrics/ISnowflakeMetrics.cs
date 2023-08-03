using Relativity.Core;
using Relativity.Core.Service;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	public interface ISnowflakeMetrics
	{
		void LogTelemetryMetricsForImport(
			BaseServiceContext serviceContext,
			MassImportManagerBase.MassImportResults results,
			ExecutionSource executionSource,
			int workspaceID);
	}
}