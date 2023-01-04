using System.Diagnostics;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using System.ServiceModel.Syndication;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class CodeService : BaseService, ICodeService
	{
		private readonly CodeManagerImplementation _codeManager;
		private readonly RetryPolicyFactory _retryPolicyFactory;
		private readonly ITraceGenerator _traceGenerator;

		public CodeService(IServiceContextFactory serviceContextFactory, RetryPolicyFactory retryPolicyFactory, ITraceGenerator traceGenerator)
			: base(serviceContextFactory)
		{
			_retryPolicyFactory = retryPolicyFactory;
			_codeManager = new CodeManagerImplementation();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<DataSetWrapper> RetrieveCodesAndTypesForCaseAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.RetrieveCodesAndTypesForCase", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _codeManager.ExternalRetrieveCodesAndTypesForCase(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
			}
		}

		public Task<object> CreateEncodedAsync(int workspaceID, Code code, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.CreateEncoded", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				code.Name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(code.Name)));
				var result = _retryPolicyFactory.CreateDeadlockExceptionAndResultRetryPolicy().Execute(() => _codeManager.ExternalCreate(GetBaseServiceContext(workspaceID), code.Map<Core.DTO.Code>(), false));

				return Task.FromResult(result);
			}
		}

		public Task<int> ReadIDEncodedAsync(int workspaceID, int parentArtifactID, int codeTypeID, string name,
			string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.ReadIDEncoded", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
				var result = _codeManager.ReadID(GetBaseServiceContext(workspaceID), parentArtifactID, codeTypeID, name);
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> GetAllForHierarchicalAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.GetAllForHierarchical", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = Core.Query.Code.RetrieveHierarchicalByCodeTypeID(GetBaseServiceContext(workspaceID), codeTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> GetInitialChunkAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.GetInitialChunk", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _codeManager.GetInitialCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> GetLastChunkAsync(int workspaceID, int codeTypeID, int lastCodeID,
			string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.GetLastChunk", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _codeManager.GetNextCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID, lastCodeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<SDK.ImportExport.V1.Models.ChoiceInfo> RetrieveCodeByNameAndTypeIDEncodedAsync(
			int workspaceID, int codeTypeID, string name, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.GetAllForHRetrieveCodeByNameAndTypeIDEncodedierarchical", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
				var result = _codeManager.RetrieveCodeByNameAndTypeID(GetBaseServiceContext(workspaceID), codeTypeID, name)
					.Map<SDK.ImportExport.V1.Models.ChoiceInfo>();
				return Task.FromResult(result);
			}
		}

		public Task<int> GetChoiceLimitForUIAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Code.GetChoiceLimitForUI", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var result = Config.ChoiceLimitForUI;
				return Task.FromResult(result);
			}
		}
	}
}