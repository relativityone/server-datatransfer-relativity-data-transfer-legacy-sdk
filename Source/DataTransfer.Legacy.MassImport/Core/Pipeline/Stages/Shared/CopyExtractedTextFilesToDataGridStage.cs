﻿using System;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.DataGrid;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class CopyExtratedTextFilesToDataGridStage : Framework.IPipelineStage<Input.NativeImportInput>
	{
		private readonly MassImportContext _context;

		public CopyExtratedTextFilesToDataGridStage(MassImportContext context)
		{
			_context = context;
		}

		public Input.NativeImportInput Execute(Input.NativeImportInput input)
		{
			var queryExecutor = new QueryExecutor(_context.BaseContext.DBContext, _context.Logger);
			var native = new Native(
				_context.BaseContext,
				queryExecutor,
				input.Settings,
				(int)input.ImportUpdateAuditAction,
				_context.ImportMeasurements,
				input.ColumnDefinitionCache,
				_context.CaseSystemArtifactId,
				new LockHelper(new AppLockProvider()));

			IDataGridInputReaderProvider dataGridReaderProvider= input.DataGridInputReaderProvider ?? native;
			if (dataGridReaderProvider.IsDataGridInputValid())
			{
				var bulkFileSharePath = string.IsNullOrEmpty(input.Settings.BulkFileSharePath)
					? _context.BaseContext.GetBcpSharePath()
					: input.Settings.BulkFileSharePath;

				if (input.Settings.HasDataGridWorkToDo)
				{
					try
					{
						_context.Logger.LogDebug("Starting DataGrid Import");
						input.DGImportFileInfo = native.DGRelativityRepository.ImportFileInfos;
						DataGridReader loader = dataGridReaderProvider.CreateDataGridInputReader(bulkFileSharePath, _context.Logger);
						native.WriteToDataGrid(loader, _context.BaseContext.AppArtifactID, bulkFileSharePath, _context.Logger);
						native.MapDataGridRecords(_context.Logger);
					}
					catch (Exception ex)
					{
						throw MassImportExceptionHandler.CreateMassImportExecutionException(ex, nameof(CopyExtratedTextFilesToDataGridStage), MassImportErrorCategory.DataGridCategory);
					}
				}

				// This file gets created even if we do not have DataGrid work, so clean it up if valid. 
				dataGridReaderProvider.CleanupDataGridInput(bulkFileSharePath, _context.Logger);
				_context.Logger.LogDebug("Finished DataGrid Import");
			}

			return input;
		}
	}
}