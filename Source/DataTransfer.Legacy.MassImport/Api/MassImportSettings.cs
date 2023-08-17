namespace Relativity.MassImport.Api
{
	// TODO: remove inheritance, for now inheritance is required to keep compatibility
	public class MassImportSettings : Relativity.MassImport.DTO.ObjectLoadInfo
	{
		public int BatchSize { get; set; } = 1000;
		public bool ReturnKeyFieldToArtifactIdsMappings { get; set; } = false;
	}
}
