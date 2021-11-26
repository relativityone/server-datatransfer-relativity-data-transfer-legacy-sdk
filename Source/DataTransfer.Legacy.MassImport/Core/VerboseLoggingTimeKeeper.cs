using kCura.Utility;
using Relativity.Logging;

namespace Relativity.MassImport.Core
{
	internal class VerboseLoggingTimeKeeper : Timekeeper
	{
		private ILog _logger;

		public VerboseLoggingTimeKeeper(ILog logger)
		{
			_logger = logger;
		}

		public override void MarkStart(string key, int thread)
		{
			_logger.LogVerbose("Timekeeper Starting {key}", key);
			base.MarkStart(key, thread);
		}

		public override void MarkEnd(string key, int thread)
		{
			_logger.LogVerbose("Timekeeper Ending {key}", key);
			base.MarkEnd(key, thread);
		}
	}
}