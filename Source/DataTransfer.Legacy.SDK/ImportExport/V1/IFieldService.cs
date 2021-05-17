using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	/// <summary>
	/// this consolidates FieldManager and FieldQuery
	/// </summary>
	[WebService("Field Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("field")]
	public interface IFieldService : IDisposable
	{
		[HttpPost]
		Task<Field> ReadAsync(int workspaceID, int fieldArtifactID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveAllMappableAsync(int workspaceID, int artifactTypeID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrievePotentialBeginBatesFieldsAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<bool> IsFieldIndexedAsync(int workspaceID, int fieldArtifactID, string correlationID);
	}
}