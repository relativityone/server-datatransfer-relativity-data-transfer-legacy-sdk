using System;
using System.Collections.Generic;
using System.Diagnostics;
using kCura.Utility;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Core.Service.MassImport;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Data;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core
{
	internal class MassImportManagerNew
	{
		private readonly ILockHelper _lockHelper;
		private bool CollectIDsOnCreate { get; set; }

		public const string TEMP_TABLE_PREFIX_FOR_FOLDER_CREATION = "RELNATTMP_";

		public MassImportManagerNew(ILockHelper lockHelper, bool collectIDsOnCreate = false) : base()
		{
			_lockHelper = lockHelper;
			CollectIDsOnCreate = collectIDsOnCreate;
		}

		protected IAPM APMClient
		{
			get
			{
				return Client.APMClient;
			}
		}

		private ILog _logger;

		protected ILog CorrelationLogger
		{
			get
			{
				if (_logger is null)
				{
					_logger = Log.Logger.ForContext("CorrelationID", Guid.NewGuid(), true);
				}

				return _logger;
			}

			set
			{
				_logger = value;
			}
		}

		public MassImportManagerBase.MassImportResults AttemptRunImageImport(Relativity.Core.BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, bool inRepository, Timekeeper timekeeper, MassImportManagerBase.MassImportResults retval)
		{
			InjectionManager.Instance.Evaluate("7f572655-d2d6-4084-8feb-243a1e060bf8");
			var massImportMetric = new MassImportMetrics(CorrelationLogger, APMClient);
			var image = new Data.Image(context.DBContext, settings);

			if (!SQLInjectionHelper.IsValidRunId(settings.RunID))
			{
				throw new System.Exception("Invalid RunId");
			}
			if (!SQLInjectionHelper.IsValidFileName(settings.BulkFileName))
			{
				throw new System.Exception("Invalid File Name");
			}
			if (image.HasDataGridWorkToDo && !image.IsDataGridInputValid())
			{
				throw new System.Exception("Invalid DataGridFileName");
			}

			var importStopWatch = new Stopwatch();
			importStopWatch.Start();

			var bulkFileShareFolderPath = string.IsNullOrEmpty(settings.BulkFileSharePath) ?
				context.GetBcpSharePath() :
				settings.BulkFileSharePath;

			if (settings.UseBulkDataImport)
			{
				settings.RunID = image.InitializeBulkTable(bulkFileShareFolderPath, this.CorrelationLogger);
			}
			else
			{
				settings.RunID = image.InitializeTempTable();
			}

			if (image.IsNewJob)
			{
				massImportMetric.SendJobStarted(settings, importType: Constants.ImportType.Images, system: Constants.SystemNames.Kepler);
			}

			timekeeper.MarkEnd("TempFileInitialization");
			if (image.IsDataGridInputValid())
			{
				var loader = image.CreateDataGridReader(bulkFileShareFolderPath, this.CorrelationLogger);
				image.WriteToDataGrid(loader, context.AppArtifactID, bulkFileShareFolderPath, this.CorrelationLogger);
				image.MapDataGridRecords(this.CorrelationLogger);
			}

			context.BeginTransaction();
			try
			{
				_lockHelper.Lock(context, MassImportManagerLockKey.LockType.DocumentOrImageOrProductionImage, () =>
				{
					image.ExistingFilesLookupInitialization();
					image.ImportMeasurements.StartMeasure(nameof(image.SetOutsideFieldName));
					image.SetOutsideFieldName = context.DBContext.ExecuteSqlStatementAsScalar(
						"SELECT TOP 1 ColumnName FROM ArtifactViewField INNER JOIN [Field] ON [Field].[ArtifactViewFieldID] = [ArtifactViewField].[ArtifactViewFieldID] AND [Field].[ArtifactID] = " +
						settings.KeyFieldArtifactID).ToString();
					image.ImportMeasurements.StopMeasure(nameof(image.SetOutsideFieldName));
					if (settings.Overlay == Relativity.MassImport.DTO.OverwriteType.Overlay)
					{
						image.ManageOverwriteErrors();
					}

					image.ManageBatesExistsErrors();
					image.ManageRedactionErrors();
					switch (settings.Overlay)
					{
						case Relativity.MassImport.DTO.OverwriteType.Append:
						{
							image.ManageAppendErrors();
							if (Relativity.Core.Config.EnforceDocumentLimit)
							{
								ThrowIfDocumentLimitExceeded(context, image);
							}

							image.CreateDocumentsFromImageFile(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled, false);
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, false);

							break;
						}

						case Relativity.MassImport.DTO.OverwriteType.Overlay:
						{
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, true);
							image.UpdateDocumentMetadata(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled);

							break;
						}

						case Relativity.MassImport.DTO.OverwriteType.Both:
						{
							if (Relativity.Core.Config.EnforceDocumentLimit)
							{
								ThrowIfDocumentLimitExceeded(context, image);
							}

							image.CreateDocumentsFromImageFile(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled, true);
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, false);
							image.UpdateDocumentMetadata(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled);

							break;
						}
					}

					switch (settings.Overlay)
					{
						case Relativity.MassImport.DTO.OverwriteType.Overlay:
						case Relativity.MassImport.DTO.OverwriteType.Both:
						{
							image.DeleteExistingImageFiles(context.UserID, Relativity.Core.Config.AuditingEnabled,
								context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination());

							break;
						}
					}

					if (settings.HasPDF == true)
					{
						image.ManageHasPDFs();
					}
					else
					{
						image.ManageHasImages();
					}
					image.CreateImageFileRows(context.UserID, Relativity.Core.Config.AuditingEnabled,
						context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), inRepository);
					image.UpdateImageCount();
					image.UpdateArtifactAuditColumns(context.UserID);

					if (settings.UploadFullText)
					{
						image.ManageImageFullText();
					}

					if (image.HasDataGridWorkToDo && image.IsDataGridInputValid())
					{
						timekeeper.MarkStart("UpdateDataGridMapping");
						image.UpdateDgFieldMappingRecords(image.DGRelativityRepository.ImportFileInfos,
							this.CorrelationLogger);
						timekeeper.MarkEnd("UpdateDataGridMapping");
					}

					// need to delete files from the file system that were NOT imported (ie - appending and documents already existed)
					if (inRepository)
					{
						image.DeleteFilesNotImported();
					}

					image.ClearTempTableAndSaveErrors();
					try
					{
						var counts = image.GetReturnReport();
						retval.ArtifactsCreated = counts[0];
						retval.ArtifactsUpdated = counts[1];
						retval.FilesProcessed = counts[2];
					}
					catch
					{
					}

					InjectionManager.Instance.Evaluate("8522f493-ea15-4409-b300-316723e784c2");
				});
				context.CommitTransaction();
			}
			catch
			{
				context.RollbackTransaction();
				retval.RunID = settings.RunID;
				throw;
			}
			finally
			{
				importStopWatch.Stop();
				APMClient.TimedOperation(
					name: "ImportAPI.Image.ImportTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: importStopWatch.ElapsedMilliseconds,
					customData: CreateImportMetricCustomData(settings, retval));
				APMClient.TimedOperation(
					name: "ImportAPI.Image.DataGridWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: image.ImportMeasurements.DataGridImportTime.ElapsedMilliseconds,
					customData: CreateDataGridImportMetricsCustomData(settings, retval, image.ImportMeasurements));
				APMClient.TimedOperation(
					name: "ImportAPI.Image.SqlWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: image.ImportMeasurements.SqlImportTime.ElapsedMilliseconds,
					customData: CreateSqlImportMetricsCustomData(settings, retval, image.ImportMeasurements));

				massImportMetric.SendBatchCompleted(settings.RunID, importStopWatch.ElapsedMilliseconds, Constants.ImportType.Images, Constants.SystemNames.Kepler, retval, image.ImportMeasurements);
			}

			retval.RunID = settings.RunID;
			return retval;
		}

		public MassImportManagerBase.MassImportResults AttemptRunProductionImageImport(Relativity.Core.BaseContext context, Relativity.MassImport.DTO.ImageLoadInfo settings, int productionArtifactID, bool inRepository, MassImportManagerBase.MassImportResults retval)
		{
			InjectionManager.Instance.Evaluate("32c593bc-b1e1-4f8e-be13-98fec84da43c");
			if (!SQLInjectionHelper.IsValidRunId(settings.RunID))
			{
				throw new System.Exception("Invalid RunId");
			}
			if (!SQLInjectionHelper.IsValidFileName(settings.BulkFileName))
			{
				throw new System.Exception("Invalid File Name");
			}

			var massImportMetric = new MassImportMetrics(CorrelationLogger, APMClient);
			var productionManager = new Relativity.Core.Service.ProductionManager();
			var image = new Data.Image(context.DBContext, settings);
			if (image.HasDataGridWorkToDo && !image.IsDataGridInputValid())
			{
				throw new System.Exception("Invalid DataGridFileName");
			}

			var importStopWatch = new Stopwatch();
			importStopWatch.Start();

			var bulkFileShareFolderPath = string.IsNullOrEmpty(settings.BulkFileSharePath) ?
				context.GetBcpSharePath() :
				settings.BulkFileSharePath;

			if (settings.UseBulkDataImport)
			{
				settings.RunID = image.InitializeBulkTable(bulkFileShareFolderPath, this.CorrelationLogger);
			}
			else
			{
				settings.RunID = image.InitializeTempTable();
			}

			if (image.IsNewJob)
			{
				massImportMetric.SendJobStarted(settings, importType: Constants.ImportType.Production, system: Constants.SystemNames.Kepler);
			}

			image.ImportMeasurements.StartMeasure(nameof(productionManager.CreateProductionDocumentFileTableForProduction));
			productionManager.CreateProductionDocumentFileTableForProduction(context, productionArtifactID);
			image.ImportMeasurements.StopMeasure(nameof(productionManager.CreateProductionDocumentFileTableForProduction));

			context.BeginTransaction();
			try
			{
				_lockHelper.Lock(context, MassImportManagerLockKey.LockType.DocumentOrImageOrProductionImage, () =>
				{
					image.ExistingFilesLookupInitialization();
					image.ManageBatesExistsErrors();

					switch (settings.Overlay)
					{
						case Relativity.MassImport.DTO.OverwriteType.Append:
						{
							image.ManageAppendErrors();
							if (Relativity.Core.Config.EnforceDocumentLimit)
							{
								ThrowIfDocumentLimitExceeded(context, image);
							}

							image.CreateDocumentsFromImageFile(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled, false);
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, false);

							break;
						}

						case Relativity.MassImport.DTO.OverwriteType.Overlay:
						{
							image.ImportMeasurements.StartMeasure(nameof(image.SetOutsideFieldName));
							image.SetOutsideFieldName = context.DBContext.ExecuteSqlStatementAsScalar(
								"SELECT TOP 1 ColumnName FROM ArtifactViewField INNER JOIN [Field] ON [Field].[ArtifactViewFieldID] = [ArtifactViewField].[ArtifactViewFieldID] AND [Field].[ArtifactID] = " +
								settings.KeyFieldArtifactID).ToString();
							image.ImportMeasurements.StopMeasure(nameof(image.SetOutsideFieldName));
							image.ManageOverwriteErrors();
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, true);

							break;
						}

						case Relativity.MassImport.DTO.OverwriteType.Both:
						{
							if (Relativity.Core.Config.CloudInstance)
							{
								ThrowIfDocumentLimitExceeded(context, image);
							}

							image.CreateDocumentsFromImageFile(context.UserID, context.RequestOrigination,
								Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled, true);
							image.PopulateArtifactIdOnInitialTempTable(context.UserID, false);

							break;
						}
					}

					image.ImportMeasurements.StartMeasure(nameof(productionManager
						.UpdateImportStatusForFilesAlreadyInProduction));
					productionManager.UpdateImportStatusForFilesAlreadyInProduction(context, productionArtifactID,
						image.TableNameImageTemp, image.QueryTimeout);
					image.ImportMeasurements.StopMeasure(nameof(productionManager
						.UpdateImportStatusForFilesAlreadyInProduction));

					image.CreateProductionImageFileRows(productionArtifactID, context.UserID,
						Relativity.Core.Config.AuditingEnabled, context.RequestOrigination,
						Relativity.Core.AuditHelper.GetRecordOrigination(), inRepository);

					image.ImportMeasurements.StartMeasure(nameof(productionManager.CreateProductionDocumentFileRows));
					productionManager.CreateProductionDocumentFileRows(context, productionArtifactID,
						image.TableNameImageTemp, image.QueryTimeout);
					image.ImportMeasurements.StopMeasure(nameof(productionManager.CreateProductionDocumentFileRows));

					int dataSourceID = productionManager.GetImportProductionDataSource(context, productionArtifactID,
						new Guid(settings.RunID.Replace("_", "-")));

					image.ImportMeasurements.StartMeasure(
						nameof(productionManager.CreateProductionInformationForImport));
					productionManager.CreateProductionInformationForImport(context, productionArtifactID, dataSourceID,
						image.QueryTimeout);
					image.ImportMeasurements.StopMeasure(nameof(productionManager
						.CreateProductionInformationForImport));

					if (settings.UploadFullText)
					{
						image.ManageImageFullText();
					}

					if (image.IsDataGridInputValid())
					{
						var loader = image.CreateDataGridReader(bulkFileShareFolderPath, this.CorrelationLogger);
						image.WriteToDataGrid(loader, context.AppArtifactID, bulkFileShareFolderPath,
							this.CorrelationLogger);
						image.MapDataGridRecords(this.CorrelationLogger);
					}

					image.UpdateArtifactAuditColumns(context.UserID);

					// need to delete files from the file system that were NOT imported (ie - appending and documents already existed)
					if (inRepository)
					{
						image.DeleteFilesNotImported();
					}

					image.ClearTempTableAndSaveErrors(); // TODO: This does nothing right now - we may want it to do something soon.
					try
					{
						var counts = image.GetReturnReport();
						retval.ArtifactsCreated = counts[0];
						retval.ArtifactsUpdated = counts[1];
						retval.FilesProcessed = counts[2];
					}
					catch
					{
					}

					InjectionManager.Instance.Evaluate("f7482c6c-2e01-4bad-bdf7-8392c9dc7fa4");
				});

				context.CommitTransaction();
			}
			catch (System.Exception ex)
			{
				context.RollbackTransaction(ex);
				throw;
			}
			finally
			{
				importStopWatch.Stop();
				APMClient.TimedOperation(
					name: "ImportAPI.ProductionImage.ImportTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: importStopWatch.ElapsedMilliseconds,
					customData: CreateImportMetricCustomData(settings, retval));
				APMClient.TimedOperation(
					name: "ImportAPI.ProductionImage.DataGridWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: image.ImportMeasurements.DataGridImportTime.ElapsedMilliseconds,
					customData: CreateDataGridImportMetricsCustomData(settings, retval, image.ImportMeasurements));
				APMClient.TimedOperation(
					name: "ImportAPI.ProductionImage.SqlWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: image.ImportMeasurements.SqlImportTime.ElapsedMilliseconds,
					customData: CreateSqlImportMetricsCustomData(settings, retval, image.ImportMeasurements));

				massImportMetric.SendBatchCompleted(settings.RunID, importStopWatch.ElapsedMilliseconds, Constants.ImportType.Production, Constants.SystemNames.Kepler, retval, image.ImportMeasurements);
			}

			retval.RunID = settings.RunID;
			return retval;
		}

		public ErrorFileKey GenerateImageErrorFiles(Relativity.Core.ICoreContext icc, string runID, int caseArtifactID, bool writeHeader, int keyFieldID)
		{
			if (!SQLInjectionHelper.IsValidRunId(runID))
			{
				throw new System.Exception("Invalid runID");
			}

			var timekeeper = new Timekeeper();
			timekeeper.MarkStart("Generate Errors");
			var settings = new Relativity.MassImport.DTO.ImageLoadInfo();
			settings.RunID = runID;
			settings.KeyFieldArtifactID = keyFieldID;
			var x = new Data.Image(icc.ChicagoContext.DBContext, settings).GenerateErrorFiles(caseArtifactID, writeHeader);
			timekeeper.MarkEnd("Generate Errors");
			timekeeper.GenerateCsvReportItemsAsRows("_webapi_image", @"C:\");
			return x;
		}

		public bool ImageRunHasErrors(Relativity.Core.ICoreContext icc, string runId)
		{
			if (!SQLInjectionHelper.IsValidRunId(runId))
			{
				throw new System.Exception("Invalid RunId");
			}

			bool retval = (bool)icc.ChicagoContext.DBContext.ExecuteSqlStatementAsScalar(string.Format("SELECT CASE WHEN EXISTS(SELECT TOP 1 [DocumentIdentifier] FROM [Resource].[{0}] WHERE NOT [Status] = 0) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", Relativity.MassImport.Constants.IMAGE_TEMP_TABLE_PREFIX + runId));
			if (!retval)
			{
				var settings = new Relativity.MassImport.DTO.ImageLoadInfo();
				settings.RunID = runId;
				settings.KeyFieldArtifactID = -1;
				var x = new Data.Image(icc.ChicagoContext.DBContext, settings);
				x.TruncateTempTables();
			}

			return retval;
		}

		public MassImportManagerBase.MassImportResults AttemptRunNativeImport(Relativity.Core.BaseContext context, Relativity.MassImport.DTO.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, Timekeeper timekeeper, MassImportManagerBase.MassImportResults retval)
		{
			var input = NativeImportInput.ForWebApi(settings, inRepository, includeExtractedTextEncoding);
			return MassImporter.ImportNatives(context, input);
		}

		public MassImportManagerBase.MassImportResults AttemptRunObjectImport(Relativity.Core.BaseContext context, Relativity.MassImport.DTO.ObjectLoadInfo settings, bool inRepository, MassImportManagerBase.MassImportResults retval)
		{
			var input = ObjectImportInput.ForWebApi(settings, CollectIDsOnCreate);
			return MassImporter.ImportObjects(context, input);
		}

		public MassImportManagerBase.MassImportResults PostImportDocumentLimitLogic(Relativity.Core.BaseServiceContext sc, int workspaceId, MassImportManagerBase.MassImportResults importResults)
		{
			if (importResults.ArtifactsCreated != 0)
			{
				int newDocumentCount = Relativity.Core.Service.DocumentManager.IncreaseDocumentCount(sc, workspaceId, importResults.ArtifactsCreated);
				int docLimit = Relativity.Core.Service.DocumentManager.RetrieveDocumentLimit(sc, workspaceId);
				if (docLimit != 0 & newDocumentCount > docLimit)
				{
					string errorMessage = "The document import was canceled. The import would have exceeded the document limit for the workspace.";
					importResults.ExceptionDetail = new Relativity.MassImport.DTO.SoapExceptionDetail(new System.Exception(errorMessage));
				}
			}

			return importResults;
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

		private void ThrowIfDocumentLimitExceeded(Relativity.Core.BaseContext context, Data.ObjectBase importObject)
		{
			int documentImportCount = importObject.IncomingObjectCount();
			bool willExceedLimit = WillExceedDocumentLimit(context, documentImportCount);
			if (willExceedLimit)
			{
				throw new System.Exception("The document import was canceled. The import would have exceeded the document limit for the workspace.");
			}
		}

		private void ThrowIfDocumentLimitExceeded(Relativity.Core.BaseContext context, Data.Image imageImport)
		{
			int documentImportCount = imageImport.IncomingImageCount();
			bool willExceedLimit = WillExceedDocumentLimit(context, documentImportCount);
			if (willExceedLimit)
			{
				throw new System.Exception("The document import was canceled. The import would have exceeded the document limit for the workspace.");
			}
		}

		public ErrorFileKey GenerateNonImageErrorFiles(Relativity.Core.ICoreContext icc, string runID, int artifactTypeID, bool writeHeader, int keyFieldID)
		{
			if (!SQLInjectionHelper.IsValidRunId(runID))
			{
				throw new System.Exception("Invalid RunId");
			}

			var timekeeper = new Timekeeper();
			timekeeper.MarkStart("GenerateError");
			var key = Data.Helper.GenerateNonImageErrorFiles(icc.ChicagoContext.DBContext, runID, icc.ChicagoContext.AppArtifactID, artifactTypeID, writeHeader, keyFieldID);
			timekeeper.MarkEnd("GenerateError");
			timekeeper.GenerateCsvReportItemsAsRows("_webapi_errors", @"C:\");
			return key;
		}

		/// <summary>
		/// Return a SqlDataReader containing errors from a mass import operation.  It is important to close
		/// the context's connection when you are through using the reader.
		/// </summary>
		/// <param name="context">A RowDataContext.BaseContext object.  It is important to call ReleaseConnection()
		/// on this object when you are done with the reader</param>
		/// <param name="runID"></param>
		/// <param name="keyArtifactID"></param>
		/// <returns></returns>
		/// <remarks>Historical Note: The reason we don't pass in a Relativity.Core.ICoreContext is because then,
		/// the method would need to internally generate a kCura.Data.RowDataGateway.BaseContext, which may create
		/// a new DataContext.  When creating the reader, we would be opening a connection on that context.
		/// Then we would return the reader.  At that point, the caller would not be able to close the connection</remarks>
		public System.Data.SqlClient.SqlDataReader GenerateNativeErrorReader(kCura.Data.RowDataGateway.BaseContext context, string runID, int keyArtifactID)
		{
			return Native.GenerateErrorReader(context, runID, keyArtifactID);
		}

		public bool NativeRunHasErrors(Relativity.Core.ICoreContext icc, string runId)
		{
			if (!SQLInjectionHelper.IsValidRunId(runId))
			{
				throw new System.Exception("Invalid RunId");
			}

			var dbContext = icc?.ChicagoContext?.DBContext;

			if (dbContext == null)
			{
				_logger.LogFatal("The passed Core Context argument {icc} or its property is null.", icc);
				throw new System.ArgumentNullException(nameof(icc));
			}

			bool retval = (bool)dbContext.ExecuteSqlStatementAsScalar(string.Format("SELECT CASE WHEN EXISTS(SELECT TOP 1 [ArtifactID] FROM [Resource].[{0}] WHERE NOT [kCura_Import_Status] = 0) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", Relativity.MassImport.Constants.NATIVE_TEMP_TABLE_PREFIX + runId));
			if (!retval)
			{
				Data.Helper.TruncateTempTables(dbContext, runId);
			}

			return retval;
		}

		public object DisposeRunTempTables(Relativity.Core.ICoreContext icc, string runId)
		{
			if (!SQLInjectionHelper.IsValidRunId(runId))
			{
				throw new System.Exception("Invalid RunId");
			}

			Data.Helper.DropRunTempTables(icc.ChicagoContext.DBContext, runId);
			return null;
		}

		public bool AuditImport(Relativity.Core.BaseServiceContext icc, string runID, bool isFatalError, Relativity.MassImport.DTO.ImportStatistics importStats)
		{
			var context = icc.ChicagoContext;
			var auditor = new ImportAuditor(context.DBContext, CorrelationLogger);
			string xmlSnapshot = auditor.GetImportAuditXmlSnapshot(importStats, !isFatalError);
			int systemArtifactID = Relativity.Core.Service.SystemArtifactQuery.Instance().RetrieveArtifactIDByIdentifier(context, "System");

			if (Guid.TryParse(runID.Replace("_", "-"), out var runIDGuid))
			{
				var mgr = new Relativity.Core.Service.ProductionManager();
				mgr.RemoveFromCache(runIDGuid);
			}
			else
			{
				CorrelationLogger.LogWarning($"Could not remove entry from cache due to an invalid Guid. Cannot parse {runID} to Guid. {isFatalError} value: ", runID, isFatalError);
			}

			Relativity.Core.AuditHelper.CreateAuditRecord(context, systemArtifactID, (int)Relativity.Core.AuditAction.Import, xmlSnapshot, new int?(importStats.RunTimeInMilliseconds));
			return SendImportAuditNotificationEmailNew(icc, isFatalError, importStats);
		}

		private bool SendImportAuditNotificationEmailNew(Relativity.Core.BaseServiceContext icc, bool isFatalError, Relativity.MassImport.DTO.ImportStatistics importStats)
		{
			string destinationPath = null;
			try
			{
				if (!Relativity.Core.Config.SendNotificationOnImportCompletion ||
					(kCura.Notification.Config.Instance.SMTPServer.Trim() ?? "") == (string.Empty ?? "") ||
					!importStats.SendNotification)
				{
					return false;
				}

				string messageTo = new Relativity.Core.Service.UserManager().Read(new Relativity.Core.ServiceContext(icc.Identity, icc.RequestOrigination, -1), icc.UserID).EmailAddress;
				if ((messageTo.Trim() ?? "") == (string.Empty ?? ""))
				{
					return false;
				}

				var msg = new System.Net.Mail.MailMessage(kCura.Notification.Config.Instance.EmailFrom, messageTo.Trim());
				if (isFatalError)
				{
					msg.Subject = "Import Failed";
				}
				else
				{
					msg.Subject = "Import Completed";
					if (importStats.NumberOfErrors > 0L)
						msg.Subject += " With Errors";
				}

				msg.Body = "See attachment for run details";
				destinationPath = System.IO.Path.GetTempFileName();
				System.IO.File.Move(destinationPath, destinationPath.Replace(".", "_"));
				destinationPath = destinationPath.Replace(".", "_") + ".csv";
				using (var sw = new System.IO.StreamWriter(destinationPath, false, System.Text.Encoding.Default))
				{
					sw.Write(new ImportAuditor(icc.ChicagoContext.DBContext, CorrelationLogger).GetAuditCsvSnapshot(importStats));
				}

				msg.Attachments.Add(new System.Net.Mail.Attachment(destinationPath));
				kCura.Notification.Email.Instance.SendNotification(msg);
				System.IO.File.Delete(destinationPath);
				return true;
			}
			catch
			{
				try
				{
					if (destinationPath is object)
						System.IO.File.Delete(destinationPath);
				}
				catch
				{
				}

				return false;
			}
		}

		public bool HasImportPermission(Relativity.Core.ICoreContext context)
		{
			return Relativity.Core.PermissionsHelper.HasAdminOperationPermission(context, Relativity.Core.Permission.AllowDesktopClientImport);
		}

		private Dictionary<string, object> CreateDataGridImportMetricsCustomData(Relativity.MassImport.DTO.ImageLoadInfo settings, MassImportManagerBase.MassImportResults results, Data.ImportMeasurements importMeasurements)
		{
			var dict = CreateImportMetricCustomData(settings, results);
			dict.Add(nameof(importMeasurements.DataGridFileSize), importMeasurements.DataGridFileSize);
			return dict;
		}

		private Dictionary<string, object> CreateSqlImportMetricsCustomData(Relativity.MassImport.DTO.ImageLoadInfo settings, MassImportManagerBase.MassImportResults results, Data.ImportMeasurements importMeasurements)
		{
			var dict = CreateImportMetricCustomData(settings, results);
			dict.Add(nameof(settings.AuditLevel), settings.AuditLevel.ToString());
			dict.Add(nameof(settings.Overlay), settings.Overlay.ToString());
			dict.Add(nameof(importMeasurements.SqlBulkImportTime), importMeasurements.SqlBulkImportTime.ElapsedMilliseconds);
			dict.Add(nameof(settings.UseBulkDataImport), settings.UseBulkDataImport);
			dict.Add(nameof(importMeasurements.PrimaryArtifactCreationTime), importMeasurements.PrimaryArtifactCreationTime.ElapsedMilliseconds);
			return dict;
		}

		private Dictionary<string, object> CreateImportMetricCustomData(Relativity.MassImport.DTO.ImageLoadInfo settings, MassImportManagerBase.MassImportResults results)
		{
			return new Dictionary<string, object>()
			{
				{ "ImportedArtifacts", results.ArtifactsCreated + results.ArtifactsUpdated },
				{ nameof(settings.ExecutionSource), settings.ExecutionSource.ToString() },
				{ nameof(settings.UploadFullText), settings.UploadFullText }
			};
		}
	}
}