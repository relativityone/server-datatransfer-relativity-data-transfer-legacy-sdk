using System.Threading.Tasks;
using Castle.Core;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]

	public class IAPICommunicationModeService : BaseService, IIAPICommunicationModeService
	{
		private readonly IAPILog _logger;

		private readonly ICommunicationModeStorage _communicationModeStorage;

		public IAPICommunicationModeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory,
			IAPILog logger, ICommunicationModeStorage communicationModeStorage)
			: base(methodRunner, serviceContextFactory)
		{
			_logger = logger;
			_communicationModeStorage = communicationModeStorage;
		}

		public Task<IAPICommunicationMode> GetIAPICommunicationModeAsync(string correlationId)
		{
			return ExecuteAsync(GetIAPICommunicationModeAsync, null, correlationId);
		}

		private async Task<IAPICommunicationMode> GetIAPICommunicationModeAsync()
		{
			try
			{
				var (success, mode) = await _communicationModeStorage.TryGetModeAsync().ConfigureAwait(false);
				if (success)
				{
					return mode;
				}

				_logger.LogWarning($"Invalid IAPI communication mode in '{_communicationModeStorage.GetStorageKey()}' toggle. WebAPI IAPI communication mode will be used.");
				return IAPICommunicationMode.WebAPI;
			}
			catch
			{
				_logger.LogWarning($"'{_communicationModeStorage.GetStorageKey()}' toggle not found. WebAPI IAPI communication mode will be used.");
				return IAPICommunicationMode.WebAPI;
			}
		}
	}
}
