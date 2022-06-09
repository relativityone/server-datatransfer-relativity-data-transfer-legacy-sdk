using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Relativity.MassImport.Core.Pipeline.Errors
{
	using Relativity.Core.Exception;

	internal class MassImportExceptionHandler
	{
		public static MassImportExecutionException CreateMassImportExecutionException(Exception exception,
			string stageName)
		{
			var errorCategory = GetErrorCategory(exception);

			return CreateMassImportExecutionException(exception, stageName, errorCategory);
		}

		public static MassImportExecutionException CreateMassImportExecutionException(Exception exception, string stageName, string errorCategory)
		{
			var errorMessageBuilder = new StringBuilder();
			errorMessageBuilder.Append($"Error occured while executing '{stageName}'.");

			if (MassImportErrorCategory.CategoryToErrorDescriptionMap.ContainsKey(errorCategory))
			{
				errorMessageBuilder.Append($" {MassImportErrorCategory.CategoryToErrorDescriptionMap[errorCategory]}");
			}

			errorMessageBuilder.Append($" Category: '{errorCategory}', message: '{exception.Message}'");

			return new MassImportExecutionException(errorMessageBuilder.ToString(), stageName, errorCategory, exception);
		}

		public static Relativity.MassImport.DTO.SoapExceptionDetail ConvertMassImportExceptionToSoapExceptionDetail(MassImportExecutionException exception,
			string runId)
		{
			var soapExceptionDetail = new Relativity.MassImport.DTO.SoapExceptionDetail(exception);
			soapExceptionDetail.Details.Add($"RunID:{runId}");
			soapExceptionDetail.Details.Add($"{nameof(exception.ErrorCategory)}:{exception.ErrorCategory}");
			return soapExceptionDetail;
		}

		public static Relativity.MassImport.DTO.SoapExceptionDetail ConvertExceptionToSoapExceptionDetail(Exception exception, string runId)
		{
			var soapExceptionDetail = new Relativity.MassImport.DTO.SoapExceptionDetail(exception);
			soapExceptionDetail.Details.Add($"RunID:{runId}");
			return soapExceptionDetail;
		}

		private static string GetErrorCategory(Exception exception)
		{
			Exception[] allExceptions = GetAllExceptions(exception);
			if (allExceptions.Any(kCura.Data.RowDataGateway.Helper.IsDeadLock))
			{
				return MassImportErrorCategory.DeadlockCategory;
			}
			else if (allExceptions.Any(kCura.Data.RowDataGateway.Helper.IsTimeout))
			{
				return MassImportErrorCategory.TimeoutCategory;
			}
			else if (allExceptions.Any(ex => ex is SqlException))
			{
				return MassImportErrorCategory.SqlCategory;
			}
			else if (allExceptions.Any(ex => ex is NoBcpDirectoryException))
			{
				return MassImportErrorCategory.BcpCategory;
			}

			return MassImportErrorCategory.UnknownCategory;
		}

		private static Exception[] GetAllExceptions(Exception exception)
		{
			return new[]
			{
				exception.GetBaseException(),
				exception,
				exception.InnerException
			};
		}
	}
}
