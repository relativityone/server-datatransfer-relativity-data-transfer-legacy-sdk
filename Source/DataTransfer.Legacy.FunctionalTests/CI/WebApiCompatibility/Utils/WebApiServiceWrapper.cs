using System;
using System.Net;
using kCura.WinEDDS.Service;
using NUnit.Framework;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Utils
{
    public class WebApiServiceWrapper
    {
        public void PerformDataRequest(Action<ICredentials, CookieContainer> action)
        {
            try
            {
                var credentials = new NetworkCredential("relativity.admin@kcura.com", "Test1234!");
                var cookieContainer = new CookieContainer();

                var userManager = new UserManager(credentials, cookieContainer);
                userManager.Login("relativity.admin@kcura.com", "Test1234!");

                action(credentials, cookieContainer);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Exception occurred when retrieving data from RelativityWebApi endpoint: {ex}");
                throw;
            }
        }
    }
}
