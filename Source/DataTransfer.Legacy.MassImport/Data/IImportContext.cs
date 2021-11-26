using System;
using kCura.Data.RowDataGateway;

namespace Relativity.MassImport.Data
{
	internal interface IImportContext
	{
		Guid RunID { get; }
		BaseContext DBContext { get; }
		int UserID { get; }
		int WorkspaceID { get; }
		int RootArtifactID { get; }
	}
}