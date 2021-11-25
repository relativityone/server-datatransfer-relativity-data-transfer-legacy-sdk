using Relativity.Data.MassImport;

namespace Relativity.MassImport.Core.Pipeline
{
	internal class MassImportJobDetails
	{
		public TableNames TableNames { get; }

		/// <summary>
		/// Gets a name of the system which started the mass import job.
		/// E.g. WebAPI, ObjectManager.
		/// </summary>
		public string ClientSystemName { get; }

		public string ImportType { get; }

		public string CorrelationId => TableNames.RunId;

		public MassImportJobDetails(
			TableNames tableNames,
			string clientSystemName,
			string importType)
		{
			TableNames = tableNames;
			ClientSystemName = clientSystemName;
			ImportType = importType;
		}
	}
}
