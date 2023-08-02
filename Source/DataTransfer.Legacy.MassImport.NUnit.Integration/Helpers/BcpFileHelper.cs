using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Kepler.Transport;
using Relativity.Services.FileSystem;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class BcpFileHelper
	{
		/// <summary>
		/// Creates file in <see cref="IntegrationTestParameters.BcpSharePath"/> with GUID as a file name.
		/// </summary>
		/// <param name="parameters">Parameters with credentials.</param>
		/// <param name="content">File content.</param>
		/// <returns>File name.</returns>
		public static async Task<string> CreateAsync(IntegrationTestParameters parameters, string content)
		{
			UnicodeEncoding encoding = new UnicodeEncoding(false, true);
			byte[] contentBytes = encoding.GetPreamble().Concat(encoding.GetBytes(content)).ToArray();

			using (var fileSystemManager = ServiceHelper.GetServiceProxy<IFileSystemManager>(parameters))
			using (var stream = new MemoryStream(contentBytes))
			using (var keplerStream = new KeplerStream(stream))
			{
				string fileName = Guid.NewGuid().ToString();
				await fileSystemManager.UploadFileAsync(keplerStream, Path.Combine(parameters.BcpSharePath, fileName)).ConfigureAwait(false);

				return fileName;
			}
		}

		public static Task<string> CreateEmptyAsync(IntegrationTestParameters parameters)
		{
			return CreateAsync(parameters, string.Empty);
		}
	}
}