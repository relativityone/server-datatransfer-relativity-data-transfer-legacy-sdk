﻿using System.Collections.Generic;
using Relativity.MassImport.Data.DataGrid;

namespace Relativity.MassImport.Core.Pipeline.Input
{
	internal class NativeImportInput : CommonInput, Interface.INativeSpecificInput, Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>, Interface.IDataGridInputReaderProviderInput, Interface.ICollectCreatedIDsInput
	{
		public bool InRepository { get; private set; }
		public Relativity.MassImport.DTO.NativeLoadInfo Settings { get; private set; }
		public IDataGridInputReaderProvider DataGridInputReaderProvider { get; private set; }
		public bool CollectCreatedIDs { get; private set; }
		public IEnumerable<Data.DataGrid.DGImportFileInfo> DGImportFileInfo { get; set; }

		private NativeImportInput(Relativity.MassImport.DTO.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, Relativity.Core.AuditAction importUpdateAuditAction, IDataGridInputReaderProvider dataGridInputReaderProvider, bool collectCreatedIDs) : base(includeExtractedTextEncoding, importUpdateAuditAction)
		{
			Settings = settings;
			InRepository = inRepository;
			DataGridInputReaderProvider = dataGridInputReaderProvider;
			CollectCreatedIDs = collectCreatedIDs;
			DGImportFileInfo = null;
		}

		public static NativeImportInput ForWebApi(Relativity.MassImport.DTO.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding)
		{
			return new NativeImportInput(settings, inRepository, includeExtractedTextEncoding, Relativity.Core.AuditAction.Update_Import, null, false);
		}

		public static NativeImportInput ForObjectManager(Relativity.MassImport.DTO.NativeLoadInfo settings, IDataGridInputReaderProvider dataGridInputReaderProvider)
		{
			return new NativeImportInput(settings, false, false, Relativity.Core.AuditAction.Update_Import, dataGridInputReaderProvider, false);
		}

		public static NativeImportInput ForRsapi(Relativity.MassImport.DTO.NativeLoadInfo settings, IDataGridInputReaderProvider dataGridInputReaderProvider, bool collectCreatedIDs)
		{
			return new NativeImportInput(settings, false, false, Relativity.Core.AuditAction.Update_Import, dataGridInputReaderProvider, collectCreatedIDs);
		}
	}
}