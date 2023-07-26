using System.Collections.Generic;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data.StagingTables
{
	using Relativity.Logging;
	using Relativity.MassImport.DTO;

	internal interface IStagingTableRepository
	{
		bool StagingTablesExist();
		void CreateStagingTables(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool includeExtractedTextEncoding, bool excludeFolderPathForOldClient);
		void TruncateStagingTables(FieldInfo[] mappedFields, bool loadImportedFullTextFromServer);
		string BulkInsert(Relativity.MassImport.DTO.NativeLoadInfo settings, string bulkFileSharePath, ILog logger);
		string Insert(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool excludeFolderPathForOldClient);
		
		/// <summary>
		/// This method returns number of choices per CodeTypeId.
		/// </summary>
		/// <returns>Mapping from CodeTypeId to a number of choices of that type.</returns>
		IDictionary<int, int> ReadNumberOfChoicesPerCodeTypeId();
	}
}