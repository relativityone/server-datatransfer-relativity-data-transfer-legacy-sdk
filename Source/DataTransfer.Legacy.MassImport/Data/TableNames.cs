using System;
using Relativity.MassImport.Data;

namespace Relativity.Data.MassImport
{
	// TODO: change to internal and adjust namespace, https://jira.kcura.com/browse/REL-477112 
	public class TableNames
	{
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

		public string Native => Relativity.MassImport.Constants.NATIVE_TEMP_TABLE_PREFIX + RunId;

		public string FullText => Relativity.MassImport.Constants.FULLTEXT_TEMP_TABLE_PREFIX + RunId;

		public string Code => Relativity.MassImport.Constants.NATIVE_TEMP_TABLE_CODES_PREFIX + RunId;

		public string Objects => Relativity.MassImport.Constants.NATIVE_TEMP_TABLE_OBJECTS_PREFIX + RunId;

		public string Image => Relativity.MassImport.Constants.IMAGE_TEMP_TABLE_PREFIX + RunId;

		public string Part => Relativity.MassImport.Constants.NATIVE_TEMP_PART_TABLE_PREFIX + RunId;

		public string Parent => Relativity.MassImport.Constants.NATIVE_TEMP_PARENT_TABLE_PREFIX + RunId;

		public string ImagePart => Relativity.MassImport.Constants.IMAGE_INSERTED_ARTIFACTS_TABLE_PREFIX + RunId;

		public string Map => Relativity.MassImport.Constants.NATIVE_TEMP_TABLE_MAP_PREFIX + RunId;

		public string RunId { get; private set; }
	}
}