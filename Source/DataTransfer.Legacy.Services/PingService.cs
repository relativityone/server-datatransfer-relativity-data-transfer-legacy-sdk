using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class PingService : BaseService, IPingService
	{
		public PingService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory)
			: base(methodRunner, serviceContextFactory)
		{
		}

		public Task<string> PingAsync()
		{
			return Task.FromResult("Pong");
		}
	}
}
