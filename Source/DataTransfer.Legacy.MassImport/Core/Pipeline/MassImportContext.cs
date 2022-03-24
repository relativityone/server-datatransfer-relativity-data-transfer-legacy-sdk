using kCura.Utility;
using Relativity.Logging;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core.Pipeline
{
	internal class MassImportContext
	{
		private readonly LoggingContext _loggingContext;

		public Relativity.Core.BaseContext BaseContext { get; }
		public ImportMeasurements ImportMeasurements { get; }
		public Timekeeper Timekeeper { get; }
		public MassImportJobDetails JobDetails { get; }
		public int CaseSystemArtifactId { get; }

		public ILog Logger => _loggingContext.Logger;

		public MassImportContext(
			Relativity.Core.BaseContext baseContext,
			LoggingContext loggingContext,
			MassImportJobDetails jobDetails,
			int caseSystemArtifactId)
		{
			BaseContext = baseContext;
			_loggingContext = loggingContext;
			JobDetails = jobDetails;
			CaseSystemArtifactId = caseSystemArtifactId;

			ImportMeasurements = new ImportMeasurements();
			Timekeeper = new Timekeeper();
		}
	}
}