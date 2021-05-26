using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Mono.Cecil;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.Tests
{
	/// <summary>
	/// Makes sure WebAPI contracts (excluding Web Distributed) match Kepler ones
	/// </summary>
	[TestFixture]
	public class WebApiContractComparisonTests
	{
		private List<TypeDefinition> _webApis;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_webApis = GetAllWebApiServices().ToList();
		}

		[Test]
		[Order(1)]
		public void MakeSureManagersExist()
		{
			var keplers = GetAllKeplerServicesNames();
			//exception b/c of merging FieldManager and FieldQuery into one
			keplers.Add("FieldQuery");

			var missingManagers = new List<string>();
			foreach (var service in keplers)
			{
				if (_webApis.All(x => x.Name != MapKeplerToWebApiServiceName(service)))
				{
					missingManagers.Add(service);
				}
			}

			missingManagers.Should().BeEmpty("All Kepler services should have corresponding WebAPI managers");
		}

		[Test]
		[TestCaseSource(nameof(GetAllKeplerServices))]
		[Order(2)]
		public void MakeSureAllEndpointsExist(TypeDefinition keplerService)
		{
			var webApiMethods = GetAllWebApiMethods(MapKeplerToWebApiServiceName(keplerService.Name));

			var missingMethods = new List<string>();
			foreach (var method in keplerService.Methods)
			{
				if (webApiMethods.All(x => x.Name != MapKeplerToWebApiEndpointName(method.Name)))
				{
					missingMethods.Add(method.Name);
				}
			}

			missingMethods.Should().BeEmpty("All Kepler endpoints should have corresponding WebAPI endpoints");
		}

		[Test]
		[TestCaseSource(nameof(GetAllKeplerEndpoints))]
		[Order(3)]
		public void MakeSureEndpointsHasCorrectNumberOfParameters(MethodDefinition keplerMethod)
		{
			var webApiMethods = GetAllWebApiMethods(MapKeplerToWebApiServiceName(keplerMethod.DeclaringType.Name));
			var webApiMethodParameters = webApiMethods.First(x => x.Name == MapKeplerToWebApiEndpointName(keplerMethod.Name)).Parameters;

			var keplerMethodParameters = keplerMethod.Parameters;

			//make sure there is the same number of parameters except correlationID
			webApiMethodParameters.Count.Should().Be(keplerMethodParameters.Count - 1);
		}

		[Test]
		[TestCaseSource(nameof(GetAllKeplerEndpoints))]
		[Order(4)]
		public void MakeSureEndpointsHasCorrectTypesOfParameters(MethodDefinition keplerMethod)
		{
			const string correlationIdParamName = "correlationID";

			var webApiMethods = GetAllWebApiMethods(MapKeplerToWebApiServiceName(keplerMethod.DeclaringType.Name));
			var webApiMethodParameters = webApiMethods.First(x => x.Name == MapKeplerToWebApiEndpointName(keplerMethod.Name)).Parameters;

			var keplerMethodParameters = keplerMethod.Parameters;

			var invalidParameters = new List<string>();

			for (int i = 0; i < keplerMethodParameters.Count; i++)
			{
				if (i == keplerMethodParameters.Count - 1)
				{
					//correlationID is new parameter in Kepler endpoints
					if (keplerMethodParameters[i].Name != correlationIdParamName || keplerMethodParameters[i].ParameterType.FullName != typeof(string).FullName)
					{
						invalidParameters.Add($"{keplerMethodParameters[i].ParameterType} {keplerMethodParameters[i]}");
					}
				}
				else
				{
					if (keplerMethodParameters[i].ParameterType.FullName != webApiMethodParameters[i].ParameterType.FullName)
					{
						if (!MappingExists(keplerMethodParameters[i].ParameterType.FullName, webApiMethodParameters[i].ParameterType.FullName))
						{
							invalidParameters.Add($"{keplerMethodParameters[i].ParameterType} {keplerMethodParameters[i]}");
						}
					}
				}
			}

			invalidParameters.Should().BeEmpty();
		}

		[Test]
		[TestCaseSource(nameof(GetAllKeplerEndpoints))]
		[Order(5)]
		public void MakeSureEndpointsHasCorrectReturnType(MethodDefinition keplerMethod)
		{
			var webApiMethods = GetAllWebApiMethods(MapKeplerToWebApiServiceName(keplerMethod.DeclaringType.Name));
			var webApiMethodReturnType = webApiMethods.First(x => x.Name == MapKeplerToWebApiEndpointName(keplerMethod.Name)).ReturnType;

			var keplerMethodReturnType = keplerMethod.ReturnType;

			if (keplerMethodReturnType is GenericInstanceType genericReturnType)
			{
				string genericReturnTypeName = genericReturnType.GenericArguments[0].FullName;

				//exception b/c we have to wrap DataSet (Kepler cannot handle it without doing that)
				if (genericReturnTypeName == typeof(DataSetWrapper).FullName)
				{
					webApiMethodReturnType.FullName.Should().Be(typeof(DataSet).FullName);
				}
				//exception b/c we have to wrap export data (Kepler loses types which we need in RDC)
				else if (genericReturnTypeName == typeof(ExportDataWrapper).FullName)
				{
					webApiMethodReturnType.FullName.Should().Be(typeof(object[]).FullName);
				}
				else
				{
					if (genericReturnTypeName != webApiMethodReturnType.FullName)
					{
						if (!MappingExists(webApiMethodReturnType.FullName, genericReturnTypeName))
						{
							genericReturnTypeName.Should().Be(webApiMethodReturnType.FullName);
						}
					}

					Assert.Pass();
				}
			}
			else
			{
				webApiMethodReturnType.FullName.Should().Be("System.Void");
			}
		}

		private bool MappingExists(string source, string destination)
		{
			source = source.Replace('/', '+');
			destination = destination.Replace('/', '+');
			TypeMap[] typeMaps = ModelExtensions.Mapper.ConfigurationProvider.GetAllTypeMaps();
			return typeMaps.Any(x => x.Types.SourceType.FullName == source && x.Types.DestinationType.FullName == destination);
		}

		private static List<string> GetAllKeplerServicesNames()
		{
			return GetAllKeplerServices().Select(x => x.Name).ToList();
		}

		private List<MethodDefinition> GetAllWebApiMethods(string webApiName)
		{
			var methods = new List<MethodDefinition>();
			methods.AddRange(_webApis.First(x => x.Name == webApiName).Methods);
			if (webApiName == "FieldManager")
			{
				//exception b/c of merging FieldManager and FieldQuery into one Kepler service
				methods.AddRange(_webApis.First(x => x.Name == "FieldQuery").Methods);
			}

			return methods;
		}

		private static IEnumerable<TypeDefinition> GetAllWebApiServices()
		{
			var assembly = AssemblyDefinition.ReadAssembly(GetWebApiContractDll());
			foreach (var type in assembly.MainModule.Types)
			{
				if (type.CustomAttributes.Any(x => x.AttributeType.Name == "WebServiceAttribute"))
				{
					yield return type;
				}
			}
		}

		private static string GetWebApiContractDll()
		{
			return System.IO.File.Exists("WebAPIContract/kCura.EDDS.WebAPI.dll") ? 
				"WebAPIContract/kCura.EDDS.WebAPI.dll" : 
				"Source/DataTransfer.Legacy.Tests/WebAPIContract/kCura.EDDS.WebAPI.dll";
		}

		private static IEnumerable<TypeDefinition> GetAllKeplerServices()
		{
			var assembly = AssemblyDefinition.ReadAssembly(GetWebApiKeplerContractDll());
			foreach (var type in assembly.MainModule.Types.Where(x => x.Name != nameof(IWebDistributedService)))
			{
				if (type.CustomAttributes.Any(x => x.AttributeType.Name == nameof(WebServiceAttribute)) 
				    && type.Name != nameof(IIAPICommunicationModeService))
				{
					yield return type;
				}
			}
		}

		private static string GetWebApiKeplerContractDll()
		{
			var assemblyFullName = typeof(IWebApiReplacementModule).Assembly.GetName().Name;
			return System.IO.File.Exists($"{assemblyFullName}.dll") ?
				$"{assemblyFullName}.dll" :
				$"Source/DataTransfer.Legacy.SDK/bin/{assemblyFullName}.dll";
		}

		private static IEnumerable<MethodDefinition> GetAllKeplerEndpoints()
		{
			var allKeplerServices = GetAllKeplerServices();
			foreach (var service in allKeplerServices)
			{
				foreach (var methodDefinition in service.Methods)
				{
					yield return methodDefinition;
				}
			}
		}

		private static string MapKeplerToWebApiServiceName(string keplerName)
		{
			if (keplerName == "IFileIOService")
			{
				//exception in naming convention in WebAPI
				return "FileIO";
			}

			return keplerName.TrimStart('I').Replace("Service", "Manager");
		}

		private static string MapKeplerToWebApiEndpointName(string keplerEndpointName)
		{
			if (keplerEndpointName == "RetrieveInitialChunkAsync")
			{
				//exception b/c of typo in WebApi
				return "RetrieveIntitialChunk";
			}

			return keplerEndpointName.Replace("Async", string.Empty);
		}
	}
}