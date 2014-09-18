using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Models
{
    public class Transaction
    {
        public string TransactionTraceID { get; set; }
        public string CustomerFirstName { get; set; }
        public bool Validity { get; set; }
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}