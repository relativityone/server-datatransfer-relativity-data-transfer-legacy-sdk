using System;
using System.Data;

namespace Relativity.MassImport.Data.Cache
{
	internal class ColumnDefinitionInfo
	{
		public string CollationName { get; set; }

		public string UnicodeMarker => IsUnicodeEnabled ? "N" : string.Empty;

		public bool IsUnicodeEnabled { get; set; }
		public int FieldLength { get; set; }
		public int OriginalArtifactID { get; set; }
		public bool? OverlayMergeValues { get; set; }
		public string ColumnName { get; set; }
		public string LinkArg { get; set; }
		public string ObjectTypeName { get; set; }
		public int ObjectTypeDescriptorArtifactTypeID { get; set; }
		public string ContainingObjectField { get; set; }
		public string NewObjectField { get; set; }
		public string RelationalTableSchemaName { get; set; }
		public int AssociatedParentID { get; set; }
		public int TopLevelParentArtifactId { get; set; }
		public int TopLevelParentAccessControlListId { get; set; }

		public ColumnDefinitionInfo()
		{
		}

		public ColumnDefinitionInfo(DataRow row)
		{
			var collation = row["CollationName"];
			CollationName = ReferenceEquals(collation, DBNull.Value) ? string.Empty : collation.ToString();
			var isUnicodeEnabledField = row["UseUnicodeEncoding"];
			IsUnicodeEnabled = !ReferenceEquals(isUnicodeEnabledField, DBNull.Value) && Convert.ToBoolean(isUnicodeEnabledField);
			var actualFieldLength = row["ActualFieldLength"];
			FieldLength = ReferenceEquals(actualFieldLength, DBNull.Value) ? 0 : Convert.ToInt32(actualFieldLength);
			OriginalArtifactID = Convert.ToInt32(row["OriginalArtifactID"]);
			var actualOverlayMergeValues = row["OverlayMergeValues"];
			OverlayMergeValues = ReferenceEquals(actualOverlayMergeValues, DBNull.Value) ? default : Convert.ToBoolean(actualOverlayMergeValues);
			var columnNameField = row["ActualColumnName"];
			ColumnName = ReferenceEquals(columnNameField, DBNull.Value) ? string.Empty : columnNameField.ToString();
			var linkArgField = row["ActualLinkArg"];
			LinkArg = ReferenceEquals(linkArgField, DBNull.Value) ? string.Empty : linkArgField.ToString();
			var objectTypeNameField = row["ObjectTypeName"];
			ObjectTypeName = ReferenceEquals(objectTypeNameField, DBNull.Value) ? string.Empty : objectTypeNameField.ToString();
			var objectTypeArtifactTypeIdField = row["ObjectTypeArtifactTypeID"];
			ObjectTypeDescriptorArtifactTypeID = ReferenceEquals(objectTypeArtifactTypeIdField, DBNull.Value) ? 0 : Convert.ToInt32(objectTypeArtifactTypeIdField);
			var containingObjectFieldIdField = row["ContainingObjectField"];
			ContainingObjectField = ReferenceEquals(containingObjectFieldIdField, DBNull.Value) ? string.Empty : containingObjectFieldIdField.ToString();
			var newObjectFieldIdField = row["NewObjectField"];
			NewObjectField = ReferenceEquals(newObjectFieldIdField, DBNull.Value) ? string.Empty : newObjectFieldIdField.ToString();
			var RelationalTableSchemaNameField = row["RelationalTableSchemaName"];
			RelationalTableSchemaName = ReferenceEquals(RelationalTableSchemaNameField, DBNull.Value) ? string.Empty : RelationalTableSchemaNameField.ToString();
			var associatedParentIdField = row["AssociatedParentID"];
			AssociatedParentID = ReferenceEquals(associatedParentIdField, DBNull.Value) ? 0 : Convert.ToInt32(associatedParentIdField);
			var topLevelParentArtifactIdField = row["TopLevelParentArtifactId"];
			TopLevelParentArtifactId = ReferenceEquals(topLevelParentArtifactIdField, DBNull.Value) ? 0 : Convert.ToInt32(topLevelParentArtifactIdField);
			var topLevelParentAccessControlListIdField = row["TopLevelParentAccessControlListId"];
			TopLevelParentAccessControlListId = ReferenceEquals(topLevelParentAccessControlListIdField, DBNull.Value) ? 0 : Convert.ToInt32(topLevelParentAccessControlListIdField);
		}

		public string GetColumnDescription(FieldInfo mappedField)
		{
			switch (mappedField.Type)
			{
				case FieldTypeHelper.FieldType.Boolean:
					{
						return "BIT NULL";
					}
				case FieldTypeHelper.FieldType.Code:
					{
						return $"NVARCHAR(200) COLLATE {CollationName} NULL";
					}
				case FieldTypeHelper.FieldType.Currency:
				case FieldTypeHelper.FieldType.Decimal:
					{
						return "DECIMAL(17,2) NULL";
					}
				case FieldTypeHelper.FieldType.Date:
					{
						return "DATETIME NULL";
					}
				case FieldTypeHelper.FieldType.File:
					{
						return string.Empty;
					}
				case FieldTypeHelper.FieldType.Object:
					{
						return $"{UnicodeMarker}VARCHAR({FieldLength}) COLLATE {CollationName} NULL";
					}
				case FieldTypeHelper.FieldType.Objects:
					{
						return "NVARCHAR(MAX) NULL";
					}
				case FieldTypeHelper.FieldType.Integer:
					{
						return "INT NULL";
					}
				case FieldTypeHelper.FieldType.MultiCode:
					{
						return $"NVARCHAR(MAX) COLLATE {CollationName} NULL";
					}
				case FieldTypeHelper.FieldType.Text:
				case FieldTypeHelper.FieldType.OffTableText:
					{
						// BIGDATA_ET_1037770, BIGDATA_ET_1037769
						return $"{UnicodeMarker}VARCHAR(MAX) COLLATE {CollationName} NULL";
					}
				case FieldTypeHelper.FieldType.User:
					{
						return "INT NULL";
					}
				case FieldTypeHelper.FieldType.Varchar:
					{
						return $"{UnicodeMarker}VARCHAR({FieldLength}) COLLATE {CollationName} NULL";
					}
				default:
					{
						throw new ArgumentException("No definition for field type!!");
					}
			}
		}
	}
}