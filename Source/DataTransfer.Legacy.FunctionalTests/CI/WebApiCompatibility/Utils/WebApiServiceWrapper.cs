using System;
using System.Net;
using kCura.WinEDDS.Service;
using NUnit.Framework;
using Relativity.Testing.Framework;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public class WebApiServiceWrapper
    {
        private readonly string _username;
        private readonly string _password;

        public WebApiServiceWrapper()
        {
            _username = RelativityFacade.Instance.Config.RelativityInstance.AdminUsername;
            _password = RelativityFacade.Instance.Config.RelativityInstance.AdminPassword;
        }

        public void PerformDataRequest(Action<ICredentials, CookieContainer> action)
        {
            try
            {
                var credentials = new NetworkCredential(_username, _password);
                var cookieContainer = new CookieContainer();

                var userManager = new UserManager(credentials, cookieContainer);
                userManager.Login(_username, _password);

                action(credentials, cookieContainer);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Exception occurred when retrieving data from RelativityWebApi endpoint for username {_username}: {ex}");
                throw;
            }
        }
    }
}
