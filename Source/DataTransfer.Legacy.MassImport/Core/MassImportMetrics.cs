using System.Collections.Generic;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using System.Diagnostics;
using Relativity.Core.Service;
using Relativity.Logging;
using Relativity.MassImport.Data;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core
{
    internal class MassImportMetrics : IMassImportMetricsService
    {
        private readonly IAPM _apm;
        private readonly ILog _logger;

        public MassImportMetrics(ILog logger, IAPM apm)
        {
            _logger = logger;
            _apm = apm;
        }

        public void SendJobStarted(Relativity.MassImport.DTO.NativeLoadInfo settings, string importType, string system)
        {
            var customData = MetricCustomDataBuilder
                .New()
                .WithContext(importType, system)
                .WithSettings(settings)
                .Build();
            SendCounter(Constants.MassImportMetricsBucketNames.JobStarted, settings.RunID, customData);
        }

        public void SendJobStarted(Relativity.MassImport.DTO.ImageLoadInfo settings, string importType, string system)
        {
            var customData = MetricCustomDataBuilder
                .New()
                .WithContext(importType, system)
                .WithSettings(settings)
                .Build();
            SendCounter(Constants.MassImportMetricsBucketNames.JobStarted, settings.RunID, customData);
        }

        public void SendPreImportStagingTableStatistics(string correlationId, Dictionary<string, object> customData)
        {
            SendCounter(Constants.MassImportMetricsBucketNames.PreImportStagingTableDetails, correlationId, customData);
        }

        public void SendBatchCompleted(string correlationId, long importTimeInMilliseconds, string importType, string system, MassImportManagerBase.MassImportResults result, ImportMeasurements importMeasurements)
        {
            var customData = MetricCustomDataBuilder
                .New()
                .WithContext(importType, system)
                .WithResult(result)
                .WithMeasurements(importMeasurements)
                .Build();

            SendTimer(Constants.MassImportMetricsBucketNames.BatchCompleted, correlationId, importTimeInMilliseconds, customData);

            if (result.ExceptionDetail != null) // when error occurred, we want to log metrics
            {
                _logger.LogError("Import failed after '{importTime}'ms, metrics: {@customData}", importTimeInMilliseconds, customData);
				TraceHelper.SetStatusError(Activity.Current, $"Import failed after '{importTimeInMilliseconds}'ms, metrics: {@customData}");
			}
        }
        
        public void SendFieldDetails(string correlationId, FieldInfo field)
        {
            var customData = MetricCustomDataBuilder
                .New()
                .WithFieldInfo(field)
                .Build();

            SendCounter(Constants.MassImportMetricsBucketNames.FieldDetails, correlationId, customData);
        }

        private void SendCounter(string bucketName, string correlationId, Dictionary<string, object> customData)
        {
            _apm.CountOperation(
                name: bucketName,
                correlationID: correlationId,
                customData: customData).Write();

            _logger.LogInformation(
                "Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
                bucketName,
                customData);
        }

        private void SendTimer(
            string bucketName, 
            string correlationId, 
            long timeInMilliseconds,
            Dictionary<string, object> customData)
        {
            _apm.TimedOperation(
                name: bucketName,
                correlationID: correlationId,
                precalculatedMilliseconds: timeInMilliseconds,
                customData: customData);

            _logger.LogInformation(
                "Relativity.MassImport metric. Bucket: {bucketName}, type: Timer, value: {timeInMilliseconds},  customData: {@customData}",
                bucketName,
                timeInMilliseconds,
                customData);
        }
    }
}