using System;
using System.Windows;
using Earthwatchers.Models;
using System.Globalization;
using RestSharp;
using RestSharp.Deserializers;
using Earthwatchers.UI.Extensions;
using System.Windows.Browser;
using System.Linq;

namespace Earthwatchers.UI.Requests
{
    public class EarthwatcherRequests
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler EarthwatcherUpdated;
        public event ChangedEventHandler EarthwatcherReceived;
        public event ChangedEventHandler ApiEwReceived;
        public event ChangedEventHandler LandReassigned;
        public event ChangedEventHandler PasswordChanged;
        public event ChangedEventHandler EarthwatcherExists;
        public event ChangedEventHandler LandReassignedByPlayingRegion;
        public event ChangedEventHandler PlayingRegionChanged;
        private readonly RestClient client;

        public EarthwatcherRequests(string url)
        {
            client = new RestClient(url);
            client.ClearHandlers();
            client.AddHandler("application/json", new JsonDeserializer());


            client.CookieContainer = RequestsHelper.GetCookieContainer();
            
        }

        public void GetByName(string username, string password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            var request = new RestRequest("earthwatchers/getbyname", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(username);

            client.ExecuteAsync<Earthwatcher>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                        EarthwatcherReceived(
                            response.Data, null)
                    ));
        }

        public void LoginWithApi(ApiEw ew)
        {
            var request = new RestRequest("earthwatchers/loginwithapi", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(ew);

            client.ExecuteAsync<Earthwatcher>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                        ApiEwReceived(
                            response.Data, null)
                    ));
        }

        public void GetById(string id)
        {
            var request = new RestRequest(@"earthwatchers/" + id.ToString(CultureInfo.InvariantCulture), Method.GET);
            client.ExecuteAsync<Earthwatcher>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                        EarthwatcherReceived(
                            response.Data, null)
                    ));
        }

        public void GetLogged()
        {
            var request = new RestRequest(@"earthwatchers/getlogged", Method.GET);
            client.ExecuteAsync<Earthwatcher>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                        EarthwatcherReceived(
                            response.Data, null)
                    ));
        }

        public void ReassignLand(Earthwatcher earthwatcher)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);

            var request = new RestRequest("earthwatchers/reassignland", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(earthwatcher);

            client.ExecuteAsync<Land>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    LandReassigned(response.Data, null)
                    ));
        }

        public void ChangePassword(Earthwatcher ew)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);


            var request = new RestRequest("password", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(ew);

            client.ExecuteAsync<Earthwatcher>(request, response =>
               Deployment.Current.Dispatcher.BeginInvoke(() =>
                       PasswordChanged(
                           response.Data, null)
                    ));
        }

        public void Update(Earthwatcher ew)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest("earthwatchers/updateearthwatcher", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(ew);

            client.ExecuteAsync(request, response =>
               Deployment.Current.Dispatcher.BeginInvoke(() =>
                       EarthwatcherUpdated(
                           response.StatusCode, null)
                    ));
        }

        public void ChangePlayingRegion(Earthwatcher earthwatcher)
        {
            var request = new RestRequest("earthwatchers/changeplayingregion", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(earthwatcher);

            client.ExecuteAsync(request, response =>
               Deployment.Current.Dispatcher.BeginInvoke(() =>
                       PlayingRegionChanged(
                           response.StatusCode, null)
                    ));
        }

        public void ReassignLandByPlayingRegion(int earthwatcherId, int regionId)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);

            var request = new RestRequest("earthwatchers/reassignland", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            Earthwatcher e = new Earthwatcher();
            e.PlayingRegion = regionId;
            e.Id = earthwatcherId;
            request.AddBody(e);

            client.ExecuteAsync<Land>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    LandReassignedByPlayingRegion(response.Data, null)
                    ));
        }
    }
}