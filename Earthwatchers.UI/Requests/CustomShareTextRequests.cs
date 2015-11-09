using System;
using System.Collections.Generic;
using System.Windows;
using Earthwatchers.Models;
using RestSharp;
using System.Linq;
using RestSharp.Deserializers;

namespace Earthwatchers.UI.Requests
{
    public class CustomShareTextRequests
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler AllShareTextReceived;
        public event ChangedEventHandler TextsReceived;
        public event ChangedEventHandler TextReceived;
        private readonly RestClient client;

        public CustomShareTextRequests(string url)
        {
            client = new RestClient(url);
            client.ClearHandlers();
            client.AddHandler("application/json", new JsonDeserializer());

            client.CookieContainer = RequestsHelper.GetCookieContainer();
        }

        public void GetAllShareTexts()
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("customsharetext/getall", Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<List<CustomShareText>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    AllShareTextReceived(response.Data, null)
                    ));
        }

        public void GetShareTextById()
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("customsharetext/getbyId", Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<CustomShareText>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    TextReceived(response.Data, null)
                    ));
        }

        public void GetByRegionIdAndLanguage(int regionId, string language)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("customsharetext/getbyregionidandlanguage", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            var shareText = new CustomShareText();
            shareText.RegionId= regionId;
            shareText.Language = language;
            request.AddBody(shareText);
            
            client.ExecuteAsync<CustomShareText>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    TextsReceived(response.Data, null)
                    ));
        }
    }
}
