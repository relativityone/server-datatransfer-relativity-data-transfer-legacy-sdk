using Castle.DynamicProxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Extensions;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	public class ToggleCheckInterceptor : InterceptorBase
	{
		private readonly ICommunicationModeStorage _communicationModeStorage;

		public ToggleCheckInterceptor(IAPILog logger, ICommunicationModeStorage communicationModeStorage) : base(logger)
		{
			_communicationModeStorage = communicationModeStorage;
		}

		public override void ExecuteBefore(IInvocation invocation)
		{
			var (success, mode) = _communicationModeStorage.TryGetModeAsync().Result;
			if (success)
			{
				if (mode == IAPICommunicationMode.ForceWebAPI)
				{
					throw new ServiceException($"IAPI communication mode set to {IAPICommunicationMode.ForceWebAPI.GetDescription()}. Kepler service disabled.");
				}
				return;
			}

			throw new ServiceException("Unable to determine IAPI communication mode toggle value. Please verify that instance setting is properly set (Section: DataTransfer.Legacy, Name: IAPICommunicationMode)");
		}
	}
}
