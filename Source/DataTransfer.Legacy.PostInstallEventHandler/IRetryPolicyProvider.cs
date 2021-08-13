using System.Runtime.CompilerServices;
using Polly;

namespace Relativity.DataTransfer.Legacy.PostInstallEventHandler
{
	public interface IRetryPolicyProvider
	{
		/// <summary>
		/// CreateInstanceSettingsTextType and return retry policy.
		/// </summary>
		/// <param name="operationName">Caller name.</param>
		/// <returns>Policy.</returns>
		Policy GetAsyncRetryPolicy([CallerMemberName] string operationName = "");
	}
}