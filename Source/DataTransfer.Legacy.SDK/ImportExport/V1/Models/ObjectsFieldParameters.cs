using System;
using System.Collections.Generic;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ObjectsFieldParameters
	{
		public ObjectsFieldParameters()
		{
			ManageFieldExposureForSingleObjectField = true;
			SiblingFieldGuids = new List<Guid>();
			CreateForeignKeys = true;
			ManageFieldExposure = true;
		}

		public bool ManageFieldExposureForSingleObjectField { get; set; }

		public string SiblingFieldName { get; set; }

		public string FieldSchemaColumnName { get; set; }

		public string SiblingFieldSchemaColumnName { get; set; }

		public List<Guid> SiblingFieldGuids { get; set; }

		public string RelationalTableSchemaName { get; set; }

		public bool CreateForeignKeys { get; set; }

		public bool ManageFieldExposure { get; set; }
	}
}