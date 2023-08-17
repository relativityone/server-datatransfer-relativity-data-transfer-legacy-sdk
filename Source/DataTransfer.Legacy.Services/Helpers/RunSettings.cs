namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public class RunSettings
	{
		public int WorkspaceID { get; }
		public ExecutionSource ExecutionSource { get; }
		public bool OverrideReferentialLinksRestriction { get; }
		public string RunID { get; }
		public string BatchID { get; }

		public RunSettings(int workspaceID, ExecutionSource executionSource, bool overrideReferentialLinksRestriction, string runID, string batchID)
		{
			WorkspaceID = workspaceID;
			ExecutionSource = executionSource;
			OverrideReferentialLinksRestriction = overrideReferentialLinksRestriction;
			RunID = runID;
			BatchID = batchID;
		}
	}
}