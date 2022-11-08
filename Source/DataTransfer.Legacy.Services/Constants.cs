namespace Relativity.DataTransfer.Legacy.Services
{
	internal static class Constants
	{
		internal class MassImportMetricsBucketNames
		{
			public const string REQUIRED_WORKSPACE_DOCUMENT_COUNT_RDC = "Required.Workspace.DocumentCount.RDC";
			public const string REQUIRED_WORKSPACE_DOCUMENT_COUNT_IMPORTAPI = "Required.Workspace.DocumentCount.ImportAPI";
			public const string REQUIRED_WORKSPACE_DOCUMENT_COUNT_RIP = "Required.Workspace.DocumentCount.RIP";
			public const string REQUIRED_WORKSPACE_DOCUMENT_COUNT_PROCESSING = "Required.Workspace.DocumentCount.Processing";
			public const string REQUIRED_WORKSPACE_DOCUMENT_COUNT_UNKNOWN = "Required.Workspace.DocumentCount.Unknown";
			public const string REQUIRED_WORKSPACE_FILE_COUNT_RDC = "Required.Workspace.FileCount.RDC";
			public const string REQUIRED_WORKSPACE_FILE_COUNT_IMPORTAPI = "Required.Workspace.FileCount.ImportAPI";
			public const string REQUIRED_WORKSPACE_FILE_COUNT_RIP = "Required.Workspace.FileCount.RIP";
			public const string REQUIRED_WORKSPACE_FILE_COUNT_PROCESSING = "Required.Workspace.FileCount.Processing";
			public const string REQUIRED_WORKSPACE_FILE_COUNT_UNKNOWN = "Required.Workspace.FileCount.Unknown";
		}

		internal class MetricsAttributes
		{
			/// <summary>
			/// Team ID attribute.
			/// </summary>
			public const string R1TeamIDAttribute = "r1.team.id";
		}

		internal class Application
		{
			/// <summary>
			///  ID of 'Holy Data Acquisition' team.
			/// </summary>
			public const string OwnerTeamId = "PTCI-4941411";
		}
	}
}