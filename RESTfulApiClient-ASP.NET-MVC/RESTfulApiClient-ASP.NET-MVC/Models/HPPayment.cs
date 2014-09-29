using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RESTfulApiClient_ASP.NET_MVC.Models
{
    public class HPPayment
    {
        public string ClientIdentifier { get; set; }

        public string ClientSecret { get; set; }

        [Required]      
        public string MobileNumber { get; set; }

        [Required]
        public double Amount { get; set; }

    }
}