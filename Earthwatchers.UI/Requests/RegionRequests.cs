using System;
using System.Collections.Generic;
using System.Windows;
using Earthwatchers.Models;
using RestSharp;
using System.Linq;
using RestSharp.Deserializers;

namespace Earthwatchers.UI.Requests
{
    public class RegionRequests
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler AllRegionsReceived;
        public event ChangedEventHandler RegionReceived;
        public event ChangedEventHandler RegionsReceived;
        private readonly RestClient client;

        public RegionRequests(string url)
        {
            client = new RestClient(url);
            client.ClearHandlers();
            client.AddHandler("application/json", new JsonDeserializer());

            client.CookieContainer = RequestsHelper.GetCookieContainer();
        }

        public void GetRegionsByCountryCode(string countryCode)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("region/getbycountrycode/countrycode=" + countryCode, Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<List<Region>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    RegionsReceived(response.Data, null)                    
                    ));
        }

        public void GetAllRegions()
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("region/getall", Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<List<Region>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    AllRegionsReceived(response.Data, null)
                    ));
        }

        public void GetById(int regionId)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("region/getbyid/id=" + regionId, Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<Region>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    RegionReceived(response.Data, null)
                    ));
        }
    }
}
