using kCura.Utility;
using Relativity.Core.Service;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.Data.DataGrid;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Core.Command
{
	internal class NativeImportCommand : ImportCommandBase
	{
		private readonly IChoicesImportService _choicesImportService;
		private readonly ILockHelper _lockHelper;

		private ImportMeasurements ImportMeasurements { get; }

		public NativeImportCommand(
			ILog correlationLogger, 
			Relativity.Core.BaseContext context, 
			Timekeeper timeKeeper, 
			ImportMeasurements importMeasurements, 
			Native native, 
			IDataGridInputReaderProvider dataGridInputReaderProvider, 
			int auditUserId, 
			NativeLoadInfo settings, 
			bool inRepository, 
			bool includeExtractedTextEncoding, 
			NativeImportInput input,
			IChoicesImportService choicesImportService,
			ILockHelper lockHelper) : base(correlationLogger, context, timeKeeper)
		{
			ImportMeasurements = importMeasurements;
			Native = native;
			DataGridInputReaderProvider = dataGridInputReaderProvider;
			AuditUserId = auditUserId;
			Settings = settings;
			InRepository = inRepository;
			IncludeExtractedTextEncoding = includeExtractedTextEncoding;
			IsAppend = settings.Overlay == Relativity.MassImport.OverwriteType.Append;
			IsOverlay = settings.Overlay == Relativity.MassImport.OverwriteType.Overlay;
			IsAppendOrOverlay = settings.Overlay == Relativity.MassImport.OverwriteType.Both;
			IsOverlayOrBoth = IsOverlay || IsAppendOrOverlay;
			IsAppendOrBoth = IsAppend || IsAppendOrOverlay;
			MoveDocumentsInAppendOverlay = IsOverlayOrBoth & settings.MoveDocumentsInAppendOverlayMode;
			Input = input;
			_choicesImportService = choicesImportService;
			_lockHelper = lockHelper;
		}

		protected int AuditUserId { get; private set; }
		protected bool IncludeExtractedTextEncoding { get; private set; }
		protected bool InRepository { get; private set; }
		protected bool IsAppend { get; private set; }
		protected bool IsAppendOrBoth { get; private set; }
		protected bool IsAppendOrOverlay { get; private set; }
		protected bool IsOverlay { get; private set; }
		protected bool IsOverlayOrBoth { get; private set; }
		protected bool MoveDocumentsInAppendOverlay { get; private set; }
		protected Native Native { get; private set; }
		protected NativeLoadInfo Settings { get; private set; }
		protected NativeImportInput Input { get; private set; }
		private IDataGridInputReaderProvider DataGridInputReaderProvider { get; set; }

		public MassImportManagerBase.MassImportResults ExecuteNativeImport()
		{
			this.CorrelationLogger.LogDebug("Starting Transaction");
			var sql = new SerialSqlQuery();
			sql.Add(PreparePopulatePartTableQuery());
			sql.Add(PrepareErrorUpdateQuery());
			Native.ExecuteQueryAndSendMetrics(new StatisticsTimeOnQuery(sql));
			int artifactsCreated = 0;
			
			Native.ImportMeasurements.StartMeasure(nameof(MassImportManagerLockKey.LockType.Objects));
			_lockHelper.Lock(Context, MassImportManagerLockKey.LockType.Objects, () =>
			{
				Native.ImportMeasurements.StopMeasure(nameof(MassImportManagerLockKey.LockType.Objects));
				CreateAssociateObjects();
				if (IsAppendOrBoth)
				{
					Native.ImportMeasurements.StartMeasure(nameof(MassImportManagerLockKey.LockType.DocumentOrImageOrProductionImage));
					_lockHelper.Lock(Context, MassImportManagerLockKey.LockType.DocumentOrImageOrProductionImage, () =>
					{
						Native.ImportMeasurements.StopMeasure(nameof(MassImportManagerLockKey.LockType.DocumentOrImageOrProductionImage));
						artifactsCreated = this.Execute(
							() => Native.CreateDocuments(this.Context.UserID, AuditUserId,
								this.Context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(),
								Relativity.Core.Config.AuditingEnabled, IncludeExtractedTextEncoding),
							"CreateDocuments");
					});
				}
			});

			this.Execute(() => Native.PopulateArtifactIdOnInitialTempTable(this.Context.UserID, IsOverlay), "PopulateArtifactIdOnInitialTempTable");
			PopulateCodeArtifactTable();
			PopulateObjectsListTable();
			int artifactsUpdated = UpdateDocumentMetadata();
			if (MoveDocumentsInAppendOverlay)
			{
				MoveDocuments();
			}

			UpdateFullTextFromFileShareLocation();
			ManageOffTableExtractedTextFields();
			int filesProcessed = AppendAndOverlayNativeFiles();
			WriteDataGridInformation();
			Native.ImportMeasurements.SqlImportTime.Stop();
			this.TimeKeeper.GenerateCsvReportItemsAsColumns("_webapi", @"C:\");
			this.CorrelationLogger.LogDebug("Ending Transaction");
			return new MassImportManagerBase.MassImportResults()
			{
				ArtifactsCreated = artifactsCreated,
				ArtifactsUpdated = artifactsUpdated,
				FilesProcessed = filesProcessed,
				RunID = Settings.RunID
			};
		}

		private SerialSqlQuery PreparePopulatePartTableQuery()
		{
			var sql = new SerialSqlQuery();
			sql.Add(new PrintSectionQuery(Native.PopulatePartTable(), nameof(Native.PopulatePartTable)));
			sql.Add(new PrintSectionQuery(Native.PopulateParentTable(), nameof(Native.PopulateParentTable)));
			return sql;
		}

		private SerialSqlQuery PrepareErrorUpdateQuery()
		{
			var sql = new SerialSqlQuery();
			if (IsOverlay)
			{
				sql.Add(new PrintSectionQuery(Native.ManageOverwriteErrors(), nameof(Native.ManageOverwriteErrors)));
			}

			if (IsAppend)
			{
				sql.Add(new PrintSectionQuery(Native.ManageAppendErrors(), nameof(Native.ManageAppendErrors)));
			}

			if (!Settings.DisableUserSecurityCheck)
			{
				sql.Add(new PrintSectionQuery(Native.ManageCheckAddingPermissions(this.Context.UserID), nameof(Native.ManageCheckAddingPermissions)));
				if (IsOverlayOrBoth)
				{
					sql.Add(new PrintSectionQuery(Native.ManageUpdateOverlayPermissions(this.Context.UserID), nameof(Native.ManageUpdateOverlayPermissions)));
				}
			}

			sql.Add(new PrintSectionQuery(Native.ManageCheckParentIsFolder(), nameof(Native.ManageCheckParentIsFolder)));

			sql.Add(new PrintSectionQuery(
				Native.ValidateIdentifiersAreNonEmpty(),
				nameof(Native.ValidateIdentifiersAreNonEmpty)));

			return sql;
		}

		private void MoveDocuments()
		{
			ImportMeasurements.StartMeasure();
			this.Execute(() =>
			{
				var importMoveDocuments = new ImportMoveDocuments(this.Context);
				importMoveDocuments.Execute(Native.RunID, Native.TableNameNativeTemp);
			}, "MoveDocuments");
			ImportMeasurements.StopMeasure();
		}

		protected int AppendAndOverlayNativeFiles()
		{
			int nativeFilesCount = 0;
			if (Settings.UploadFiles)
			{
				switch (Settings.Overlay)
				{
					case Relativity.MassImport.OverwriteType.Both:
					case Relativity.MassImport.OverwriteType.Overlay:
						{
							this.Execute(() => Native.DeleteExistingNativeFiles(this.Context.UserID, Relativity.Core.Config.AuditingEnabled, this.Context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination()), "DeleteExistingNativeFiles");
							break;
						}
				}

				nativeFilesCount = this.Execute(() => Native.CreateNativeFiles(AuditUserId, Relativity.Core.Config.AuditingEnabled, this.Context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), InRepository), "CreateNativeFiles");

				// need to delete files from the file system that were NOT imported (ie - appending and documents already existed)
				if (InRepository && Settings.UploadFiles)
				{
					this.Execute(() => Native.DeleteFilesNotImported(), "CleanupFilesNotImported");
				}
			}

			return nativeFilesCount;
		}

		protected void CreateAssociateObjects()
		{
			this.Execute(() => Native.CreateAssociatedObjects(this.Context.UserID, AuditUserId, this.Context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), Relativity.Core.Config.AuditingEnabled), "CreateAssociatedObjects");
		}

		protected void ManageOffTableExtractedTextFields()
		{
			this.Execute(() => Native.ManageOffTableExtractedTextFields(), "ManageOffTableExtractedTextFields");
		}

		protected void PopulateCodeArtifactTable()
		{
			Native.ImportMeasurements.StartMeasure(nameof(MassImportManagerLockKey.LockType.Choice));
			_lockHelper.Lock(Context, MassImportManagerLockKey.LockType.Choice, () =>
			{
				Native.ImportMeasurements.StopMeasure(nameof(MassImportManagerLockKey.LockType.Choice));
				this.Execute(() => _choicesImportService.PopulateCodeArtifactTable(), "PopulateCodeArtifactTable");
			});
		}

		protected void PopulateObjectsListTable()
		{
			this.Execute(() => Native.PopulateObjectsListTable(), "PopulateObjectsListTable");
		}

		protected int UpdateDocumentMetadata()
		{
			int updatedDocumentsCount = 0;
			if (IsOverlayOrBoth)
			{
				updatedDocumentsCount = this.Execute(() => Native.UpdateDocumentMetadata(this.Context.UserID, AuditUserId, this.Context.RequestOrigination, Relativity.Core.AuditHelper.GetRecordOrigination(), Relativity.Core.Config.AuditingEnabled, IncludeExtractedTextEncoding), "UpdateDocumentMetadata");
			}

			return updatedDocumentsCount;
		}

		protected void UpdateFullTextFromFileShareLocation()
		{
			this.Execute(() => Native.UpdateFullTextFromFileShareLocation(), "UpdateFullTextFromFileShareLocation");
		}

		protected void WriteDataGridInformation()
		{
			if (DataGridInputReaderProvider.IsDataGridInputValid())
			{
				if (Settings.HasDataGridWorkToDo)
				{
					this.Execute(() => Native.UpdateDgFieldMappingRecords(Input.DGImportFileInfo, this.CorrelationLogger), nameof(WriteDataGridInformation));
				}
			}
		}
	}
}