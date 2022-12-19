using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Exceptions
{
	[FaultCode(HttpStatusCode.Conflict)]
	public class BatchInProgressException : ServiceException
	{
		public BatchInProgressException()
		{
		}

		public BatchInProgressException(string message) : base(message)
		{
		}

		public BatchInProgressException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BatchInProgressException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
