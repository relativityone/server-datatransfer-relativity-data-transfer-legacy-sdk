using System;
using AutoMapper;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
	public class Mapper
	{
		private readonly IMapper _mapper;

		public Mapper()
		{
			var config = new MapperConfiguration(Configure);
			_mapper = config.CreateMapper();
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			return _mapper.Map(source, sourceType, destinationType);
		}

		public T Map<T>(object source)
		{
			return _mapper.Map<T>(source);
		}

		private void Configure(IMapperConfigurationExpression config)
		{
			config.CreateMap<SDK.ImportExport.V1.Models.ImageImportStatistics, kCura.EDDS.WebAPI.AuditManagerBase.ImageImportStatistics>();
			config.CreateMap<SDK.ImportExport.V1.Models.ObjectImportStatistics, kCura.EDDS.WebAPI.AuditManagerBase.ObjectImportStatistics>();
			config.CreateMap<SDK.ImportExport.V1.Models.ExportStatistics, kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics>();
			config.CreateMap<SDK.ImportExport.V1.Models.ImageLoadInfo, kCura.EDDS.WebAPI.BulkImportManagerBase.ImageLoadInfo>();
			config.CreateMap<SDK.ImportExport.V1.Models.NativeLoadInfo, kCura.EDDS.WebAPI.BulkImportManagerBase.NativeLoadInfo>();
			config.CreateMap<SDK.ImportExport.V1.Models.ObjectLoadInfo, kCura.EDDS.WebAPI.BulkImportManagerBase.ObjectLoadInfo>();
			config.CreateMap<SDK.ImportExport.V1.Models.LoadRange, kCura.EDDS.WebAPI.BulkImportManagerBase.LoadRange>();
			config.CreateMap<SDK.ImportExport.V1.Models.FieldInfo, kCura.EDDS.WebAPI.BulkImportManagerBase.FieldInfo>();
			config.CreateMap<SDK.ImportExport.V1.Models.Code, kCura.EDDS.WebAPI.CodeManagerBase.Code>();
			config.CreateMap<SDK.ImportExport.V1.Models.KeyboardShortcut, kCura.EDDS.WebAPI.CodeManagerBase.KeyboardShortcut>();
		}
	}
}
