using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace Relativity.DataTransfer.Legacy.Services
{
	public interface ICommunicationModeStorage
	{
		Task<(bool, IAPICommunicationMode)> TryGetModeAsync();
		string GetStorageKey();
	}
}