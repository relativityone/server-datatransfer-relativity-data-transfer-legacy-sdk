using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Relativity.MassImport.Core.Pipeline.Errors
{
	[Serializable]
	internal class MassImportExecutionException : Exception
	{
		public readonly string StageName = "Unknown";

		public readonly string ErrorCategory = MassImportErrorCategory.UnknownCategory;

		public MassImportExecutionException()
		{
		}

		public MassImportExecutionException(string message) : base(message)
		{
		}

		public MassImportExecutionException(string message, Exception inner) : base(message, inner)
		{
		}

		public MassImportExecutionException(
			string message,
			string stageName,
			string errorCategory,
			Exception inner) : base(message, inner)
		{
			this.StageName = stageName;
			this.ErrorCategory = errorCategory;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected MassImportExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.StageName = info.GetString(nameof(StageName));
			this.ErrorCategory = info.GetString(nameof(ErrorCategory));
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(StageName), StageName);
			info.AddValue(nameof(ErrorCategory), ErrorCategory);
		}

		public bool IsRetryable() => MassImportErrorCategory.RetryableErrorCategories.Contains(ErrorCategory);

		public bool IsDeadlock() => ErrorCategory == MassImportErrorCategory.DeadlockCategory;

		public bool IsTimeout() => ErrorCategory == MassImportErrorCategory.TimeoutCategory;
	}
}
