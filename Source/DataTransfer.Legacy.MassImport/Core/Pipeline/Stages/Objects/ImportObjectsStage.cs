using kCura.Utility;
using Relativity.Core.Service;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.Toggles;

namespace Relativity.MassImport.Core.Pipeline.Stages.Objects
{
	internal class ImportObjectsStage : Framework.IPipelineStage<Input.ObjectImportInput, MassImportManagerBase.MassImportResults>
	{
		private readonly MassImportContext _context;
		private readonly ILockHelper _lockHelper;

		public ImportObjectsStage(MassImportContext context, ILockHelper lockHelper)
		{
			_context = context;
			_lockHelper = lockHelper;
		}

		public MassImportManagerBase.MassImportResults Execute(ObjectImportInput input)
		{
			return AttemptRunObjectImportNew(input);
		}

		private MassImportManagerBase.MassImportResults AttemptRunObjectImportNew(ObjectImportInput input)
		{
			var settings = input.Settings;
			var queryExecutor = new QueryExecutor(_context.BaseContext.DBContext, _context.Logger);
			var importObject = new Data.Objects(_context.BaseContext.DBContext, queryExecutor, settings, (int)input.ImportUpdateAuditAction, _context.ImportMeasurements, input.ColumnDefinitionCache, _context.CaseSystemArtifactId);
			IChoicesImportService choicesImportService = CreateChoicesImportService(settings, input.ColumnDefinitionCache);
			var result = this.ExecuteObjectImport(settings, importObject, choicesImportService, input.CollectCreatedIDs);
			InjectionManager.Instance.Evaluate("b7e37d83-d252-4986-8362-390a7c0e299f");
			return result;
		}

		private MassImportManagerBase.MassImportResults ExecuteObjectImport(
			Relativity.MassImport.DTO.ObjectLoadInfo settings, 
			Data.Objects importObject,
			IChoicesImportService choicesImportService,
			bool collectCreatedIDs)
		{
			var sql = new SerialSqlQuery();
			sql.Add(PreparePopulatePartTableQuery(importObject));
			sql.Add(PrepareErrorUpdateQuery(settings, importObject));
			importObject.ExecuteQueryAndSendMetrics(new StatisticsTimeOnQuery(sql));
			int auditUserId = Relativity.Core.Service.Audit.ImpersonationToolkit.GetCaseAuditUserId(_context.BaseContext, settings.OnBehalfOfUserToken);
			bool isAppend = settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Append;
			bool isAppendOrOverlay = settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Both;
			bool isAppendOrBoth = isAppend || isAppendOrOverlay;
			int artifactsCreated = 0;

			_lockHelper.Lock(_context.BaseContext, MassImportManagerLockKey.LockType.Objects, () =>
			{
				importObject.CreateAssociatedObjects(_context.BaseContext.AclUserID, auditUserId,
					_context.BaseContext.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(),
					Relativity.Core.Config.AuditingEnabled);
				if (isAppendOrBoth)
				{
					artifactsCreated = importObject.CreateObjects(_context.BaseContext.AclUserID, auditUserId,
						_context.BaseContext.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(),
						Relativity.Core.Config.AuditingEnabled);
				}
			});

			bool isOverlay = settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Overlay;
			importObject.PopulateArtifactIdOnInitialTempTable(_context.BaseContext.UserID, isOverlay);
			
			_lockHelper.Lock(_context.BaseContext, MassImportManagerLockKey.LockType.Choice, () =>
			{
				choicesImportService.PopulateCodeArtifactTable();
			});

			importObject.PopulateObjectsListTable();
			int artifactsUpdated = 0;
			bool isOverlayOrBoth = isOverlay || isAppendOrOverlay;
			if (isOverlayOrBoth)
			{
				artifactsUpdated = importObject.UpdateObjectMetadata(_context.BaseContext.AclUserID, auditUserId, _context.BaseContext.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), Relativity.Core.Config.AuditingEnabled);
			}

			importObject.UpdateFullTextFromFileShareLocation();
			int filesProcessed = 0;
			if (settings.UploadFiles)
			{
				filesProcessed = importObject.PopulateFileTables(auditUserId, Relativity.Core.Config.AuditingEnabled, _context.BaseContext.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), _context.BaseContext.MasterDatabasePrependString);
			}

			importObject.ClearTempTableAndSaveErrors();
			var result = new MassImportManagerBase.MassImportResults();
			if (collectCreatedIDs)
			{
				result = DetailedObjectImporReportGenerator.PopulateResultsObject(importObject);
			}

			result.ArtifactsCreated = artifactsCreated;
			result.ArtifactsUpdated = artifactsUpdated;
			result.FilesProcessed = filesProcessed;
			result.RunID = settings.RunID;
			return result;
		}

		private SerialSqlQuery PreparePopulatePartTableQuery(Data.Objects importObject)
		{
			var sql = new SerialSqlQuery();
			sql.Add(new PrintSectionQuery(importObject.PopulatePartTable(), nameof(importObject.PopulatePartTable)));
			sql.Add(new PrintSectionQuery(importObject.PopulateParentTable(), nameof(importObject.PopulateParentTable)));
			return sql;
		}

		private SerialSqlQuery PrepareErrorUpdateQuery(Relativity.MassImport.DTO.ObjectLoadInfo settings, Data.Objects importObject)
		{
			var sql = new SerialSqlQuery();
			switch (settings.Overlay)
			{
				case Relativity.MassImport.DTO.OverwriteType.Append:
					{
						sql.Add(new PrintSectionQuery(importObject.ManageAppendErrors(), nameof(importObject.ManageAppendErrors)));
						break;
					}

				case Relativity.MassImport.DTO.OverwriteType.Overlay:
					{
						sql.Add(new PrintSectionQuery(importObject.ManageOverwriteErrors(), nameof(importObject.ManageOverwriteErrors)));
						break;
					}

				case Relativity.MassImport.DTO.OverwriteType.Both:
					{
						sql.Add(new PrintSectionQuery(importObject.ManageAppendOverlayParentMissingErrors(), nameof(importObject.ManageAppendOverlayParentMissingErrors)));
						break;
					}
			}

			if (!settings.DisableUserSecurityCheck)
			{
				sql.Add(new PrintSectionQuery(importObject.ManageCheckAddingPermissions(_context.BaseContext.AclUserID), nameof(importObject.ManageCheckAddingPermissions)));
				if (settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Both || settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Overlay)
				{
					sql.Add(new PrintSectionQuery(importObject.ManageUpdateOverlayPermissions(_context.BaseContext.AclUserID), nameof(importObject.ManageUpdateOverlayPermissions)));
				}
			}

			sql.Add(new PrintSectionQuery(
				importObject.ValidateIdentifiersAreNonEmpty(),
				nameof(importObject.ValidateIdentifiersAreNonEmpty)));

			return sql;
		}

		private void ThrowIfDocumentLimitExceeded(Relativity.Core.BaseContext context, ObjectBase importObject)
		{
			int documentImportCount = importObject.IncomingObjectCount();
			bool willExceedLimit = WillExceedDocumentLimit(context, documentImportCount);
			if (willExceedLimit)
			{
				throw new System.Exception("The document import was canceled. The import would have exceeded the document limit for the workspace.");
			}
		}

		private bool WillExceedDocumentLimit(Relativity.Core.BaseContext context, int documentImportCount)
		{
			bool willExceedLimit = false;
			int workspaceId = context.AppArtifactID;
			int currentDocCount = Relativity.Core.Service.DocumentManager.RetrieveCurrentDocumentCount(context, workspaceId);
			int countAfterImport = currentDocCount + documentImportCount;
			int docLimit = Relativity.Core.Service.DocumentManager.RetrieveDocumentLimit(context, workspaceId);
			if (docLimit != 0 & countAfterImport > docLimit)
			{
				willExceedLimit = true;
			}

			return willExceedLimit;
		}

		private IChoicesImportService CreateChoicesImportService(Relativity.MassImport.DTO.NativeLoadInfo settings, ColumnDefinitionCache columnDefinitionCache)
		{
			return new ChoicesImportService(
				_context.BaseContext.DBContext,
				ToggleProvider.Current,
				_context.JobDetails.TableNames,
				_context.ImportMeasurements,
				settings,
				columnDefinitionCache,
				Relativity.Data.Config.MassImportSqlTimeout);
		}
	}
}