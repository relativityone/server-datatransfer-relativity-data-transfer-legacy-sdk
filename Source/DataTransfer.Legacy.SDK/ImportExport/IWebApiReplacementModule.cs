using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport
{
	[ServiceModule("WebAPI replacement module")]
	[RoutePrefix("webapi-replacement", VersioningStrategy.Namespace)]
	public interface IWebApiReplacementModule
	{
	}
}