using System;

namespace Relativity.MassImport.Api
{
	[Serializable]
	public class MassImportException : System.Exception
	{
		public MassImportException()
		{
		}

		public MassImportException(string message) : base(message)
		{
		}

		public MassImportException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public MassImportException(System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
