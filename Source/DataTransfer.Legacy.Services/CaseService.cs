using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class CaseService : BaseService, ICaseService
	{
		private readonly CaseManager _caseManager;
		private readonly ITraceGenerator _traceGenerator;

		public CaseService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_caseManager = new CaseManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<SDK.ImportExport.V1.Models.CaseInfo> ReadAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Case.Read", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var instanceLevelContext = GetBaseServiceContext(AdminWorkspace);
				var workspace = _caseManager.Read(instanceLevelContext, workspaceID);
				var caseInfo = workspace.ToCaseInfo();

				var path = ResourceServerManager.Read(instanceLevelContext, workspace.DefaultFileLocationCodeArtifactID).URL;
				caseInfo.DocumentPath = path;
				caseInfo.Name = XmlHelper.StripIllegalXmlCharacters(caseInfo.Name);
				var result = caseInfo.Map<SDK.ImportExport.V1.Models.CaseInfo>();
				return Task.FromResult(result);
			}
		}

		public Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Case.GetAllDocumentFolderPathsForCase", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _caseManager.GetAllDocumentFolderPathsForCase(GetBaseServiceContext(AdminWorkspace), workspaceID);
				return Task.FromResult(result);
			}
		}

		public Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Case.GetAllDocumentFolderPaths", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = _caseManager.GetAllDocumentFolderPaths(GetBaseServiceContext(AdminWorkspace));
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Case.RetrieveAllEnabled", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = _caseManager.RetrieveAll(GetBaseServiceContext(AdminWorkspace), null, true);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}
	}
}