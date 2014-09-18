using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Code
{
    public static class Helper
    {
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
        }

        private static bool RemoteCertificateValidate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}