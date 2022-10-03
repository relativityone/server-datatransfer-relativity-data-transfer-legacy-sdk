using kCura.Utility;
using Relativity.Core.Service;
using Relativity.MassImport.Core.Command;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.Data.DataGrid;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.Toggles;
using DataTransfer.Legacy.MassImport.Data.Cache;

namespace Relativity.MassImport.Core.Pipeline.Stages.Natives
{
	internal class ImportNativesStage : Framework.IPipelineStage<NativeImportInput, MassImportManagerBase.MassImportResults>
	{
		private readonly MassImportContext _context;

		public ImportNativesStage(MassImportContext context)
		{
			_context = context;
		}

		public MassImportManagerBase.MassImportResults Execute(NativeImportInput input)
		{
			var settings = input.Settings;
			bool inRepository = input.InRepository;
			bool includeExtractedTextEncoding = input.IncludeExtractedTextEncoding;
			var queryExecutor = new QueryExecutor(_context.BaseContext.DBContext, _context.Logger);
			var native = new Native(
				_context.BaseContext,
				queryExecutor,
				settings,
				(int)input.ImportUpdateAuditAction,
				_context.ImportMeasurements,
				input.ColumnDefinitionCache,
				_context.CaseSystemArtifactId,
				new LockHelper(new AppLockProvider()));
			var dataGridInputReaderProvider = input.DataGridInputReaderProvider ?? native;
			_context.ImportMeasurements.SqlImportTime.Start();
			int auditUserId = Relativity.Core.Service.Audit.ImpersonationToolkit.GetCaseAuditUserId(_context.BaseContext, settings.OnBehalfOfUserToken);
			var result = this.ExecuteNativeImport(_context, native, dataGridInputReaderProvider, settings, auditUserId, inRepository, includeExtractedTextEncoding, input);
			InjectionManager.Instance.Evaluate("85cf89dd-e6a0-4e63-9065-a4971ba4eb07");
			return result;
		}

		private MassImportManagerBase.MassImportResults ExecuteNativeImport(MassImportContext context, Native native, IDataGridInputReaderProvider dataGridInputReaderFactory, Relativity.MassImport.DTO.NativeLoadInfo settings, int auditUserID, bool inRepository, bool includeExtractedTextEncoding, NativeImportInput input)
		{
			IChoicesImportService choicesImportService = CreateChoicesImportService(settings, input.ColumnDefinitionCache);
			var command = new NativeImportCommand(
				_context.Logger,
				context.BaseContext,
				_context.Timekeeper,
				_context.ImportMeasurements,
				native,
				dataGridInputReaderFactory,
				auditUserID,
				settings,
				inRepository,
				includeExtractedTextEncoding,
				input,
				choicesImportService, 
				new LockHelper(new AppLockProvider()));
			return command.ExecuteNativeImport();
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
				InstanceSettings.MassImportSqlTimeout);
		}
	}
}