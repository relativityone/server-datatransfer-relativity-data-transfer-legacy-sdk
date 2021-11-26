using System;
using System.Collections.Generic;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class SendMetricWithPreImportStagingTablesDetails<T> : IPipelineStage<T>
	{
		private readonly MassImportContext _context;
		private readonly IStagingTableRepository _stagingTableRepository;
		private readonly IMassImportMetricsService _metricsService;


		public SendMetricWithPreImportStagingTablesDetails(MassImportContext context, IStagingTableRepository stagingTableRepository, IMassImportMetricsService metricsService)
		{
			_context = context;
			_stagingTableRepository = stagingTableRepository;
			_metricsService = metricsService;
		}

		public T Execute(T input)
		{
			// TODO we can include in this metric structure of other staging tables (e.g. multi objects).
			// TODO We can also send similar metric after batch import has completed (e.g. number of new/existing associated objects, audit details)

			try
			{
				IDictionary<int, int> numberOfChoicesPerCodeTypeId = _stagingTableRepository.ReadNumberOfChoicesPerCodeTypeId();
				var customData = MetricCustomDataBuilder
					.New()
					.WithChoicesDetails(numberOfChoicesPerCodeTypeId)
					.Build();
				_metricsService.SendPreImportStagingTableStatistics(_context.JobDetails.CorrelationId, customData);
			}
			catch (Exception ex)
			{
				// we do not want to stop mass import when that stage failed.
				_context.Logger.LogWarning(ex, "Failed to send a metric with a structure of choices staging table.");
			}

			return input;
		}
	}
}
