using System.Linq;
using System.Web;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;

namespace Relativity.DataTransfer.Legacy.Tests
{
	[TestFixture]
	public class WebDistributedContractTests
	{
		#region Taken from TransferAPI

		private const string DownloadFileFileArtifactUrlTemplate =
			@"{0}{1}/Download.aspx?ObjectArtifactID={2}&FileID={3}&AppID={4}&FileFieldArtifactID={5}&AuthenticationToken={6}";

		private const string DownloadFullTextUrlTemplate =
			@"{0}{1}/Download.aspx?ArtifactID={2}&AppID={3}&ExtractedText=True&AuthenticationToken={4}";

		private const string DownloadLongTextFieldArtifactUrlTemplate =
			@"{0}{1}/Download.aspx?ArtifactID={2}&AppID={3}&LongTextFieldArtifactID={4}&AuthenticationToken={5}";

		private const string DownloadNativeFileUrlTemplate =
			@"{0}{1}/Download.aspx?ArtifactID={2}&GUID={3}&AppID={4}&AuthenticationToken={5}";

		#endregion

		[Test]
		public void EnsureFieldFileHasProperParameters()
		{
			string[] queryParams = HttpUtility.ParseQueryString(DownloadFileFileArtifactUrlTemplate.Split('?')[1]).AllKeys.Where(x => x != "AuthenticationToken").ToArray();

			var methodParameters = typeof(IWebDistributedService).GetMethod(nameof(IWebDistributedService.DownloadFieldFileAsync)).GetParameters().Where(x => x.Name != "correlationID").ToArray();

			methodParameters.Length.Should().Be(queryParams.Length);
		}

		[Test]
		public void EnsureFullTextHasProperParameters()
		{
			string[] queryParams = HttpUtility.ParseQueryString(DownloadFullTextUrlTemplate.Split('?')[1]).AllKeys.Where(x => x != "AuthenticationToken").ToArray();

			var methodParameters = typeof(IWebDistributedService).GetMethod(nameof(IWebDistributedService.DownloadFullTextAsync)).GetParameters().Where(x => x.Name != "correlationID").ToArray();

			//however ExtractedText parameter is always true, so it's skipped
			methodParameters.Length.Should().Be(queryParams.Length - 1);
		}

		[Test]
		public void EnsureLongTextHasProperParameters()
		{
			string[] queryParams = HttpUtility.ParseQueryString(DownloadLongTextFieldArtifactUrlTemplate.Split('?')[1]).AllKeys.Where(x => x != "AuthenticationToken").ToArray();

			var methodParameters = typeof(IWebDistributedService).GetMethod(nameof(IWebDistributedService.DownloadLongTextFieldAsync)).GetParameters().Where(x => x.Name != "correlationID").ToArray();

			methodParameters.Length.Should().Be(queryParams.Length);
		}

		[Test]
		public void EnsureNativeFileHasProperParameters()
		{
			string[] queryParams = HttpUtility.ParseQueryString(DownloadNativeFileUrlTemplate.Split('?')[1]).AllKeys.Where(x => x != "AuthenticationToken").ToArray();

			var methodParameters = typeof(IWebDistributedService).GetMethod(nameof(IWebDistributedService.DownloadNativeFileAsync)).GetParameters().Where(x => x.Name != "correlationID").ToArray();

			methodParameters.Length.Should().Be(queryParams.Length);
		}

		[Test]
		public void EnsureTempFileHasProperParameters()
		{
			string[] queryParams = HttpUtility.ParseQueryString(DownloadNativeFileUrlTemplate.Split('?')[1]).AllKeys.Where(x => x != "AuthenticationToken").ToArray();

			var methodParameters = typeof(IWebDistributedService).GetMethod(nameof(IWebDistributedService.DownloadTempFileAsync)).GetParameters().Where(x => x.Name != "correlationID").ToArray();

			//for temp files ArtifactID is not required
			methodParameters.Length.Should().Be(queryParams.Length - 1);
		}
	}
}