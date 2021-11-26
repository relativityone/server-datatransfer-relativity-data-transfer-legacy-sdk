using System.Collections.Generic;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data.StagingTables
{
	internal interface IStagingTableRepository
	{
		bool StagingTablesExist();
		void CreateStagingTables(ColumnDefinitionCache columnDefinitionCache, NativeLoadInfo settings, bool includeExtractedTextEncoding, bool excludeFolderPathForOldClient);
		void TruncateStagingTables(FieldInfo[] mappedFields, bool loadImportedFullTextFromServer);
		string BulkInsert(NativeLoadInfo settings, string bulkFileSharePath);
		string Insert(ColumnDefinitionCache columnDefinitionCache, NativeLoadInfo settings, bool excludeFolderPathForOldClient);
		
		/// <summary>
		/// This method returns number of choices per CodeTypeId.
		/// </summary>
		/// <returns>Mapping from CodeTypeId to a number of choices of that type.</returns>
		IDictionary<int, int> ReadNumberOfChoicesPerCodeTypeId();
	}
}