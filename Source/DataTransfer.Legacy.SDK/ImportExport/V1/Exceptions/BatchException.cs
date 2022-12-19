using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Exceptions
{
	[FaultCode(HttpStatusCode.BadRequest)]
	public class BatchException : ServiceException
	{
		public BatchException()
		{
		}

		public BatchException(string message) : base(message)
		{
		}

		public BatchException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
