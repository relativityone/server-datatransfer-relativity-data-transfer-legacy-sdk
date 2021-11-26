using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Core.Pipeline.Input
{
	internal class CommonInput : Input.Interface.IAuditInput, Input.Interface.IColumnDefinitionCacheInput, Input.Interface.IExtractedTextInput
	{
		public Relativity.Core.AuditAction ImportUpdateAuditAction { get; private set; }
		public bool IncludeExtractedTextEncoding { get; private set; }
		public ColumnDefinitionCache ColumnDefinitionCache { get; set; }

		public CommonInput(bool includeExtractedTextEncoding, Relativity.Core.AuditAction importUpdateAuditAction)
		{
			ImportUpdateAuditAction = importUpdateAuditAction;
			IncludeExtractedTextEncoding = includeExtractedTextEncoding;
		}
	}
}