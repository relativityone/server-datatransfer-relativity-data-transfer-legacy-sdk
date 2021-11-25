using System.Data;
using Relativity.Logging;

namespace Relativity.MassImport.Data
{
	internal interface IObjectBase
	{
		int QueryTimeout { get; set; }
		string TableNameNativeTemp { get; }
		string TableNameFullTextTemp { get; }
		string TableNameCodeTemp { get; }
		string TableNameObjectsTemp { get; }
		string RunID { get; }
		ImportMeasurements ImportMeasurements { get; }

		void CreateAssociatedObjects(int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit);
		void ProcessMultiObjectField(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit);
		void ProcessSingleObjectField(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit);
		void CreateAssociatedObjectsForSingleObjectFieldByName(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit);
		void VerifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactId(FieldInfo field, int userID, int? auditUserId);
		void CreateAssociatedObjectsForMultiObjectFieldByName(FieldInfo field, int userID, string requestOrigination, string recordOrigination, bool performAudit);
		void VerifyExistenceOfAssociatedObjectsForMultiObjectByArtifactId(FieldInfo field, int userID, int? auditUserId);
		int IncomingObjectCount();
		void UpdateFullTextFromFileShareLocation();
		void PopulateArtifactIdOnInitialTempTable(int userID, bool updateOverlayPermissions);
		void PopulateObjectsListTable();
		IDataReader CreateDataGridMappingDataReader();
		void WriteToDataGrid(DataGridReader loader, int appID, string bulkFileShareFolderPath, ILog correlationLogger);
		void MapDataGridRecords(ILog correlationLogger);
		string GetTempTableName();
	}
}