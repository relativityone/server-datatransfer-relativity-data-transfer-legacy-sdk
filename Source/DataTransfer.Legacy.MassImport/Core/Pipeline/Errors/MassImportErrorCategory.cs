using System.Collections.Generic;

namespace Relativity.MassImport.Core.Pipeline.Errors
{
	internal static class MassImportErrorCategory
	{
		public const string TimeoutCategory = "Timeout";
		public const string DeadlockCategory = "Deadlock";
		public const string SqlCategory = "Sql";
		public const string DataGridCategory = "DataGrid";
		public const string BcpCategory = "BCP";
		public const string UnknownCategory = "Unknown";

		public static IReadOnlyDictionary<string, string> CategoryToErrorDescriptionMap => _categoryToErrorDescriptionMapping;

		public static IEnumerable<string> RetryableErrorCategories => _retryableErrorCategories;

		private static readonly Dictionary<string, string> _categoryToErrorDescriptionMapping = new Dictionary<string, string>
		{
			[TimeoutCategory] = "Please try to lower import batch size.",
			[DeadlockCategory] = "Please try to lower number of concurrent imports to this workspace.",
		};

		private static readonly string[] _retryableErrorCategories =
		{
			TimeoutCategory,
			DeadlockCategory
		};
	}
}
