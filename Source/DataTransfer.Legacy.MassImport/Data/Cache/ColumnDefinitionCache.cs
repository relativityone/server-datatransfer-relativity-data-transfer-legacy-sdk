using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Utility.Extensions;

namespace Relativity.MassImport.Data.Cache
{
	internal class ColumnDefinitionCache : IColumnDefinitionCache
	{
		private readonly Dictionary<int, ColumnDefinitionInfo> _lookup = new Dictionary<int, ColumnDefinitionInfo>();
		private readonly kCura.Data.RowDataGateway.BaseContext _context;

		// "faked" field ID that is used to store global level database objects ids
		private const int GlobalFieldId = -1;

		public ColumnDefinitionCache(kCura.Data.RowDataGateway.BaseContext context)
		{
			_context = context;
		}

		public int TopLevelParentArtifactId => _lookup[GlobalFieldId].TopLevelParentArtifactId;

		public int TopLevelParentAccessControlListId => _lookup[GlobalFieldId].TopLevelParentAccessControlListId;

		public ColumnDefinitionInfo this[int artifactID] => _lookup[artifactID];

		public void ValidateFieldMapping(FieldInfo[] fieldInfoList)
		{
			foreach (FieldInfo field in fieldInfoList)
			{
				if (ShouldFieldBeInCache(field) & !_lookup.ContainsKey(field.ArtifactID))
				{
					throw new KeyNotFoundException($"The Field with ArtifactId: {field.ArtifactID} does not exist. Please check your fields mapping settings");
				}
			}
		}

		public void InitializeCache(FieldInfo[] mappedFields, string runId)
		{
			var ids = mappedFields.Select(m => m.ArtifactID).ToArray();

			string populateCacheSql = PopulateColumnDefinitionCacheSql(_context, ids, runId);
			_context.ExecuteNonQuerySQLStatement(populateCacheSql, kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout);
		}

		public ColumnDefinitionCache LoadDataFromCache(string runId)
		{
			_lookup.Clear();

			string readCachedDataSql = GetCachedColumnDefinitionSql(runId);
			var dt = _context.ExecuteSqlStatementAsDataTable(readCachedDataSql, kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout);

			foreach (DataRow row in dt.Rows)
			{
				var info = new Cache.ColumnDefinitionInfo(row);
				_lookup.Add(info.OriginalArtifactID, info);
			}

			return this;
		}

		// TODO: In query for objects 'IN (10,13)' should work instead of 'NOT IN (5,8)'.
		private static string PopulateColumnDefinitionCacheSql(kCura.Data.RowDataGateway.BaseContext context, int[] ids,
			string runId)
		{
			return $@"
IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{GetColumnDefinitionTableName(runId)}')
BEGIN

CREATE TABLE [Resource].[{GetColumnDefinitionTableName(runId)}] (
	[CollationName] NVARCHAR(128),
	[ActualFieldLength] INT,
	[UseUnicodeEncoding] INT,
	[OriginalArtifactID] INT,
	[OverlayMergeValues] INT,
	[ActualColumnName] VARCHAR(100),
	[ActualLinkArg] VARCHAR(50),
	[ObjectTypeName] NVARCHAR(255),
	[ObjectTypeArtifactTypeID] INT,
	[ContainingObjectField] NVARCHAR(128),
	[NewObjectField] NVARCHAR(128),
	[RelationalTableSchemaName] NVARCHAR(128),
	[AssociatedParentID] INT,
	[TopLevelParentArtifactID] INT,
	[TopLevelParentAccessControlListID] INT
);

DECLARE @parent int;
DECLARE @parentAccessControlListID int;

SELECT @parent = [ArtifactId], @parentAccessControlListID = [AccessControlListID]
FROM [Artifact] 
WHERE [ParentArtifactID] is NULL AND [ArtifactTypeID] = {(int) Relativity.ArtifactType.Case};

INSERT INTO [Resource].[{GetColumnDefinitionTableName(runId)}] ([CollationName], [ActualFieldLength], [UseUnicodeEncoding], [OriginalArtifactId], [OverlayMergeValues], [ActualColumnName], [ActualLinkArg], [ObjectTypeName], [ObjectTypeArtifactTypeID], [ContainingObjectField], [NewObjectField], [RelationalTableSchemaName], [AssociatedParentID])
SELECT sc.collation_name, ActualFieldLength, UseUnicodeEncoding, OriginalArtifactID, [OverlayMergeValues], [ActualColumnName], [ActualLinkArg], [ObjectTypeName], [ObjectTypeArtifactTypeID], [ContainingObjectField], [NewObjectField], [RelationalTableSchemaName], [AssociatedParentID] FROM(
-- 1. Choices
(SELECT
	OriginalArtifactID = Field.ArtifactID,
    ObjectID = OBJECT_ID('EDDSDBO.[Code]'),
    ActualArtifactID = [Field].[ArtifactID],
    ActualColumnName = 'Name',
	ActualFieldLength = 200,
	UseUnicodeEncoding = 1,
	OverlayMergeValues = [Field].[OverlayMergeValues],
	ActualLinkArg = NULL,
	ObjectTypeName = NULL,
	ObjectTypeArtifactTypeID = NULL,
	ContainingObjectField = NULL,
	NewObjectField = NULL,
	RelationalTableSchemaName = NULL,
	AssociatedParentID = NULL
FROM 
    [Field]
Where [Field].FieldTypeID IN (5,8)
)
-- 2. Simple types
UNION ALL
(
SELECT
	OriginalArtifactID = Field.ArtifactID,
    ObjectID = OBJECT_ID('EDDSDBO.[' + ArtifactType.ArtifactType + ']'),
    ActualArtifactID =  [Field].[ArtifactID],
    ActualColumnName = ArtifactViewField.ColumnName,
	ActualFieldLength = [Field].[Maxlength],
	UseUnicodeEncoding = [Field].UseUnicodeEncoding,
	OverlayMergeValues = [Field].[OverlayMergeValues],
	ActualLinkArg = [ArtifactViewField].[LinkArg],
	ObjectTypeName = NULL,
	ObjectTypeArtifactTypeID = NULL,
	ContainingObjectField = NULL,
	NewObjectField = NULL,
	RelationalTableSchemaName = NULL,
	AssociatedParentID = NULL
FROM 
    [Field]
INNER JOIN
    ArtifactViewField ON ArtifactViewField.ArtifactViewFieldID = [Field].[ArtifactViewFieldID]
INNER JOIN
    ArtifactType ON ArtifactType.ArtifactTypeID = [field].FieldArtifactTypeID
WHERE [Field].AssociativeArtifactTypeID IS NULL
	AND [Field].FieldTypeID NOT IN (5,8) 
)
-- 3. Objects
UNION ALL
(
SELECT
	OriginalArtifactID = Field.ArtifactID,
    ObjectID = OBJECT_ID('EDDSDBO.[' + AssociativeArtifactType.ArtifactType + ']'),
    ActualArtifactID = AssociativeField.ArtifactID,
    ActualColumnName = AssociativeArtifactViewField.ColumnName,
	ActualFieldLength = AssociativeField.[Maxlength],
	UseUnicodeEncoding = AssociativeField.UseUnicodeEncoding,
	OverlayMergeValues = [Field].[OverlayMergeValues],
	ActualLinkArg = [AssociativeArtifactViewField].[LinkArg],
	ObjectTypeName = [ObjT].[Name],
	ObjectTypeArtifactTypeID = [Field].AssociativeArtifactTypeID,
	ContainingObjectField = IIF([Field].[ArtifactID] = ofr.[FieldArtifactId1], ofr.[RelationalTableFieldColumnName1], ofr.[RelationalTableFieldColumnName2]),
	NewObjectField = IIF([Field].[ArtifactID] = ofr.[FieldArtifactId1], ofr.[RelationalTableFieldColumnName2], ofr.[RelationalTableFieldColumnName1]),
	RelationalTableSchemaName = ofr.[RelationalTableSchemaName],
	AssociatedParentID = ObjT.[ParentArtifactTypeID]
FROM 
    [Field]
INNER JOIN
    ArtifactViewField ON ArtifactViewField.ArtifactViewFieldID = [Field].[ArtifactViewFieldID]
INNER JOIN
    ArtifactType AssociativeArtifactType ON AssociativeArtifactType.ArtifactTypeID = [Field].[AssociativeArtifactTypeID]
INNER JOIN 
    Field AssociativeField ON AssociativeField.FieldArtifactTypeID = [Field].[AssociativeArtifactTypeID]
    AND
    AssociativeField.FieldCategoryID = 2
INNER JOIN 
    ArtifactViewField AssociativeArtifactViewField ON AssociativeArtifactViewField.ArtifactViewFieldID = AssociativeField.ArtifactViewFieldID
INNER JOIN
	ObjectType ObjT ON Field.AssociativeArtifactTypeID = ObjT.DescriptorArtifactTypeID
LEFT OUTER JOIN
	ObjectsFieldRelation ofr ON Field.ArtifactID = ofr.FieldArtifactId1 OR Field.ArtifactID = ofr.FieldArtifactId2
WHERE 
	[Field].FieldTypeID NOT IN (5,8) 
)
) t
INNER JOIN 
    sys.columns sc ON 
		sc.object_id = t.ObjectID 
		AND 
		sc.[name] = t.ActualColumnName COLLATE DATABASE_DEFAULT
WHERE
    t.OriginalArtifactID IN ({ids.ToCsv()});

INSERT INTO [Resource].[{GetColumnDefinitionTableName(runId)}] (OriginalArtifactId, TopLevelParentArtifactId, TopLevelParentAccessControlListId) Values({GlobalFieldId}, @parent, @parentAccessControlListID);
END";
		}

		internal static string GetCachedColumnDefinitionSql(string runId)
		{
			return $@"
				SELECT [CollationName], [ActualFieldLength], [UseUnicodeEncoding], [OriginalArtifactID], [OverlayMergeValues], [ActualColumnName], [ActualLinkArg], [ObjectTypeName], [ObjectTypeArtifactTypeID], [ContainingObjectField], [NewObjectField], [RelationalTableSchemaName], [AssociatedParentID], [TopLevelParentArtifactID], [TopLevelParentAccessControlListID]
				FROM [Resource].[{ Constants.COLUMN_DEFINTION_TABLE_PREFIX + runId }]";
		}

		private static string GetColumnDefinitionTableName(string runId)
		{
			return Constants.COLUMN_DEFINTION_TABLE_PREFIX + runId;
		}

		private bool ShouldFieldBeInCache(FieldInfo field)
		{
			// Those fields should not be in the cache
			return !field.EnableDataGrid && field.Type != FieldTypeHelper.FieldType.LayoutText && field.Type != FieldTypeHelper.FieldType.File;
		}
	}
}