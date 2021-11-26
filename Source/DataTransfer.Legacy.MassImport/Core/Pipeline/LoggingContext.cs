using System;
using Relativity.Logging;

namespace Relativity.MassImport.Core.Pipeline
{
	internal class LoggingContext
	{
		private const string MassImportLogPropertyPrefix = "MassImport";
		private ILog _logger;
		private readonly string _correlationId;
		private readonly string _clientName;

		public LoggingContext(string correlationId, string clientName) : this(correlationId, clientName, Log.Logger)
		{
		}

		public LoggingContext(string correlationId, string clientName, ILog logger)
		{
			_logger = logger;
			if (string.IsNullOrWhiteSpace(correlationId))
			{
				_correlationId = Guid.NewGuid().ToString();
			}
			else
			{
				_correlationId = correlationId;
			}

			if (string.IsNullOrWhiteSpace(clientName))
			{
				_clientName = "Unknown";
			}
			else
			{
				_clientName = clientName;
			}

			SetContextInLogger();
			if ((_correlationId ?? "") != (correlationId ?? ""))
			{
				logger.LogInformation("CorrelationId '{oldCorrelationId}' was empty, using auto generated value '{newCorrelationId}'.", correlationId, _correlationId);
			}
		}

		public ILog Logger
		{
			get
			{
				return _logger;
			}
		}

		private void SetContextInLogger()
		{
			_logger = _logger
				.ForContext($"{MassImportLogPropertyPrefix}.CorrelationId", _correlationId, true)
				.ForContext($"{MassImportLogPropertyPrefix}.ClientName", _clientName, true);

		}
	}
}