using System;
using System.Collections.Concurrent;
using System.Linq;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Telemetry.MetricsCollection;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	public class SnowflakeMetrics : ISnowflakeMetrics
	{
		private static ConcurrentDictionary<int, Guid> _workspaceGuidCache = new ConcurrentDictionary<int, Guid>();

		public void LogTelemetryMetricsForImport(BaseServiceContext serviceContext, MassImportManagerBase.MassImportResults results, ExecutionSource executionSource, int workspaceID)
		{
			long documentsCreated = results.ArtifactsCreated;
			Guid workspaceGuid = RetrieveWorkspaceGuid(workspaceID, serviceContext);
			if (documentsCreated > 0)
			{
				switch (executionSource)
				{
					case ExecutionSource.Rdc:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_DOCUMENT_COUNT_RDC, workspaceGuid, documentsCreated);
							break;
						}
					case ExecutionSource.ImportAPI:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_DOCUMENT_COUNT_IMPORTAPI, workspaceGuid, documentsCreated);
							break;
						}
					case ExecutionSource.RIP:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_DOCUMENT_COUNT_RIP, workspaceGuid, documentsCreated);
							break;
						}
					case ExecutionSource.Processing:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_DOCUMENT_COUNT_PROCESSING, workspaceGuid, documentsCreated);
							break;
						}
					case ExecutionSource.Unknown:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_DOCUMENT_COUNT_UNKNOWN, workspaceGuid, documentsCreated);
							break;
						}
				}
			}

			long filesCreated = results.FilesProcessed;
			if (filesCreated > 0)
			{
				switch (executionSource)
				{
					case ExecutionSource.Rdc:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_FILE_COUNT_RDC, workspaceGuid, filesCreated);
							break;
						}
					case ExecutionSource.ImportAPI:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_FILE_COUNT_IMPORTAPI, workspaceGuid, filesCreated);
							break;
						}
					case ExecutionSource.RIP:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_FILE_COUNT_RIP, workspaceGuid, filesCreated);
							break;
						}
					case ExecutionSource.Processing:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_FILE_COUNT_PROCESSING, workspaceGuid, filesCreated);
							break;
						}
					case ExecutionSource.Unknown:
						{
							Client.MetricsClient.LogPointInTimeLong(Constants.MassImportMetricsBucketNames.REQUIRED_WORKSPACE_FILE_COUNT_UNKNOWN, workspaceGuid, filesCreated);
							break;
						}
				}
			}
		}

		private static Guid RetrieveWorkspaceGuid(int workspaceID, BaseServiceContext serviceContext)
		{
			ArtifactGuidManager artifactGuidManager = new ArtifactGuidManager(serviceContext.GetMasterDbServiceContext());
			SnowflakeMetrics._workspaceGuidCache.TryAdd(workspaceID, artifactGuidManager.GetGuidsByArtifactID(workspaceID).SingleOrDefault());

			return SnowflakeMetrics._workspaceGuidCache[workspaceID];
		}
	}
}