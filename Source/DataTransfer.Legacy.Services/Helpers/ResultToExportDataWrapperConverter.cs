using System;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using System.Linq;
using Relativity.DataTransfer.Legacy.Services.Extensions;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public class ResultToExportDataWrapperConverter
	{
		private readonly IAPILog _logger;

		public ResultToExportDataWrapperConverter(IAPILog logger)
		{
			_logger = logger;
		}

		public ExportDataWrapper Convert(object[] result)
		{
			ExportDataWrapper dataWrapper = null;
			while (dataWrapper == null)
			{
				try
				{
					_logger.LogVerbose("ResultToExportDataWrapperConverter. Result has '{count}' records.", result?.Length ?? 0);
					dataWrapper = ConvertToDeserializable(result);
				}
				catch (Exception ex)
				{
					var exceptions = ex.GetAllExceptionsInChain();

					bool isOutOfMemoryException = exceptions.Any(x => x is OutOfMemoryException);
					if (isOutOfMemoryException)
					{
						_logger.LogWarning(ex, "OutOfMemoryException was thrown when creating ExportDataWrapper. Result has '{count}' records.", result.Length);

						if (result.Length > 1)
						{
							int newResultsLength = result.Length / 2;
							result = result.Take(newResultsLength).ToArray();
							dataWrapper = null;
						}
						else
						{
							throw;
						}
					}
					else
					{
						throw;
					}
				}
			}

			return dataWrapper;
		}

		private ExportDataWrapper ConvertToDeserializable(object[] result)
		{
			// REL-797147, System.OutOfMemoryException: Array dimensions exceeded supported range
			const int maximumSerializedDataLength = 456176861; // 1024 rows, 1024 columns, 384 characters per each element

			var dataWrapper = new ExportDataWrapper(result);
			while (dataWrapper.SerializedDataLength > maximumSerializedDataLength && result.Length > 1)
			{
				int newResultsLength = result.Length / 2;
				_logger.LogWarning(
					"Length of ExportDataWrapper is too large: '{dataWrapperLength}'. Reducing size of result set from {length} to {newLength} records.",
					dataWrapper.SerializedDataLength,
					result.Length,
					newResultsLength);

				dataWrapper = null;
				result = result.Take(newResultsLength).ToArray();
				dataWrapper = new ExportDataWrapper(result);
			}

			return dataWrapper;
		}
	}
}
