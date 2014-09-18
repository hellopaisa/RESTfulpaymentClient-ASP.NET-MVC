using DotNetOpenAuth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Code
{
    public class AuthHelper
    {
        public static WebServerClient CreateClient()
        {
            var desc = GetAuthServerDescription();
            var client = new WebServerClient(desc, clientIdentifier: "PUJAN_ID");
            client.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter("PUJAN_SECRET");

            return client;
        }

        private static AuthorizationServerDescription GetAuthServerDescription()
        {
            //throw new NotImplementedException();
            var authServerDesc = new AuthorizationServerDescription();
            authServerDesc.AuthorizationEndpoint = new Uri(@"https://localhost:44302/OAuth/Auth");
            authServerDesc.TokenEndpoint = new Uri(@"https://localhost:44302/OAuth/Token");
            authServerDesc.ProtocolVersion = ProtocolVersion.V20;
            return authServerDesc;

        }
    }
}