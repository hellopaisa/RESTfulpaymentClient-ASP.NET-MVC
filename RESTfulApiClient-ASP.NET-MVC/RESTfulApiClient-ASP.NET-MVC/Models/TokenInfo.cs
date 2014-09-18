using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Models
{
    public class TokenInfo
    {
        public string Code { get; set; }
        public double Amount { get; set; }
        public string TransactionTraceID { get; set; }
        public long ClientID { get; set; }
        public string AccessToken { get; set; }

        public string TransactionOTP { get; set; }
    }
}