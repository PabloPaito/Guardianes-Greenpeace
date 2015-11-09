using Earthwatchers.Data;
using Earthwatchers.Models;
using Earthwatchers.Models.Portable;
using Earthwatchers.Services.Security;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;

namespace Earthwatchers.Services.Resources
{
    [ServiceContract]
    public class ScoresResource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IScoreRepository scoreRepository;
        private string connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["EarthwatchersConnection"].ConnectionString;

        public ScoresResource(IScoreRepository scoreRepository)
        {
            this.scoreRepository = scoreRepository;
        }

        /// <summary>
        /// Devuelve el listado de scores PARA EL PAIS ACTUAL DE JUEGO del EW
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "?user={userid}")]
        public HttpResponseMessage<List<Score>> GetScoresByUserId(int userid, HttpRequestMessage request)
        {
            var scoreCollection = scoreRepository.GetScoresByUserId(userid);

            return scoreCollection != null ?
                new HttpResponseMessage<List<Score>>(scoreCollection) { StatusCode = HttpStatusCode.OK } :
                new HttpResponseMessage<List<Score>>(null) { StatusCode = HttpStatusCode.BadRequest };
        }

        [WebGet(UriTemplate = "/servertime")]
        public HttpResponseMessage<Score> GetServerTime()
        {
            try
            {
                return new HttpResponseMessage<Score>( new Score {Published = DateTime.UtcNow}) { StatusCode = HttpStatusCode.OK };

            }
            catch (Exception ex)
            {
                logger.ErrorException("Excepcion, no se ejecuto servertime", ex);
                return new HttpResponseMessage<Score>(new Score { Published = new DateTime() }) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/leaderboardnational?user={userid}")]
        public HttpResponseMessage<List<Rank>> GetLeaderBoardNationalRanking(int userid, HttpRequestMessage request) 
        {
            try
            {
                IEarthwatcherRepository ewRepo = new EarthwatcherRepository(connectionstring);
                var earthwatcher = ewRepo.GetEarthwatcher(userid);

                var leaderBoard = scoreRepository.GetLeaderBoardNationalRanking(false, earthwatcher.PlayingRegion);
                var rankingCollection = leaderBoard.Take(10).ToList();
                if (!rankingCollection.Any(x => x.EarthwatcherId == earthwatcher.Id))
                {
                    rankingCollection.Add(leaderBoard.Where(x => x.EarthwatcherId == earthwatcher.Id).First());
                }
                foreach (var r in rankingCollection)
                {
                    //si tiene nick le dejo el nick
                    if(!string.IsNullOrEmpty(r.Nick))
                    {
                        r.Name = r.Nick;
                    }
                    //si no tiene name (tw o fb) le dejo el nick
                    if (string.IsNullOrEmpty(r.Name))
                    {
                        r.Name = r.Nick; 
                    }
                }
                return new HttpResponseMessage<List<Rank>>(rankingCollection) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<List<Rank>>(null) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }
       
        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/leaderboardinternational?user={userid}")]
        public HttpResponseMessage<List<Rank>> GetLeaderBoardInternationalRanking(int userid, HttpRequestMessage request)
        {
            try
            {
                var leaderBoard = scoreRepository.GetLeaderBoardInternationalRanking();
                var rankingCollection = leaderBoard.Take(10).ToList();
                if (!rankingCollection.Any(x => x.EarthwatcherId == userid))
                {
                    rankingCollection.Add(leaderBoard.Where(x => x.EarthwatcherId == userid).First());
                }
                foreach (var r in rankingCollection)
                {
                    //si tiene nick le dejo el nick
                    if (!string.IsNullOrEmpty(r.Nick))
                    {
                        r.Name = r.Nick;
                    }
                    //si no tiene name (tw o fb) le dejo el nick
                    if (string.IsNullOrEmpty(r.Name))
                    {
                        r.Name = r.Nick;
                    }
                    r.flagUrl = string.Format("/Resources/Images/Flags/{0}-35.png", r.PlayingRegion);
                }
                return new HttpResponseMessage<List<Rank>>(rankingCollection) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<List<Rank>>(null) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/rankingposition?user={userid}")]
        public HttpResponseMessage<Rank> GetRankingPosition(int userid, HttpRequestMessage request)
        {
            try
            {
                var leaderBoard = scoreRepository.GetLeaderBoardInternationalRanking();
                var userRank = leaderBoard.Where(x => x.EarthwatcherId == userid).FirstOrDefault();
                
                return new HttpResponseMessage<Rank>(userRank) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<Rank>(null) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/contestleaderboard?user={userid}")]
        public HttpResponseMessage<List<Rank>> GetContestLeaderBoard(int userid, HttpRequestMessage request)
        {
            try
            {
                IEarthwatcherRepository ewRepo = new EarthwatcherRepository(connectionstring);
                var earthwatcher = ewRepo.GetEarthwatcher(userid);

                var leaderBoard = scoreRepository.GetLeaderBoardNationalRanking(true, earthwatcher.PlayingRegion);
                var rankingCollection = leaderBoard.Take(10).ToList();
                if (!rankingCollection.Any(x => x.EarthwatcherId == userid))
                {
                    rankingCollection.Add(leaderBoard.Where(x => x.EarthwatcherId == userid).First());
                }
                foreach (var r in rankingCollection)
                {
                    if (string.IsNullOrEmpty(r.Name))
                    {
                        r.Name = r.Nick;
                    }
                }

                return new HttpResponseMessage<List<Rank>>(rankingCollection) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<List<Rank>>(null) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/update", Method = "POST")]
        public HttpResponseMessage<Score> UpdateScore(Score score, HttpRequestMessage<Score> request)
        {
            if (score.EarthwatcherId != 0 &&
                !String.IsNullOrEmpty(score.Action) &&
                score.Points != 0)
            {
                var newScore = scoreRepository.UpdateScore(score);
                var response = new HttpResponseMessage<Score>(newScore) { StatusCode = HttpStatusCode.OK };
                response.Headers.Location = new Uri(newScore.Uri, UriKind.Relative);
                return response;
            }
            else
            {
                var response = new HttpResponseMessage<Score>(null) { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "request parameters not correct" };
                return response;
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "", Method = "POST")]
        public HttpResponseMessage<List<Score>> PostScore(List<Score> scores, HttpRequestMessage<List<Score>> request)
        {
            try
            {
                List<Score> newScores = new List<Score>();
                foreach (var score in scores)
                {
                    if (score.EarthwatcherId != 0 && !String.IsNullOrEmpty(score.Action))
                    {
                        newScores.Add(scoreRepository.PostScore(score));

                    }
                }

                var response = new HttpResponseMessage<List<Score>>(newScores) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage<List<Score>>(null) { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = "request parameters not correct" };
                return response;
            }
        }
    }
}