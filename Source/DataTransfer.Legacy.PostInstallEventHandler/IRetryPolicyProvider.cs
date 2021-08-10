namespace DataTransfer.Legacy.PostInstallEventHandler
{
	using System.Runtime.CompilerServices;
	using Polly;

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