using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AzureRmHelper.Tests
{
    [TestClass]
    public class UnitTest2
    {

        const string loginAuthority = "https://login.windows.net/{0}";
        const string azureSubscriptionUri = "https://management.core.windows.net/";
        const string azureResourceManagerUri = "https://management.azure.com/";
        const string clientId1 = "{clientid}";
        const string returnUri = "https://localhost:8080/login";

        const string mySubscription = "{subid}";

        [TestMethod]
        public void AddAndAssignCertificate()
        {
            var helper = new AuthenticationHelper(clientId1, returnUri);
            var ar = helper.SimpleLogin("myapp@MyDomain.onmicrosoft.com", "Pass@word1");
            var azmHelper = new AzureManagementHelper(ar);
            var bytes = File.ReadAllBytes(@"E:\gitrepos\JetDotCom\src\AzureResourceManagerDemo\wildcard.cicoriadevnet.pfx");
            var b64 = Convert.ToBase64String(bytes);
            azmHelper.AddCertificate(
                mySubscription,
                "Default-Web-EastUS",
                "East US",
                "Wildcard1",
                "pass@word1",
                b64);


            azmHelper.AssignCertificate(
                mySubscription,
                "Default-Web-EastUS",
                "East US",
                "scicoriacustom",
                "azw.cicoriadev.net",
                "DEA5DED6142EDECCDF952F4D431ED772F01D22D1");



        }

    }
}
