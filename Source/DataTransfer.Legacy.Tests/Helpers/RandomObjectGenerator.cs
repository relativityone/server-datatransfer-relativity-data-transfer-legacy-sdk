using System;
using System.Collections.Generic;
using AutoBogus;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using ErrorFileKey = Relativity.MassImport.ErrorFileKey;
using Field = Relativity.Core.DTO.Field;
using Folder = Relativity.Core.DTO.Folder;
using ProductionInfo = Relativity.Production.ProductionInfo;
using RelationalFieldPane = Relativity.Core.DTO.RelationalFieldPane;

namespace Relativity.DataTransfer.Legacy.Tests.Helpers
{
	public class RandomObjectGenerator
	{
		private readonly IDictionary<Type, Func<object>> _generators = new Dictionary<Type, Func<object>>();

		public RandomObjectGenerator()
		{
			_generators.Add(typeof(ExportStatistics), () => AutoFaker.Generate<ExportStatistics>());
			_generators.Add(typeof(ImageImportStatistics), () => AutoFaker.Generate<ImageImportStatistics>());
			_generators.Add(typeof(ObjectImportStatistics), () => AutoFaker.Generate<ObjectImportStatistics>());
			_generators.Add(typeof(SDK.ImportExport.V1.Models.ImageLoadInfo), () => AutoFaker.Generate<SDK.ImportExport.V1.Models.ImageLoadInfo>());
			_generators.Add(typeof(SDK.ImportExport.V1.Models.NativeLoadInfo), () => AutoFaker.Generate<SDK.ImportExport.V1.Models.NativeLoadInfo>());
			_generators.Add(typeof(SDK.ImportExport.V1.Models.ObjectLoadInfo), () => AutoFaker.Generate<SDK.ImportExport.V1.Models.ObjectLoadInfo>());
			_generators.Add(typeof(LoadRange), () => AutoFaker.Generate<LoadRange>());
			_generators.Add(typeof(KeyboardShortcut), () => AutoFaker.Generate<KeyboardShortcut>());
			_generators.Add(typeof(SDK.ImportExport.V1.Models.FieldInfo), () => AutoFaker.Generate<SDK.ImportExport.V1.Models.FieldInfo>());
			_generators.Add(typeof(Code), () => AutoFaker.Generate<Code>());

			_generators.Add(typeof(CaseInfo), () => AutoFaker.Generate<CaseInfo>());
			_generators.Add(typeof(ChoiceInfo), () => AutoFaker.Generate<ChoiceInfo>());
			_generators.Add(typeof(ErrorFileKey), () => AutoFaker.Generate<ErrorFileKey>());
			_generators.Add(typeof(Field), () => AutoFaker.Generate<Field>());
			_generators.Add(typeof(Folder), () => AutoFaker.Generate<Folder>());
			_generators.Add(typeof(Core.Export.InitializationResults), () => AutoFaker.Generate<Core.Export.InitializationResults>());
			_generators.Add(typeof(ExternalIO.IoResponse), () => AutoFaker.Generate<ExternalIO.IoResponse>());
			_generators.Add(typeof(MassImportManagerBase.MassImportResults), () => AutoFaker.Generate<MassImportManagerBase.MassImportResults>());
			_generators.Add(typeof(ProductionInfo), () => AutoFaker.Generate<ProductionInfo>());
			_generators.Add(typeof(Field.ObjectsFieldParameters), () => AutoFaker.Generate<Field.ObjectsFieldParameters>());
			_generators.Add(typeof(SoapExceptionDetail), () => AutoFaker.Generate<SoapExceptionDetail>());
			_generators.Add(typeof(Core.DTO.KeyboardShortcut), () => AutoFaker.Generate<Core.DTO.KeyboardShortcut>());
			_generators.Add(typeof(RelationalFieldPane), () => AutoFaker.Generate<RelationalFieldPane>());
		}

		public object Generate(Type type)
		{
			if (!_generators.ContainsKey(type))
			{
				return null;
			}

			return _generators[type]();
		}
	}
}