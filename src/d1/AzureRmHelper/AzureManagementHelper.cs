using AzureRmHelper.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureRmHelper
{
    public class AzureManagementHelper
    {
        const string MsVersionHeader = "2013-08-01";

        static string s_subscriptionResourcePath = "subscriptions?api-version=2014-04-01";
        static string s_baseManagementUri = "https://management.core.windows.net/";

        static string s_baseAzureManagementUri = "https://management.azure.com/";
        static string s_providersPath = "subscriptions/{0}/providers?api-version=2015-01-01";
        //static string s_certificatesPath = 
        //    "subscriptions/{0}/providers/Microsoft.Web/certificates?api-version=2015-01-01&location=East US";

        static string s_certificatesReadPath =
            "subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Web/certificates?api-version=2014-11-01";

        static string s_certificatesUpdatePath =
            "subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Web/certificates/{2}?api-version=2014-11-01";

        static string s_sitesUpdatePath =
            "subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Web/sites/{2}?api-version=2014-11-01";


        AuthenticationResult _authResult;
        public AzureManagementHelper(AuthenticationResult authResult)
        {
            _authResult = authResult;
        }

        public List<string> Providers(string subscriptionId)
        {
            var content = Get(subscriptionId, "Default-Web-EastUS", s_providersPath);
            Console.WriteLine(content);
            File.WriteAllText("providers.json", content);
            return new List<string>();// rv;
        }

        public List<string> Certificates(string subscriptionId)
        {
            var content = Get(subscriptionId, "Default-Web-EastUS", s_certificatesReadPath);
            File.WriteAllText("certs.json", content);
            return new List<string>();

        }

        public void AddCertificate(
            string subscriptionId, 
            string resourceGroup, 
            string location, 
            string resourceName, 
            string pfxPassword, 
            string pfxBytesBase64)
        {
            string path = string.Format(
                s_certificatesUpdatePath, subscriptionId, resourceGroup, resourceName);

            var jsonBody = new
            {
                name = resourceName,
                type = "Microsoft.web/certificates",
                location = location,
                properties = new
                {
                    pfxBlob = pfxBytesBase64,
                    password = pfxPassword
                }
            };

            var jsonBodyText = JsonConvert.SerializeObject(jsonBody);

            CallUpdates(path, jsonBodyText);
        }

        public void AssignCertificate(
            string subscriptionId,
            string resourceGroup,
            string location,
            string resourceName,
            string siteUrl,
            string certificateThumbprint
            )
        {
            string path = string.Format(
                s_sitesUpdatePath, subscriptionId, resourceGroup, resourceName);


            var hostNameSslStatesItems = new  {
            name = siteUrl,
            sslState = 1,
            thumbprint = certificateThumbprint,
            toUpdate = 1};

            var hnsArray = new[] { hostNameSslStatesItems };

            var jsonBody = new
            {
                name = resourceName,
                type = "Microsoft.Web/sites",
                location = location,
                properties = new { hostNameSslStates = hnsArray }
            };

            var jsonBodyText = JsonConvert.SerializeObject(jsonBody);
            File.WriteAllText("addcert.json", jsonBodyText);

            CallUpdates(path, jsonBodyText);
        
        }

        void CallUpdates(string path, string jsonBodyText)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(s_baseAzureManagementUri);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _authResult.AccessToken);

                HttpContent content = new StringContent(jsonBodyText, Encoding.UTF8, "application/json");
                var result = client.PutAsync(path, content).Result;
                var resultTxt = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultTxt);
            }
        }


        string Get(string subscriptionId, string regionName, string uriPath)
        {
            string content;

            using (HttpClient client = new HttpClient())
            {
                var path = string.Format(uriPath, subscriptionId, regionName);

                client.BaseAddress = new Uri(s_baseAzureManagementUri);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _authResult.AccessToken);

                Console.WriteLine("Path: {0}", path);
                var result = client.GetAsync(path).Result;

                content = result.Content.ReadAsStringAsync().Result;
                //result.EnsureSuccessStatusCode();


            }

            return content;
        }


        public List<AzSubscription> Subscriptions()
        {
            var rv = new List<AzSubscription>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(s_baseManagementUri);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _authResult.AccessToken);
                client.DefaultRequestHeaders.Add("x-ms-version", MsVersionHeader);

                var result = client.GetAsync(s_subscriptionResourcePath).Result;

                result.EnsureSuccessStatusCode();
                using (var content = result.Content.ReadAsStreamAsync().Result)
                {
                    XNamespace ns = "http://schemas.microsoft.com/windowsazure";
                    XDocument xdoc = XDocument.Load(content);

                    foreach (var subscription in xdoc.Descendants(ns + "Subscription"))
                    {
                        rv.Add(new AzSubscription
                        {
                            AadTenantId = subscription.Element(ns + "AADTenantID").Value,
                            SubscriptionId = subscription.Element(ns + "SubscriptionID").Value,
                            SubscriptionName = subscription.Element(ns + "SubscriptionName").Value

                        });
                    }
                }
            }

            return rv;
        }
    }
}
