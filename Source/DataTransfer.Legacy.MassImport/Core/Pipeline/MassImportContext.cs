using kCura.Utility;
using Relativity.Logging;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core.Pipeline
{
	using Relativity.API;

	internal class MassImportContext
	{
		private readonly LoggingContext _loggingContext;

		public Relativity.Core.BaseContext BaseContext { get; }
		public ImportMeasurements ImportMeasurements { get; }
		public Timekeeper Timekeeper { get; }
		public MassImportJobDetails JobDetails { get; }
		public int CaseSystemArtifactId { get; }

		public ILog Logger => _loggingContext.Logger;

		public IHelper Helper { get; }

		public MassImportContext(
			Relativity.Core.BaseContext baseContext,
			LoggingContext loggingContext,
			MassImportJobDetails jobDetails,
			int caseSystemArtifactId,
			IHelper helper)
		{
			BaseContext = baseContext;
			_loggingContext = loggingContext;
			JobDetails = jobDetails;
			CaseSystemArtifactId = caseSystemArtifactId;
			Helper = helper; 

			ImportMeasurements = new ImportMeasurements();
			Timekeeper = new Timekeeper();
		}
	}
}