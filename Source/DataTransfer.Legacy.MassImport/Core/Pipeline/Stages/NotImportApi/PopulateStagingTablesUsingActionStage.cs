using System;
using Relativity.Data.MassImport;

namespace Relativity.MassImport.Core.Pipeline.Stages.NotImportApi
{
	internal class PopulateStagingTablesUsingActionStage<T> : Framework.IPipelineStage<T>
	{
		private readonly MassImportContext _context;
		private readonly Action<TableNames> _populateAction;

		public PopulateStagingTablesUsingActionStage(MassImportContext context, Action<TableNames> populateAction)
		{
			_context = context;
			_populateAction = populateAction;
		}

		public T Execute(T input)
		{
			_populateAction(_context.JobDetails.TableNames);
			return input;
		}
	}
}