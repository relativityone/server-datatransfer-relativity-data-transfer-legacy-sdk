using System;

namespace Relativity.Data.MassImport
{
	// TODO: adjust namespace, https://jira.kcura.com/browse/REL-477112 
	internal class TableNames
	{
		private const string IMAGE_TEMP_TABLE_PREFIX = "RELIMGTMP_";
		private const string IMAGE_INSERTED_ARTIFACTS_TABLE_PREFIX = "RELIMGTMPART_";
		private const string NATIVE_TEMP_TABLE_PREFIX = "RELNATTMP_";
		private const string FULLTEXT_TEMP_TABLE_PREFIX = "RELFTTMP_";
		private const string NATIVE_TEMP_PART_TABLE_PREFIX = "RELNATTMPPART_";
		private const string NATIVE_TEMP_PARENT_TABLE_PREFIX = "RELNATTMPPARENT_";
		private const string NATIVE_TEMP_TABLE_CODES_PREFIX = "RELNATTMPCOD_";
		private const string NATIVE_TEMP_TABLE_OBJECTS_PREFIX = "RELNATTMPOBJ_";
		private const string NATIVE_TEMP_TABLE_MAP_PREFIX = "RELNATTMPMAP_";
		private const string COLUMN_DEFINTION_TABLE_PREFIX = "RELCOLUMNDEF_";
		private const string PARENT_ANCESTORS_TABLE_PREFIX = "RELNATPARENTACENSTORS_";
		private const string NEW_ANCESTORS_TABLE_PREFIX = "RELNATNEWANCESTORS_";

		public TableNames() : this(null)
		{
		}

		public TableNames(string runId)
		{
			if (string.IsNullOrWhiteSpace(runId))
			{
				runId = Guid.NewGuid().ToString().Replace("-", "_");
			}

			RunId = runId;
		}

		public string Native => NATIVE_TEMP_TABLE_PREFIX + RunId;

		public string FullText => FULLTEXT_TEMP_TABLE_PREFIX + RunId;

		public string Code => NATIVE_TEMP_TABLE_CODES_PREFIX + RunId;

		public string Objects => NATIVE_TEMP_TABLE_OBJECTS_PREFIX + RunId;

		public string Image => IMAGE_TEMP_TABLE_PREFIX + RunId;

		public string Part => NATIVE_TEMP_PART_TABLE_PREFIX + RunId;

		public string Parent => NATIVE_TEMP_PARENT_TABLE_PREFIX + RunId;

		public string ImagePart => IMAGE_INSERTED_ARTIFACTS_TABLE_PREFIX + RunId;

		public string Map => NATIVE_TEMP_TABLE_MAP_PREFIX + RunId;

		public string ParentAncestors => PARENT_ANCESTORS_TABLE_PREFIX + RunId;

		public string NewAncestors => NEW_ANCESTORS_TABLE_PREFIX + RunId;

		//public string ParentAncestors => 

		public string RunId { get; private set; }

		/// <summary>
		/// 		''' Gets the name of all the temp tables that can be created during the
		/// 		''' import process given the specified run id.
		/// 		''' </summary>
		/// 		''' <param name="runID">The run id to get the tables for</param>
		/// 		''' <returns>An array of possible temp table names</returns>
		public static string[] GetAllTempTableNames(string runID)
		{
			return new string[] { IMAGE_TEMP_TABLE_PREFIX + runID, IMAGE_INSERTED_ARTIFACTS_TABLE_PREFIX + runID, NATIVE_TEMP_TABLE_PREFIX + runID, NATIVE_TEMP_PARENT_TABLE_PREFIX + runID, NATIVE_TEMP_PART_TABLE_PREFIX + runID, NATIVE_TEMP_PART_TABLE_PREFIX + runID + "_ANSII", NATIVE_TEMP_PART_TABLE_PREFIX + runID + "_UNICODE", NATIVE_TEMP_TABLE_CODES_PREFIX + runID, NATIVE_TEMP_TABLE_OBJECTS_PREFIX + runID, NATIVE_TEMP_TABLE_MAP_PREFIX + runID, FULLTEXT_TEMP_TABLE_PREFIX + runID };
		}

		/// <summary>
		/// 		''' Gets name of cached/auxiliary-type tables that are created during import process
		/// 		''' </summary>
		/// 		''' <param name="runId">The run id to get the tables for</param>
		/// 		''' <returns>An array of possible cached/auxiliary-type names</returns>
		public static string[] GetAllAuxiliaryTableNames(string runId)
		{
			return new string[] { COLUMN_DEFINTION_TABLE_PREFIX + runId };
		}
	}
}