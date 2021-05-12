using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace RAPTemplate.Services
{
	[WebService("RAPTemplateEndpoint Manager")]
	[ServiceAudience(Audience.Testing)]
	public interface IRAPTemplateEndpointManager : IDisposable
	{
		Task<bool> RAPTemplateEndpoint();
	}
}
