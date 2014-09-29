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
            //We have use DotNetOpenAuth as a OAuth Client library.

            //Since we have used the same url to initiate the request and receive the response as a callback URL,
            //Check the QueryString whether it's a response or not.

            if (string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                //Initiate a request to the Hello-Paisa Authorization Server
                return InitAuth();
            }
            else
            {
                //Response received from the authorization server.
                return OAuthCallback();
            }
        }

        //creating an instance of the client
        static WebServerClient client = AuthHelper.CreateClient();

        /// <summary>
        /// Initializes the request for authorization
        /// </summary>
        /// <returns></returns>
        private ActionResult InitAuth()
        {
            var state = new AuthorizationState();

            var uri = Request.Url.AbsoluteUri;
            uri = RemoveQueryStringFromUri(uri);
            state.Callback = new Uri(uri);

            //In the scope, please add only a single element, which is the Transaction amount, else the request will be rejected.
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
        /// Response from the Hello Paisa Authorization Server
        /// </summary>
        /// <returns></returns>
        private ActionResult OAuthCallback()
        {
            //create instance of TokenInfo Object
            var _tokenInfoObj = new TokenInfo();

            try
            {
                //bypassing the HTTPS security, as the certificate in our test server is self signed.
                Helper.SetCertificatePolicy();

                //process the response
                var auth = client.ProcessUserAuthorization(this.Request);

                if (auth != null)
                {
                    Authorization = auth;

                    //assigning the received access-token to the AccessToken Property of tokenInfo object.
                    _tokenInfoObj.AccessToken = auth.AccessToken;

                }                
                
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
                    _httpClient.BaseAddress = new Uri(@"https://test.hellopaisa.com.np/");
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
                                //check the status of the transaction, if it's validity is true and the responseCode is 0, the the transaction is successful.
                                if (_transactionResponse.Validity == true && _transactionResponse.ResponseCode == 0)
                                {
                                    //success
                                    var msg = " Your transaction is " + _transactionResponse.ResponseMessage + ", Transaction Trace ID=" + _transactionResponse.TransactionTraceID;
                                    ViewBag.Message = msg;
                                }
                                else
                                {
                                    //transaction failed
                                    var errorMsg = "Transaction Failed. " + _transactionResponse.ResponseMessage + " , error Code=" + _transactionResponse.ResponseCode;
                                    ViewBag.Message = errorMsg;
                                }
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Invalid TokenInfo Status";
                        }

                    }
                    else
                    {
                        ViewBag.Message = "TokenInfo not found." + _responseForTokenInfo.StatusCode;
                    }
                }
            }

            return View();

        }

    }
}
