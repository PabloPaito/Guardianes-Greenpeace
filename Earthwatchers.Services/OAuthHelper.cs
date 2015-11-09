using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Earthwatchers.Services
{
    public class OAuthHelper
    {
        private string oauth_consumer_key;
        private string oauth_consumer_secret;
        private string callbackUrl;
        private string REQUEST_TOKEN;
        private string AUTHORIZE;
        private string ACCESS_TOKEN;
        private string GET_USER_DATA;
        private string POST_SHOUT;
        private httpMethod method;

        public OAuthHelper(string apiName)
        {
            if (apiName == "Twitter")
            {
                //TWITTER
                this.oauth_consumer_key = "fKk4hoqdLtgRSY1Y0NHs0pBEs";
                this.oauth_consumer_secret = "csUwFWY8W3urhyNM3DQAOFXwek3ibMstQOEMLl7Q7UdZusvL30";
                //this.callbackUrl = "http://localhost:1305/home.aspx"; // DEV
                //this.callbackUrl = "http://guardianes2.iantech.net/home.aspx"; //QA
                this.callbackUrl = "http://guardianes.greenpeace.org.ar/home.aspx"; //PROD

                this.REQUEST_TOKEN = "https://api.twitter.com/oauth/request_token";
                this.AUTHORIZE = "https://api.twitter.com/oauth/authenticate";
                this.ACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";
                this.method = httpMethod.POST;
            }
            else if (apiName == "Taringa")
            {
                this.oauth_consumer_key = "16";
                this.oauth_consumer_secret = "comsB3p64xXeoAAvUyo7N3XXoLUkt97W";
                //this.callbackUrl = "http://localhost:1305/home.aspx"; // DEV
                //this.callbackUrl = "http://guardianes2.iantech.net/home.aspx"; // QA
                this.callbackUrl = "http://guardianes.greenpeace.org.ar/home.aspx"; // PROD

                this.REQUEST_TOKEN = "http://api.taringa.net/oauth/request_token";
                this.AUTHORIZE = "http://api.taringa.net/oauth/authorize";
                this.ACCESS_TOKEN = "http://api.taringa.net/oauth/access_token";
                this.GET_USER_DATA = "http://api.taringa.net/user/view";
                this.POST_SHOUT = "http://api.taringa.net/shout/create";
                this.method = httpMethod.GET;
            }
        }

        #region Common Methods

        public enum httpMethod
        {
            POST, GET
        }
        public string oauth_request_token { get; set; }
        public string oauth_access_token { get; set; }
        public string oauth_access_token_secret { get; set; }
        public string user_id { get; set; }
        public string screen_name { get; set; }
        public string oauth_error { get; set; }
        // Guarda la cookie con el nombre de la Red social
        public static void GenerateOauthCookie(string OauthApi)
        {
            System.Web.HttpContext.Current.Response.Cookies.Remove("OAUTH_API");
            HttpCookie cookie = new HttpCookie("OAUTH_API");
            cookie.Value = OauthApi;
            cookie.Expires = DateTime.UtcNow.AddMinutes(30);
            cookie.HttpOnly = false;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }
        //Recupera la cookie con el nombre de la red social
        public static string GetCookieInfo()
        {
            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies["OAUTH_API"];
            return cookie.Value;
        }

        public string GetRequestToken()
        {
            HttpWebRequest request = FetchRequestToken(method, oauth_consumer_key, oauth_consumer_secret);
            string result = getResponce(request);
            Dictionary<string, string> resultData = OAuthUtility.GetQueryParameters(result);
            if (resultData.Keys.Contains("oauth_token"))
            {
                //Guarda la cookie ots "OauthTokenSecret" Para persistir el dato que hay que enviar para el signature de taringa
                System.Web.HttpContext.Current.Response.Cookies.Remove("Ots");
                HttpCookie cookie = new HttpCookie("Ots");
                cookie.Value = resultData["oauth_token_secret"];
                cookie.Expires = DateTime.UtcNow.AddMinutes(30);
                cookie.HttpOnly = false;
                System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

                return resultData["oauth_token"];
            }
            else
            {
                this.oauth_error = result;
                return "";
            }
        }
        HttpWebRequest FetchRequestToken(httpMethod method, string oauth_consumer_key, string oauth_consumer_secret)
        {
            string OutUrl = "";
            string OAuthHeader = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(REQUEST_TOKEN), method.ToString(), oauth_consumer_key, oauth_consumer_secret, "", "", out OutUrl, callbackUrl);

            if (method == httpMethod.GET)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(OutUrl + "?" + OAuthHeader);
                request.Method = method.ToString();
                return request;
            }
            else if (method == httpMethod.POST)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(OutUrl);
                request.Method = method.ToString();
                request.Headers["Authorization"] = OAuthHeader;
                return request;
            }
            else
                return null;
        }
        public string GetAuthorizeUrl(string requestToken)
        {
            return string.Format("{0}?oauth_token={1}", AUTHORIZE, requestToken);
        }
        HttpWebRequest FetchAccessToken(httpMethod method, string oauth_consumer_key, string oauth_consumer_secret, string oauth_token, string oauth_verifier)
        {
            string postData = "oauth_verifier=" + oauth_verifier;
            string AccessTokenURL = string.Format("{0}?{1}", ACCESS_TOKEN, postData);
            string OAuthHeader;
            if (method == httpMethod.GET && oauth_consumer_key == "16") //Solo si es de taringa va sin el callback 
            {
                HttpCookie tokenSecret = System.Web.HttpContext.Current.Request.Cookies["Ots"];
                OAuthHeader = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(AccessTokenURL), method.ToString(), oauth_consumer_key, oauth_consumer_secret, oauth_token, tokenSecret.Value, out AccessTokenURL);
            }
            else
            {
                OAuthHeader = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(AccessTokenURL), method.ToString(), oauth_consumer_key, oauth_consumer_secret, oauth_token, "", out AccessTokenURL, callbackUrl);
            }
            if (method == httpMethod.GET)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AccessTokenURL + "?" + OAuthHeader);
                request.Method = method.ToString();
                return request;
            }
            else if (method == httpMethod.POST)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AccessTokenURL);
                request.Method = method.ToString();
                request.Headers["Authorization"] = OAuthHeader;

                byte[] array = Encoding.ASCII.GetBytes(postData);
                request.GetRequestStream().Write(array, 0, array.Length);
                return request;
            }
            else
                return null;
        }
        public static string getResponce(HttpWebRequest request)
        {
            try
            {
                var responseTest = request.GetResponse();
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                string result = reader.ReadToEnd();
                reader.Close();
                return result + "&status=200";
            }
            catch (Exception ex)
            {
                string statusCode = "";
                if (ex.Message.Contains("403"))
                    statusCode = "403";
                else if (ex.Message.Contains("401"))
                    statusCode = "401";
                return string.Format("status={0}&error={1}", statusCode, ex.Message);
            }
        }
        #endregion

        #region Twitter Methods
        public void GetUserTwAccessToken(string oauth_token, string oauth_verifier)
        {
            HttpWebRequest request = FetchAccessToken(method, oauth_consumer_key, oauth_consumer_secret, oauth_token, oauth_verifier);
            string result = getResponce(request);

            Dictionary<string, string> resultData = OAuthUtility.GetQueryParameters(result);
            if (resultData.Keys.Contains("oauth_token"))
            {
                this.oauth_access_token = resultData["oauth_token"];
                this.oauth_access_token_secret = resultData["oauth_token_secret"];
                this.user_id = resultData["user_id"];
                this.screen_name = resultData["screen_name"];
            }
            else
                this.oauth_error = result;
        }
        public void TweetOnBehalfOf(string oauth_access_token, string oauth_token_secret, string postData)
        {
            HttpWebRequest request = PostTwits(oauth_consumer_key, oauth_consumer_secret, oauth_access_token, oauth_token_secret, postData);
            string result = OAuthHelper.getResponce(request);
            Dictionary<string, string> dcResult = OAuthUtility.GetQueryParameters(result);
            if (dcResult["status"] != "200")
            {
                this.oauth_error = result;
            }

        }
        HttpWebRequest PostTwits(string oauth_consumer_key, string oauth_consumer_secret, string oauth_access_token, string oauth_token_secret, string postData)
        {
            postData = "trim_user=true&include_entities=true&status=" + postData;
            string updateStatusURL = "https://api.twitter.com/1/statuses/update.json?" + postData;

            string outUrl;
            string OAuthHeaderPOST = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(updateStatusURL), httpMethod.POST.ToString(), oauth_consumer_key, oauth_consumer_secret, oauth_access_token, oauth_token_secret, out outUrl, callbackUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(outUrl);
            request.Method = httpMethod.POST.ToString();
            request.Headers["Authorization"] = OAuthHeaderPOST;

            byte[] array = Encoding.ASCII.GetBytes(postData);
            request.GetRequestStream().Write(array, 0, array.Length);
            return request;

        }

        #endregion

        #region Taringa Methods
        public void GetUserTaringaAccessToken(string oauth_token, string oauth_verifier)
        {
            HttpWebRequest request = FetchAccessToken(method, oauth_consumer_key, oauth_consumer_secret, oauth_token, oauth_verifier);
            string result = getResponce(request);

            Dictionary<string, string> resultData = OAuthUtility.GetQueryParameters(result);
            if (resultData.Keys.Contains("oauth_token"))
            {
                this.oauth_access_token = resultData["oauth_token"];
                this.oauth_access_token_secret = resultData["oauth_token_secret"];
            }
            else
                this.oauth_error = result;
        }
        public void GetTaringaUserData(string oauth_access_token, string oauth_access_token_secret)
        {
            HttpWebRequest request = FetchTaringaUserData(method, oauth_consumer_key, oauth_consumer_secret, oauth_access_token, oauth_access_token_secret);
            string result = getResponce(request);

            var userData = result.Substring(1).Replace("\"", string.Empty).Split(',');
            if (userData[0].StartsWith("id:"))
            {
                this.user_id = userData[0].Substring(3);
                this.screen_name = userData[2].Substring(5);
            }
            else
                this.oauth_error = result;
        }
        HttpWebRequest FetchTaringaUserData(httpMethod method, string oauth_consumer_key, string oauth_consumer_secret, string oauth_access_token, string oauth_access_token_secret)
        {
            string UserDataTokenURL = "";
            string OAuthHeader;
            OAuthHeader = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(GET_USER_DATA), method.ToString(), oauth_consumer_key, oauth_consumer_secret, oauth_access_token, oauth_access_token_secret, out UserDataTokenURL);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GET_USER_DATA + "?" + OAuthHeader);
            request.Method = method.ToString();
            return request;
        }

        public void PostTaringaShout(string oauth_access_token = "", string oauth_access_token_secret = "", string bodyText = "")
        {
            HttpWebRequest request = FetchTaringaShout(oauth_access_token, oauth_access_token_secret, bodyText);
            string result = getResponce(request);

            var userData = result.Substring(1).Replace("\"", string.Empty).Split(',');
            if (userData[0].StartsWith("id:"))
            {
                this.user_id = userData[0].Substring(3);
                this.screen_name = userData[2].Substring(5);
            }
            else
                this.oauth_error = result;
        }
        HttpWebRequest FetchTaringaShout(string oauth_access_token, string oauth_access_token_secret, string bodyText)
        {
            if (oauth_access_token != "" && oauth_access_token_secret != "")
            {
                string oauth_consumer_key = this.oauth_consumer_key;
                string oauth_consumer_secret = this.oauth_consumer_secret;

                string shoutUrl = POST_SHOUT; //test 

                string UserDataTokenURL = "";
                string OAuthHeader;
                OAuthHeader = OAuthUtility.GetAuthorizationHeaderForPost_OR_QueryParameterForGET(new Uri(shoutUrl), "POST", oauth_consumer_key, oauth_consumer_secret, oauth_access_token, oauth_access_token_secret, out UserDataTokenURL, "", bodyText);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(POST_SHOUT);
                request.Method = httpMethod.POST.ToString();
                request.ContentType = "application/x-www-form-urlencoded";
                //request.Headers["Body"] = OAuthHeader;
                byte[] array = Encoding.ASCII.GetBytes(OAuthHeader);
                request.GetRequestStream().Write(array, 0, array.Length);
                return request;
            }
            else
            {
                string requestToken = this.GetRequestToken();

                string authorize = "";
                if (string.IsNullOrEmpty(this.oauth_error))
                    authorize = this.GetAuthorizeUrl(requestToken);
                else
                    authorize = this.oauth_error;

                if (HttpContext.Current.Request.QueryString["oauth_token"] != null && HttpContext.Current.Request.QueryString["oauth_verifier"] != null)
                {
                    var OauthCookieApi = GetCookieInfo();

                    string oauth_token = HttpContext.Current.Request.QueryString["oauth_token"];
                    string oauth_verifier = HttpContext.Current.Request.QueryString["oauth_verifier"];

                    GetUserTaringaAccessToken(oauth_token, oauth_verifier);
                    if (string.IsNullOrEmpty(oauth_error))
                    {
                        PostTaringaShout(oauth_access_token, oauth_access_token_secret, bodyText);
                        return null;
                    }
                }
                else if (authorize != this.oauth_error && authorize!= null)
                {
                    HttpContext.Current.Response.Redirect(authorize, true);
                    
                } 
            
            }
            return null;
        }
        #endregion
    }
}
