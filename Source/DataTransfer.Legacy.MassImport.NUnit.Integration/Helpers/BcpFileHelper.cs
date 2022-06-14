using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;


namespace MassImport.NUnit.Integration.Helpers
{
	public static class BcpFileHelper
	{
		/// <summary>
		/// Creates file in <see cref="IntegrationTestParameters.BcpSharePath"/> with GUID as a file name.
		/// </summary>
		/// <param name="parameters">Parameters with credentials.</param>
		/// <param name="workspaceId"></param>
		/// <param name="content">File content.</param>
		/// <returns>File name.</returns>
		public static async Task<string> CreateAsync(IntegrationTestParameters parameters, int workspaceId, string content)
		{
			UnicodeEncoding encoding = new UnicodeEncoding(false, true);
			byte[] contentBytes = encoding.GetPreamble().Concat(encoding.GetBytes(content)).ToArray();

			string fileName = Guid.NewGuid().ToString();
			using (var fileIOService = ServiceHelper.GetServiceProxy<IFileIOService>(parameters))
			{
				await fileIOService.BeginFillAsync(workspaceId, contentBytes.Concat(new byte[] { 0x49 }).ToArray(),
					parameters.BcpSharePath+"\\",
					fileName, Guid.NewGuid().ToString());
			}

			return fileName;
		}

		public static Task<string> CreateEmptyAsync(IntegrationTestParameters parameters, int workspaceId)
		{
			return CreateAsync(parameters, workspaceId, string.Empty);
		}
	}
}