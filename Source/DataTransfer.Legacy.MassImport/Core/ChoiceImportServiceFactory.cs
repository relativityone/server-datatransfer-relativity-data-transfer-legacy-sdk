using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.Choices;
using Relativity.MassImport.Toggles;
using Relativity.Toggles;

namespace Relativity.MassImport.Core
{
	internal class ChoiceImportServiceFactory
	{
		private readonly IToggleProvider _toggleProvider;

		public ChoiceImportServiceFactory(IToggleProvider toggleProvider)
		{
			this._toggleProvider = toggleProvider;
		}

		public IChoicesImportService Create(
			MassImportContext context,
			NativeLoadInfo settings,
			IColumnDefinitionCache columnDefinitionCache)
		{
			int queryTimeout = Relativity.Data.Config.MassImportSqlTimeout;
			if (_toggleProvider.IsEnabled<UseNewChoicesQueryToggle>())
			{
				return new ChoicesImportService(
					context.BaseContext.DBContext,
					_toggleProvider,
					context.JobDetails.TableNames,
					context.ImportMeasurements,
					settings,
					columnDefinitionCache,
					queryTimeout);
			}
			else
			{
				return new OldChoicesImportService(
					context.BaseContext.DBContext,
					_toggleProvider,
					context.JobDetails.TableNames,
					context.ImportMeasurements,
					settings,
					queryTimeout);
			}
		}
	}
}
