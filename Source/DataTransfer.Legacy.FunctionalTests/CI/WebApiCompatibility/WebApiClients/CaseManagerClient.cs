using System.Data;
using System.Net;
using kCura.WinEDDS.Service;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.WebApiClients
{
    public class CaseManagerClient : kCura.EDDS.WebAPI.CaseManagerBase.CaseManager
    {
        private readonly WebApiResultMapper _mapper;

        public CaseManagerClient(ICredentials credentials, CookieContainer cookieContainer, string webApiUrl, int webApiTimeout)
        {
            Credentials = credentials;
            CookieContainer = cookieContainer;
            Url = $"{webApiUrl}CaseManager.asmx";
            Timeout = webApiTimeout;

            _mapper = new WebApiResultMapper();
        }

        public new DataSet RetrieveAll()
        {
            return this.RetryOnReLoginException(() => base.RetrieveAll());
        }
        
        public new string[] GetAllDocumentFolderPaths()
        {
            return this.RetryOnReLoginException(() => base.GetAllDocumentFolderPaths());
        }

        public new string[] GetAllDocumentFolderPathsForCase(int caseArtifactID)
        {
            return this.RetryOnReLoginException(() => base.GetAllDocumentFolderPathsForCase(caseArtifactID));
        }

        public new CaseInfo Read(int caseArtifactID)
        {
            return this.RetryOnReLoginException(() => _mapper.Map<Relativity.CaseInfo>(base.Read(caseArtifactID)));
        }
    }
}
