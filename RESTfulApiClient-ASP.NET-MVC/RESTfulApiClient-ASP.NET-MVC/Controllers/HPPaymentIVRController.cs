using RESTfulApiClient_ASP.NET_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace RESTfulApiClient_ASP.NET_MVC.Controllers
{
    public class HPPaymentIVRController : Controller
    {
        //
        // GET: /HPPaymentIVR/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HPPayment model)
        {
            if(ModelState.IsValid)
            {
                //validate mobile 
                

                //enter the client credentials
                model.ClientIdentifier = "ID";
                model.ClientSecret = "SECRET";


                using (var _httpClient = new HttpClient())
                {
                    _httpClient.BaseAddress = new Uri(@"https://test.hellopaisa.com.np/");
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = _httpClient.PostAsJsonAsync("api/initiatepayment", model);

                    var _transactionResponse = response.Result.Content.ReadAsAsync<HPPaymentResponse>().Result;

                    ViewBag.Message = " validity: " + _transactionResponse.Validity + ", errorMsg=" + _transactionResponse.ErrorMessage + ", transaction Trace ID=" + _transactionResponse.TransactionTraceID;
                    ViewBag.TransactionTraceID = _transactionResponse.TransactionTraceID;
                    return View("Confirmation");                
                }


            }

            return View();
        
        }


        public ActionResult VerifyTransaction(string id)
        {
            if (id == null || id == "")
            { 
                //error
            }

            using (var _httpClient = new HttpClient())
            {
                _httpClient.BaseAddress = new Uri(@"https://test.hellopaisa.com.np/");
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = _httpClient.GetAsync("api/verifytransaction/"+id);

                var _verificationResponse = response.Result.Content.ReadAsAsync<HPPaymentResponse>().Result;

                ViewBag.Message = "Result: " + _verificationResponse.ResponseCode + ", validity=" + _verificationResponse.Validity + ", msg=" + _verificationResponse.ErrorMessage;
                ViewBag.TransactionTraceID = _verificationResponse.TransactionTraceID;
                return View("Confirmation");
            }
            
        }



    }
}
