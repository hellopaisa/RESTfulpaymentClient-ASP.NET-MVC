using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Models
{
    public class HPPaymentResponse
    {
        public int ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionTraceID { get; set; }
        public bool Validity { get; set; }

    }
}