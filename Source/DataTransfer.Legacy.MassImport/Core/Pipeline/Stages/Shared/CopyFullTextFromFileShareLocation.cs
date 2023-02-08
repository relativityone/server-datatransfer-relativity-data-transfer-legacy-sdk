using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class CopyFullTextFromFileShareLocationStage : Framework.IPipelineStage<Input.NativeImportInput>
	{
		private readonly MassImportContext _context;

		public CopyFullTextFromFileShareLocationStage(MassImportContext context)
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
				new LockHelper(new AppLockProvider()),
				_context.Helper);
			native.CopyFullTextFromFileShareLocation();
			return input;
		}

		public override string ToString()
		{
			return $"\"{nameof(CopyFullTextFromFileShareLocationStage)}\"";
		}
	}
}