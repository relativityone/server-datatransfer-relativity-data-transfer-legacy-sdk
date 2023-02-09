using Polly;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies
{
	internal interface IKeplerRetryPolicyFactory
	{
		IAsyncPolicy CreateRetryPolicy();
		IAsyncPolicy<T> CreateRetryPolicy<T>();
	}
}