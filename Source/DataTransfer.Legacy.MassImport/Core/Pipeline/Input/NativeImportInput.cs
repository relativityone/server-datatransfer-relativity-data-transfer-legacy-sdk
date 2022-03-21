using System.Collections.Generic;
using Relativity.MassImport.Data.DataGrid;

namespace Relativity.MassImport.Core.Pipeline.Input
{
	internal class NativeImportInput : CommonInput, Interface.INativeSpecificInput, Interface.IImportSettingsInput<NativeLoadInfo>, Interface.IDataGridInputReaderProviderInput, Interface.ICollectCreatedIDsInput
	{
		public bool InRepository { get; private set; }
		public NativeLoadInfo Settings { get; private set; }
		public IDataGridInputReaderProvider DataGridInputReaderProvider { get; private set; }
		public bool CollectCreatedIDs { get; private set; }
		public IEnumerable<Data.DataGrid.DGImportFileInfo> DGImportFileInfo { get; set; }

		private NativeImportInput(NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, Relativity.Core.AuditAction importUpdateAuditAction, IDataGridInputReaderProvider dataGridInputReaderProvider, bool collectCreatedIDs, string bulkFileSharePath = null) : base(includeExtractedTextEncoding, importUpdateAuditAction)
		{
			Settings = settings;
			InRepository = inRepository;
			DataGridInputReaderProvider = dataGridInputReaderProvider;
			CollectCreatedIDs = collectCreatedIDs;
			DGImportFileInfo = null;
			BulkFileSharePath = bulkFileSharePath;
		}

		public static NativeImportInput ForWebApi(NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string bulkFileSharePath)
		{
			return new NativeImportInput(settings, inRepository, includeExtractedTextEncoding, Relativity.Core.AuditAction.Update_Import, null, false, bulkFileSharePath);
		}

		public static NativeImportInput ForObjectManager(NativeLoadInfo settings, IDataGridInputReaderProvider dataGridInputReaderProvider)
		{
			return new NativeImportInput(settings, false, false, Relativity.Core.AuditAction.Update_Import, dataGridInputReaderProvider, false);
		}

		public static NativeImportInput ForRsapi(NativeLoadInfo settings, IDataGridInputReaderProvider dataGridInputReaderProvider, bool collectCreatedIDs)
		{
			return new NativeImportInput(settings, false, false, Relativity.Core.AuditAction.Update_Import, dataGridInputReaderProvider, collectCreatedIDs);
		}
	}
}