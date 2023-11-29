using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.DataGrid.Interfaces.DGFS;
using Relativity.DataGrid.Interfaces.DGFS.DTOs;

namespace Relativity.MassImport.Data.DataGrid
{
	internal class DGFieldInformationLookup : IFieldInformationLookup
	{
		private readonly Dictionary<int, DGImportFileInfo> deleteDictionary;

		public DGFieldInformationLookup(IEnumerable<DGImportFileInfo> deleteList)
		{
			deleteDictionary = deleteList.ToDictionary(x => x.ImportId);
		}

		public Task<FieldInformation> RetrieveFieldInformation(int documentArtifactID, int fieldArtifactID)
		{
			var importFileInfo = deleteDictionary[documentArtifactID];
			var fieldInformation = new FieldInformation()
			{
				DocumentArtifactID = documentArtifactID,
				FieldArtifactID = fieldArtifactID,
				FilePath = new Uri(importFileInfo.FileLocation),
				FileSize = (long) importFileInfo.FileSize,
			};
			return Task.FromResult(fieldInformation);
		}

		public Task<IEnumerable<IGrouping<string, FieldInformation>>> GetAllByBackEnd()
		{
			var fileList = new List<FieldInformation>();
			foreach (DGImportFileInfo importFileInfo in deleteDictionary.Values)
			{
				fileList
					.Add(this.RetrieveFieldInformation(importFileInfo.ImportId, importFileInfo.FieldArtifactId)
					.GetAwaiter()
					.GetResult());
			}

			return Task.FromResult(fileList.GroupBy(x => Relativity.DataGrid.Constants.URI_FILE_SCHEME));
		}
	}
}