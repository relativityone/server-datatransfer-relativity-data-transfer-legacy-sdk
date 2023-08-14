using Microsoft.VisualBasic;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.SQL;
using Relativity.DataTransfer.Legacy.Services.Toggles;
using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache
{
	internal class BatchResultCacheFactory : IBatchResultCacheFactory
	{
		private readonly IAPILog _logger;
		private readonly ISqlExecutor _sqlExecutor;
		private readonly IToggleProvider _toggleProvider;

		public BatchResultCacheFactory(IAPILog logger, ISqlExecutor sqlExecutor, IToggleProvider toggleProvider)
		{
			_logger = logger;
			_sqlExecutor = sqlExecutor;
			_toggleProvider = toggleProvider;
		}

		public IBatchResultCache Create()
		{
			if (_toggleProvider.IsEnabled<DisableBatchResultCacheToggle>())
			{
				return new NullBatchResultCache();
			}
			else
			{
				return new BatchResultCache(_logger, _sqlExecutor);
			}
		}
	}
}
