namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class Field : Artifact
	{
		public Field()
		{
			DisplayName = string.Empty;
			IsUpdatable = true;
			IsVisible = true;
			IsSortable = true;
			RelativityApplications = new int[0];
			ArtifactTypeID = 14;
		}

		public int FieldArtifactTypeID { get; set; }

		public string DisplayName { get; set; }

		public int FieldTypeID { get; set; }

		public FieldType FieldType { get; set; }

		public int FieldCategoryID { get; set; }

		public FieldCategory FieldCategory { get; set; }

		public int ArtifactViewFieldID { get; set; }

		public int? CodeTypeID { get; set; }

		public int? MaxLength { get; set; }

		public bool IsRequired { get; set; }

		public bool IsRemovable { get; set; }

		public bool IsEditable { get; set; }

		public bool IsUpdatable { get; set; }

		public bool IsVisible { get; set; }

		public bool IsArtifactBaseField { get; set; }

		public object Value { get; set; }

		public string TableName { get; set; }

		public string ColumnName { get; set; }

		public bool IsReadOnlyInLayout { get; set; }

		public string FilterType { get; set; }

		public int FieldDisplayTypeID { get; set; }

		public int Rows { get; set; }

		public bool IsLinked { get; set; }

		public string FormatString { get; set; }

		public int? RepeatColumn { get; set; }

		public int? AssociativeArtifactTypeID { get; set; }

		public bool IsAvailableToAssociativeObjects { get; set; }

		public bool IsAvailableInChoiceTree { get; set; }

		public bool IsGroupByEnabled { get; set; }

		public bool IsIndexEnabled { get; set; }

		public string DisplayValueTrue { get; set; }

		public string DisplayValueFalse { get; set; }

		public string Width { get; set; }

		public bool Wrapping { get; set; }

		public int LinkLayoutArtifactID { get; set; }

		public string NameValue { get; set; }

		public bool LinkType { get; set; }

		public bool UseUnicodeEncoding { get; set; }

		public bool AllowHtml { get; set; }

		public bool IsSortable { get; set; }

		public string FriendlyName { get; set; }

		public ImportBehaviorChoice? ImportBehavior { get; set; }

		public bool EnableDataGrid { get; set; }

		public bool? OverlayBehavior { get; set; }

		public int? RelationalIndexViewArtifactID { get; set; }

		public ObjectsFieldParameters ObjectsFieldArgs { get; set; }

		public bool AllowGroupBy { get; set; }

		public bool AllowPivot { get; set; }

		public int? PopupPickerView { get; set; }

		public int? FieldTreeView { get; set; }

		public KeyboardShortcut KeyboardShortcut { get; set; }

		public bool AvailableInViewer { get; set; }

		public int[] RelativityApplications { get; set; }

		public RelationalFieldPane RelationalPane { get; set; }

		public bool AutoAddChoices { get; set; }

		public bool IsReflected { get; set; }

		public bool IsSystemField { get; set; }

		public bool IsSystemOrRelationalField { get; set; }

		public bool IsRelationalField { get; set; }
	}
}