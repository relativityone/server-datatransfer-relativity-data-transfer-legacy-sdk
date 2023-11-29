using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.DataGrid.Interfaces.DGFS;
using Relativity.MassImport.Data.DataGrid;

namespace Relativity.MassImport.Data.DataGrid
{
	internal class DGFieldInformationLookupFactory : IFieldInformationLookupFactory
	{
		private IFieldInformationLookupFactory fieldInformationLookupFactory;

		public IEnumerable<DGImportFileInfo> DeleteList { get; set; }

		public DGFieldInformationLookupFactory(IFieldInformationLookupFactory fieldInformationLookupFactory)
		{
			this.fieldInformationLookupFactory = fieldInformationLookupFactory;
		}

		public Task<IFieldInformationLookup> BuildLookup(string indexName, IEnumerable<string> fieldNames,
			IEnumerable<string> datagridIDs, IArtifactMappingLookup artifactMappingLookup)
		{
			return fieldInformationLookupFactory.BuildLookup(indexName, fieldNames, datagridIDs, artifactMappingLookup);
		}

		public async Task<IFieldInformationLookup> BuildLookup(string indexName, IEnumerable<int> fieldArtifactIDs,
			IEnumerable<int> artifactIDs)
		{
			if (DeleteList != null)
			{
				return new DGFieldInformationLookup(DeleteList);
			}
			else
			{
				return await fieldInformationLookupFactory.BuildLookup(indexName, fieldArtifactIDs, artifactIDs);
			}
		}
	}
}