using DotNetOpenAuth.OAuth2;
using RESTfulApiClient_ASP.NET_MVC.Code;
using RESTfulApiClient_ASP.NET_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;

namespace RESTfulApiClient_ASP.NET_MVC.Controllers
{
    public class HelloPaisaPaymentController : Controller
    {
        private static IAuthorizationState Authorization
        {
            get { return (AuthorizationState)System.Web.HttpContext.Current.Session["Authorization"]; }
            set { System.Web.HttpContext.Current.Session["Authorization"] = value; }
        }

        //
        // GET: /HelloPaisaPayment/

        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                //Initial request
                return InitAuth();
            }
            else
            {
                //response of the authorization
                return OAuthCallback();
            }
        }

        //creating the instance of the client
        static WebServerClient client = AuthHelper.CreateClient();

        /// <summary>
        /// Initialize the request for authorization
        /// </summary>
        /// <returns></returns>
        private ActionResult InitAuth()
        {
            var state = new AuthorizationState();

            var uri = Request.Url.AbsoluteUri;
            uri = RemoveQueryStringFromUri(uri);
            state.Callback = new Uri(uri);

            //Transaction Amount
            state.Scope.Add("15");

            var r = client.PrepareRequestUserAuthorization(state);
            return r.AsActionResult();

        }


        private static string RemoveQueryStringFromUri(string uri)
        {
            int index = uri.IndexOf('?');
            if (index > -1)
            {
                uri = uri.Substring(0, index);
            }
            return uri;
        }

        /// <summary>
        /// Response from the authorization
        /// </summary>
        /// <returns></returns>
        private ActionResult OAuthCallback()
        {
            var _tokenInfoObj = new TokenInfo();

            try
            {
                //bypassing the HTTPS security, as the certificate in our test server is self signed.
                Helper.SetCertificatePolicy();

                //the response from the authorization
                var auth = client.ProcessUserAuthorization(this.Request);

                if (auth != null)
                {
                    Authorization = auth;


                    _tokenInfoObj.AccessToken = auth.AccessToken;

                }

                ViewBag.Message += auth.AccessToken;
            }
            catch (Exception ex)
            {
                var p = ex.ToString();
                ViewBag.Message = p.ToString();

            }


            return View(_tokenInfoObj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HPTransaction(TokenInfo tokenInfo)
        {
            if (tokenInfo != null)
            {

                //STEPS: 
                //1. get token info 
                //2. validate token 
                //3. complete payment

                //get token info
                using (var _httpClient = new HttpClient())
                {
                    _httpClient.BaseAddress = new Uri("https://localhost:44302/");
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //make the request for the token info
                    HttpResponseMessage _responseForTokenInfo = _httpClient.GetAsync("api/TokenInfo?token=" + tokenInfo.AccessToken).Result;

                    //if the status code is success, then
                    if (_responseForTokenInfo.IsSuccessStatusCode)
                    {
                        //retrive the token info object
                        var _tokenInfo = _responseForTokenInfo.Content.ReadAsAsync<TokenInfo>().Result;

                        //now check the validation of the tokenInfo
                        var _responseForTokenValidation = _httpClient.PostAsJsonAsync("api/ValidateTokenInfo", _tokenInfo);

                        var _tokenValidationStatus = _responseForTokenValidation.Result.Content.ReadAsAsync<TokenStatus>().Result;


                        //check if the token status is valid or not
                        if (_tokenValidationStatus.IsTokenValid == true)
                        {
                            //enter the transaction OTP, entered by the user                           

                            _tokenInfo.TransactionOTP = tokenInfo.TransactionOTP;
                            //call the complete payment function
                            var _paymentResponse = _httpClient.PostAsJsonAsync("api/CompletePayment", _tokenInfo);

                            var _transactionResponse = _paymentResponse.Result.Content.ReadAsAsync<Transaction>().Result;

                            if (_transactionResponse != null)
                            {
                                ViewBag.Message = _transactionResponse.ResponseMessage + "   ::::::   ";
                            }

                        }
                    }
                }
            }

            return View();

        }

    }
}
