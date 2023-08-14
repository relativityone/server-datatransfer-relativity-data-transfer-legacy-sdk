using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.Server;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal static class SqlDataRecordEnumerable
	{
		/// <summary>
		/// Warning. Do not call ToArray, ToList methods on this because you will get N records, all with the same, duplicated values.
		/// </summary>
		public static IEnumerable<SqlDataRecord> GetFolderCandidates(this IEnumerable<FolderNode> folderNodes)
		{
			// The sqlDataRecord is reused. This is on purpose. Per MSDN, creating SqlDataRecord is expensive and this is the preferable way.
			var sqlDataRecord = new SqlDataRecord(new SqlMetaData("TempArtifactID", SqlDbType.Int), new SqlMetaData("TempParentArtifactID", SqlDbType.Int), new SqlMetaData("Name", SqlDbType.NVarChar, 255L));

			foreach (FolderNode folderNode in folderNodes)
			{
				sqlDataRecord.SetInt32(0, folderNode.TempArtifactID);
				sqlDataRecord.SetInt32(1, folderNode.Parent.TempArtifactID);
				sqlDataRecord.SetString(2, folderNode.Name);
				yield return sqlDataRecord;
			}
		}

		/// <summary>
		/// Warning. Do not call ToArray, ToList methods on this because you will get N records, all with the same, duplicated values.
		/// </summary>
		public static IEnumerable<SqlDataRecord> GetImportMapping(this IEnumerable<FolderNode> folderNodes, IEnumerable<FolderArtifactIDMapping> folderArtifactIdMappings)
		{
			// The sqlDataRecord is reused. This is on purpose. Per MSDN, creating SqlDataRecord is expensive and this is the preferable way.
			var sqlDataRecord = new SqlDataRecord(new SqlMetaData("ImportID", SqlDbType.Int), new SqlMetaData("ParentArtifactID", SqlDbType.Int));

			var dictionary = folderArtifactIdMappings.ToDictionary(p => p.TempArtifactID);

			return folderNodes.SelectMany(node => node.LeafIDs.Select(leafID =>
			{
				sqlDataRecord.SetInt32(0, leafID);
				sqlDataRecord.SetInt32(1, dictionary[node.TempArtifactID].ArtifactID);
				return sqlDataRecord;
			}));
		}
	}
}