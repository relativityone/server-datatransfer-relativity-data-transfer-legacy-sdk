using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("User Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("user")]
	public interface IUserService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveAllAssignableInCaseAsync")]
		Task<DataSetWrapper> RetrieveAllAssignableInCaseAsync(int workspaceID, string correlationID);

		/// <summary>
		/// parameters don't match WebAPI, but endpoint is obsolete anyway
		/// </summary>
		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("LogoutAsync")]
		Task LogoutAsync(string correlationID);

		/// <summary>
		/// parameters don't match WebAPI, but endpoint is obsolete anyway
		/// </summary>
		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("ClearCookiesBeforeLoginAsync")]
		Task ClearCookiesBeforeLoginAsync(string correlationID);

		/// <summary>
		/// parameters don't match WebAPI, but endpoint is obsolete anyway
		/// </summary>
		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("LoggedInAsync")]
		Task<bool> LoggedInAsync(string correlationID);

		/// <summary>
		/// parameters don't match WebAPI, but endpoint is obsolete anyway
		/// </summary>
		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("LoginAsync")]
		Task<bool> LoginAsync([SensitiveData] string emailAddress, [SensitiveData] string password, string correlationID);

		/// <summary>
		/// parameters don't match WebAPI, but endpoint is obsolete anyway
		/// </summary>
		[Obsolete("I consider it not necessary anymore since we have Kepler auth")]
		[HttpPost]
		[Route("GenerateDistributedAuthenticationTokenAsync")]
		Task<string> GenerateDistributedAuthenticationTokenAsync(string correlationID);
	}
}