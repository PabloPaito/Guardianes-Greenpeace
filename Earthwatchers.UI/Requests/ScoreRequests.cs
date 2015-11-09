using System;
using System.Collections.Generic;
using System.Windows;
using Earthwatchers.Models;
using RestSharp;
using System.Linq;
using RestSharp.Deserializers;

namespace Earthwatchers.UI.Requests
{
    public class ScoreRequests
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler ScoresReceived;
        public event ChangedEventHandler ScoresInterReceived;
        public event ChangedEventHandler ContestLeaderboardReceived;
        public event ChangedEventHandler ScoreAdded;
        public event ChangedEventHandler ScoreUpdated;
        public event ChangedEventHandler ServerDateTimeReceived;
        public event ChangedEventHandler PositionInRankingReceived;
        private readonly RestClient client;

        public ScoreRequests(string url)
        {
            client = new RestClient(url);
            client.ClearHandlers();
            client.AddHandler("application/json", new JsonDeserializer());

            client.CookieContainer = RequestsHelper.GetCookieContainer();
        }

        public void GetByUser(int earthwatcherid)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest(@"scores?user=" + earthwatcherid, Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<List<Score>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ScoresReceived(response.Data, null)                    
                    ));
        }

        public void GetLeaderBoardNationalRanking(int earthwatcherid)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest(@"scores/leaderboardnational?user=" + earthwatcherid, Method.GET) { RequestFormat = DataFormat.Json };
            
            client.ExecuteAsync<List<Rank>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ScoresReceived(response.Data, null)
                    ));
        }

        public void GetLeaderBoardInternationalRanking(int earthwatcherid)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest(@"scores/leaderboardinternational?user=" + earthwatcherid, Method.GET) { RequestFormat = DataFormat.Json };

            client.ExecuteAsync<List<Rank>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ScoresInterReceived(response.Data, null)
                    ));
        }
        public void GetRankingPosition(int earthwatcherid)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest(@"scores/rankingposition?user=" + earthwatcherid, Method.GET) { RequestFormat = DataFormat.Json };

            client.ExecuteAsync<Rank>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    PositionInRankingReceived(response.Data, null)
                    ));
        }

        public void GetContestLeaderBoard(int earthwatcherid)
        {
            client.Authenticator = new HttpBasicAuthenticator(Current.Instance.Username, Current.Instance.Password);
            var request = new RestRequest(@"scores/contestleaderboard?user=" + earthwatcherid, Method.GET) { RequestFormat = DataFormat.Json };
            client.ExecuteAsync<List<Rank>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ContestLeaderboardReceived(response.Data, null)
                    ));
        }

        public void GetServerTime()
        {
            var request = new RestRequest(@"scores/servertime", Method.GET) { RequestFormat = DataFormat.Json, DateFormat = "" };
            client.ExecuteAsync<Score>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ServerDateTimeReceived(response.Data, null)
                    ));
        }

        public void Update(Score score, string username, string password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            var request = new RestRequest("scores/update", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(score);

            client.ExecuteAsync<Score>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ScoreUpdated(response.Data, null)
                    ));
        }

        public void Post(List<Score> scores, string username, string password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);
            var request = new RestRequest("scores", Method.POST) { RequestFormat = DataFormat.Json };
            request.JsonSerializer = new JsonSerializer();
            request.AddBody(scores);
            
            client.ExecuteAsync<List<Score>>(request, response =>
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ScoreAdded(response.Data, null)
                    ));
        }
    }
}
