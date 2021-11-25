
namespace Relativity.Core.Service.MassImport.Logging
{
	// TODO: change to internal and correct namespace, https://jira.kcura.com/browse/REL-482642
	public class ImportLogContext
	{
		public string RunId { get; private set; }
		public string Type { get; private set; }
		public int WorkspaceId { get; private set; }

		public ImportLogContext(string runId, string action, int workspaceId)
		{
			RunId = runId;
			Type = action;
			WorkspaceId = workspaceId;
		}
	}
}