using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.Kepler.Transport;
using Relativity.Services.FileSystem;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	public static class BcpFileHelper
	{
		public static async Task<string> GetBcpPathAsync(int workspaceId)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (IFileIOService fileIoService = serviceFactory.GetServiceProxy<IFileIOService>())
			{
				return await fileIoService.GetBcpSharePathAsync(workspaceId, Guid.NewGuid().ToString()).ConfigureAwait(false);
			}

		}

		public static Task<string> CreateEmptyAsync(int workspaceId)
		{
			return CreateAsync(string.Empty, workspaceId);
		}

		/// <summary>
		/// Creates file in BcpPath with GUID as a file name.
		/// </summary>
		/// <param name="content">File content.</param>
		/// <returns>File name.</returns>
		public static async Task<string> CreateAsync(string content, int workspaceId)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

			var bcpPath = await GetBcpPathAsync(workspaceId).ConfigureAwait(false);

			UnicodeEncoding encoding = new UnicodeEncoding(false, true);
			byte[] contentBytes = encoding.GetPreamble().Concat(encoding.GetBytes(content)).ToArray();

			string fileName = Guid.NewGuid().ToString();
			using (var fileIOService = serviceFactory.GetServiceProxy<IFileIOService>())
			{
				await fileIOService.BeginFillAsync(workspaceId, contentBytes.Concat(new byte[] { 0x49 }).ToArray(),
					bcpPath + "\\",
					fileName, Guid.NewGuid().ToString());
			}

			return fileName;
		}
	}
}