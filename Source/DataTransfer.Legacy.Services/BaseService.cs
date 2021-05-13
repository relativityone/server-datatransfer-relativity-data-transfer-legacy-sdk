using System;
using System.Data;
using System.Threading.Tasks;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;
using Relativity.Query;
using DataView = kCura.Data.DataView;

namespace Relativity.DataTransfer.Legacy.Services
{
	public abstract class BaseService : IDisposable
	{
		private readonly IMethodRunner _methodRunner;
		private readonly IServiceContextFactory _serviceContextFactory;
		protected static int AdminWorkspace = -1;

		protected BaseService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory)
		{
			_methodRunner = methodRunner;
			_serviceContextFactory = serviceContextFactory;
		}

		public async Task ExecuteAsync(Action func, int? workspaceId, string correlationId)
		{
			await _methodRunner.ExecuteAsync(async () =>
			{
				await Task.Yield();
				func();
				return true;
			}, workspaceId, correlationId);
		}

		public async Task<T> ExecuteAsync<T>(Func<T> func, int? workspaceId, string correlationId)
		{
			return await _methodRunner.ExecuteAsync(async () =>
			{
				await Task.Yield();
				return func();
			}, workspaceId, correlationId);
		}

		public async Task<DataSetWrapper> ExecuteAsync(Func<DataSet> func, int? workspaceId, string correlationId)
		{
			var result = await _methodRunner.ExecuteAsync(async () =>
			{
				await Task.Yield();
				return func();
			}, workspaceId, correlationId);
			return new DataSetWrapper(result);
		}

		public async Task<DataSetWrapper> ExecuteAsync(Func<DataView> func, int? workspaceId, string correlationId)
		{
			var result = await _methodRunner.ExecuteAsync(async () =>
			{
				await Task.Yield();
				return func();
			}, workspaceId, correlationId).ConfigureAwait(false);
			return new DataSetWrapper(result.ToDataSet());
		}

		protected BaseServiceContext GetBaseServiceContext(int workspaceID)
		{
			return _serviceContextFactory.GetBaseServiceContext(workspaceID);
		}

		protected IPermissionsMatrix GetUserAclMatrix(int workspaceID)
		{
			return new UserPermissionsMatrix(GetBaseServiceContext(workspaceID));
		}

		public void Dispose()
		{
		}
	}
}