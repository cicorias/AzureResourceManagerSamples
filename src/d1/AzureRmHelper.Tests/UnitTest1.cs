using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
namespace AzureRmHelper.Tests
{
    [TestClass]
    public class UnitTest1
    {

        static AuthenticationResult _authResultSubscription = null;
        static AuthenticationResult _authResultResourceManager = null;

        const string mySubscription = "{subid}";

        [ClassInitialize]
        //[TestInitialize]
        public static void SetupAuthResult(TestContext context)
        {

            var clientId1 = "{clientid}";
            var returnUri = "https://localhost:8080/login";
            var authHelper = new AuthenticationHelper(clientId1, returnUri);
            _authResultSubscription = authHelper.GetSubscriptionToken();

            var authHelper2 = new AuthenticationHelper(clientId1, returnUri, "{subid}");
            _authResultResourceManager = authHelper2.GetResourceManagerToken();


            File.WriteAllText("tokenSubscription.txt", _authResultSubscription.AccessToken);
            File.WriteAllText("tokenRM.txt", _authResultResourceManager.AccessToken);
        }

        [TestMethod]
        public void SimpleAuthResultCheck()
        {
            Console.WriteLine(_authResultSubscription.TenantId);
            Assert.IsNotNull(_authResultSubscription, "Auth result is null");
        }

        [TestMethod]
        public void HasSubscriptions()
        {
            var azmHelper = new AzureManagementHelper(_authResultSubscription);

            var rv = azmHelper.Subscriptions();

            Assert.IsNotNull(rv, "no subscriptions");
            Assert.IsTrue(rv.Count > 1, "Count zero");

            foreach (var item in rv)
            {
                Console.WriteLine("SubID: {0}  - Name: {1}", item.SubscriptionId, item.SubscriptionName);
            }
        }


        [TestMethod]
        public void HasProviders()
        {
            var azmHelper = new AzureManagementHelper(_authResultResourceManager);

            var providers = azmHelper.Providers(mySubscription);

            Assert.IsNotNull(providers, "providers is null");
            //Assert.IsTrue(providers.Count > 0, "no providers");
        }

        [TestMethod]
        public void HasCertificates()
        {
            var azmHelper = new AzureManagementHelper(_authResultResourceManager);

            var certificates = azmHelper.Certificates(mySubscription);

            Assert.IsNotNull(certificates, "certificates is null");
            //Assert.IsTrue(providers.Count > 0, "no providers");
        }

        [TestMethod]
        public void AddCert()
        {
            var azmHelper = new AzureManagementHelper(_authResultResourceManager);

            var bytes = File.ReadAllBytes(@"E:\gitrepos\JetDotCom\src\AzureResourceManagerDemo\wildcard.cicoriadevnet.pfx");

            var b64 = Convert.ToBase64String(bytes);


            azmHelper.AddCertificate(
                mySubscription,
                "Default-Web-EastUS",
                "East US",
                "Wildcard1",
                "pass@word1",
                b64);


        }




    }
}
