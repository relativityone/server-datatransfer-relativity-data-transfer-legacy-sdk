using NUnit.Framework;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class HelperFixture
	{
		// A return value of "1", or a SQL statement that evaluates to "1", means the CodeArtifact values will be merged/appended.
		// A return value of "0", or a SQL statement that evaluates to "0", means that CodeArtifact values will be replaced.
		[TestCase(Relativity.MassImport.OverlayBehavior.MergeAll, FieldTypeHelper.FieldType.Code, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleChoice_Merge", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.MergeAll, FieldTypeHelper.FieldType.MultiCode, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiChoice_Merge", ExpectedResult = "1")]
		[TestCase(Relativity.MassImport.OverlayBehavior.MergeAll, FieldTypeHelper.FieldType.Object, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleObject_Merge", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.MergeAll, FieldTypeHelper.FieldType.Objects, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiObject_Merge", ExpectedResult = "1")]
		[TestCase(Relativity.MassImport.OverlayBehavior.ReplaceAll, FieldTypeHelper.FieldType.Code, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleChoice_Replace", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.ReplaceAll, FieldTypeHelper.FieldType.MultiCode, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiChoice_Replace", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.ReplaceAll, FieldTypeHelper.FieldType.Object, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleObject_Replace", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.ReplaceAll, FieldTypeHelper.FieldType.Objects, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiObject_Replace", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.UseRelativityDefaults, FieldTypeHelper.FieldType.Code, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleChoice_Default", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.UseRelativityDefaults, FieldTypeHelper.FieldType.MultiCode, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiChoice_Default", ExpectedResult = "(SELECT COALESCE([OverlayMergeValues],0) FROM [Field] WHERE [ArtifactID] = 1062899)")]
		[TestCase(Relativity.MassImport.OverlayBehavior.UseRelativityDefaults, FieldTypeHelper.FieldType.Object, "1062899", TestName = "GetFieldOverlaySwitchStatement_SingleObject_Default", ExpectedResult = "0")]
		[TestCase(Relativity.MassImport.OverlayBehavior.UseRelativityDefaults, FieldTypeHelper.FieldType.Objects, "1062899", TestName = "GetFieldOverlaySwitchStatement_MultiObject_Default", ExpectedResult = "(SELECT COALESCE([OverlayMergeValues],0) FROM [Field] WHERE [ArtifactID] = 1062899)")]
		public string GetFieldOverlaySwitchStatement_TestResult(Relativity.MassImport.OverlayBehavior overlayBehavior, FieldTypeHelper.FieldType fieldType, string fieldArtifactIDParam)
		{
			var settings = new NativeLoadInfo();
			settings.OverlayBehavior = overlayBehavior;
			string result = Relativity.Data.MassImportOld.Helper.GetFieldOverlaySwitchStatement(settings, fieldType, fieldArtifactIDParam);
			return result;
		}
	}
}