using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AzureRmHelper
{
    public class AuthenticationHelper
    {
        const string loginAuthority = "https://login.windows.net/{0}";
        const string azureSubscriptionUri = "https://management.core.windows.net/";
        const string azureResourceManagerUri = "https://management.azure.com/";
        //const string graphResourceUri = "https://graph.windows.net";
        //const string azureManagementUri = "https://management.azure.com/";

        string _clientId;
        Uri _returnUri;
        AuthenticationContext _authContext;

        public AuthenticationHelper(string clientId, string returnUri)
            : this(clientId, returnUri, "common")
        {

        }

        public AuthenticationHelper(string clientId, string returnUri, string subscriptionId)
        {
            _clientId = clientId;
            _returnUri = new Uri(returnUri);

            //GetAToken();
            _authContext = new AuthenticationContext(
                string.Format(loginAuthority, subscriptionId));
        }

        public AuthenticationResult SimpleLogin(string userName, string password)
        {
            var userCred = new UserCredential(userName, password);
            var rv = _authContext.AcquireTokenAsync(azureResourceManagerUri, _clientId, userCred).Result;
            return rv;
        }

        
        public AuthenticationResult GetResourceManagerToken()
        {

            //IAuthorizationParameters parent;
            //parent = new AuthorizationParameters(PromptBehavior.RefreshSession, null);// this.Handle);

            IPlatformParameters parent;
            parent = new PlatformParameters(PromptBehavior.RefreshSession);


            //if (_authContext.TokenCache.ReadItems().Count() > 0)
                //_authContext =
                //    new AuthenticationContext(_authContext.TokenCache.ReadItems().First().Authority);

            var authResult = _authContext.AcquireTokenAsync(azureResourceManagerUri, _clientId, _returnUri, parent).Result;

            return authResult;
        }

        public AuthenticationResult GetSubscriptionToken()
        {
            IPlatformParameters parent;
            parent = new PlatformParameters(PromptBehavior.RefreshSession);
            //if (_authContext.TokenCache.ReadItems().Count() > 0)
            //_authContext =
            //    new AuthenticationContext(_authContext.TokenCache.ReadItems().First().Authority);

            var authResult = _authContext.AcquireTokenAsync(azureSubscriptionUri, _clientId, _returnUri, parent).Result;

            return authResult;
        }

    }
}



