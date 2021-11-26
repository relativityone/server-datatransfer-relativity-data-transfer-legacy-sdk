using System;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class QueryMetricsCollector : IDisposable
	{
		private bool _disposed = false;
		private readonly SqlConnection _connection;
		private readonly ImportMeasurements _importMeasurements;

		public QueryMetricsCollector(BaseContext context, ImportMeasurements importMeasurements)
		{
			_importMeasurements = importMeasurements;
			_connection = context.GetConnection();
			_connection.InfoMessage += SqlInfoMessageEventHandler;
		}

		private void SqlInfoMessageEventHandler(object sender, SqlInfoMessageEventArgs args)
		{
			_importMeasurements.ParseTimeStatistics(args.Message);
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_connection.InfoMessage -= SqlInfoMessageEventHandler;

			_disposed = true;
		}
	}
}
