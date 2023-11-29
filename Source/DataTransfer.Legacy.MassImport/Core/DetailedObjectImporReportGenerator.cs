using System.Collections.Generic;
using System.Data;
using Relativity.Core.Service;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal class DetailedObjectImporReportGenerator
	{
		public const string ID_COLUMN_NAME = "ArtifactID";
		public const string KEY_FIELD_COLUMN_NAME = "KeyFieldName";
		public const string ISNEW_COLUMN_NAME = "IsNew";
		public const string FILE_COUNT_COLUMN_NAME = "FileCount";

		public static MassImportManagerBase.MassImportResults PopulateResultsObject(Objects importObject)
		{
			var dt = importObject.GetReturnReportData(true);
			var result = new MassImportManagerBase.MassImportResults();
			return PopulateResultsObject(dt, result);
		}

		private static MassImportManagerBase.MassImportResults PopulateResultsObject(DataTable dt, MassImportManagerBase.MassImportResults existingSettings)
		{
			var results = CreateDetailedMassImportResults(existingSettings);

			var affectedIds = new List<int>();
			var keyFieldToArtifactIDs = new Dictionary<string, List<int>>();
			foreach (DataRow row in dt.Rows)
			{
				int artifactId = (int) row[ID_COLUMN_NAME];
				string keyFieldName = (string) row[KEY_FIELD_COLUMN_NAME];
				if (keyFieldToArtifactIDs.ContainsKey(keyFieldName))
				{
					keyFieldToArtifactIDs[keyFieldName].Add(artifactId);
				}
				else
				{
					keyFieldToArtifactIDs.Add(keyFieldName, new List<int>() { artifactId });
				}

				affectedIds.Add(artifactId);
			}

			results.AffectedIDs = affectedIds.ToArray();
			results.KeyFieldToArtifactIDMapping = keyFieldToArtifactIDs;
			return results;
		}

		private static MassImportManagerBase.DetailedMassImportResults CreateDetailedMassImportResults(MassImportManagerBase.MassImportResults existingSettings)
		{
			var retval = new MassImportManagerBase.DetailedMassImportResults();
			if (existingSettings is object)
			{
				retval.ExceptionDetail = existingSettings.ExceptionDetail;
				retval.RunID = existingSettings.RunID;
			}

			return retval;
		}
	}
}