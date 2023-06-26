using AutoMapper;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using ErrorFileKey = Relativity.MassImport.ErrorFileKey;
using Field = Relativity.Core.DTO.Field;
using Folder = Relativity.Core.DTO.Folder;
using ProductionInfo = Relativity.Production.ProductionInfo;
using RelationalFieldPane = Relativity.Core.DTO.RelationalFieldPane;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{

	public static class ModelExtensions
	{
		public static IMapper Mapper { get; }

		static ModelExtensions()
		{
			var config = new MapperConfiguration(cfg =>
			{
				ConfigureReturnTypesMapping(cfg);
				ConfigureParameterTypesMapping(cfg);
			});
			Mapper = config.CreateMapper();
		}

		private static void ConfigureParameterTypesMapping(IMapperConfigurationExpression cfg)
		{
			cfg.CreateMap<ExportStatistics, MassImport.ExportStatistics>();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageImportStatistics, Relativity.MassImport.DTO.ImageImportStatistics >();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ObjectImportStatistics, Relativity.MassImport.DTO.ObjectImportStatistics >();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImageLoadInfo, Relativity.MassImport.DTO.ImageLoadInfo>();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.NativeLoadInfo, Relativity.MassImport.DTO.NativeLoadInfo>();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ObjectLoadInfo, Relativity.MassImport.DTO.ObjectLoadInfo>();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.LoadRange, Relativity.MassImport.DTO.NativeLoadInfo.LoadRange>();
			cfg.CreateMap<DataTransfer.Legacy.SDK.ImportExport.V1.Models.FieldInfo, FieldInfo>();
			cfg.CreateMap<Code, Core.DTO.Code>();
			cfg.CreateMap<KeyboardShortcut, Core.DTO.KeyboardShortcut>();
		}

		private static void ConfigureReturnTypesMapping(IMapperConfigurationExpression cfg)
		{
			cfg.CreateMap<CaseInfo, DataTransfer.Legacy.SDK.ImportExport.V1.Models.CaseInfo>();
			cfg.CreateMap<ChoiceInfo, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ChoiceInfo>();
			cfg.CreateMap<ErrorFileKey, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ErrorFileKey>();
			cfg.CreateMap<Field, DataTransfer.Legacy.SDK.ImportExport.V1.Models.Field>();
			cfg.CreateMap<RelationalFieldPane, DataTransfer.Legacy.SDK.ImportExport.V1.Models.RelationalFieldPane>();
			cfg.CreateMap<Field.ObjectsFieldParameters, ObjectsFieldParameters>();
			cfg.CreateMap<Core.DTO.KeyboardShortcut, KeyboardShortcut>();
			cfg.CreateMap<Folder, DataTransfer.Legacy.SDK.ImportExport.V1.Models.Folder>();
			cfg.CreateMap<Core.Export.InitializationResults, InitializationResults>();
			cfg.CreateMap<ExternalIO.IoResponse, IoResponse>();
			cfg.CreateMap<MassImportManagerBase.MassImportResults, MassImportResults>();
			cfg.CreateMap<ProductionInfo, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ProductionInfo>();
			cfg.CreateMap<Relativity.MassImport.DTO.SoapExceptionDetail, DataTransfer.Legacy.SDK.ImportExport.V1.Models.SoapExceptionDetail>();
			cfg.CreateMap<Relativity.MassImport.DTO.ImportedDocumentInfo, DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportedDocumentInfo>();

		}

		public static T Map<T>(this object t1)
		{
			return Mapper.Map<T>(t1);
		}
	}
}