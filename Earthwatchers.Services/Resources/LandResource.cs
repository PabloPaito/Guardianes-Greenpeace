using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using Earthwatchers.Data;
using Earthwatchers.Models;
using Earthwatchers.Services.Security;
using Microsoft.ApplicationServer.Http.Dispatcher;
using Microsoft.AspNet.SignalR;
using System.Web;
using System.Linq;
using NLog;

namespace Earthwatchers.Services.Resources
{
    [ServiceContract]
    public class LandResource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ILandRepository landRepository;

        public LandResource(ILandRepository landRepository)
        {
            this.landRepository = landRepository;
        }

        [WebGet(UriTemplate = "{id}")]
        public HttpResponseMessage<Land> Get(int id, HttpRequestMessage request)
        {
            try
            {
                var land = landRepository.GetLand(id);
                if (land == null)
                {
                    return new HttpResponseMessage<Land>(HttpStatusCode.NotFound);
                }

                //LandLinks.AddLinks(land, request);

                return new HttpResponseMessage<Land>(land) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/stats")]
        public HttpResponseMessage<List<Statistic>> GetStats()
        {
            try
            {
                return new HttpResponseMessage<List<Statistic>>(landRepository.GetStats()) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<List<Statistic>>(null) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/verifiedlandscodes", Method = "POST")]
        public HttpResponseMessage<List<string>> GetVerifiedLandsGeoHexCodes(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            try
            {
                return new HttpResponseMessage<List<string>>(landRepository.GetVerifiedLandsGeoHexCodes(earthwatcher.Id, earthwatcher.IsPowerUser)) { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage<List<string>>(new List<string> { ex.Message }) { StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebGet(UriTemplate = "/landstoconfirm/{page}/{showVerifieds}/{regionId}")]
        public HttpResponseMessage GetLandsToConfirm(int page, bool showVerifieds, int regionId, HttpRequestMessage request)
        {
            return new HttpResponseMessage<List<LandCSV>>(landRepository.GetLandsToConfirm(page, showVerifieds, regionId)) { StatusCode = HttpStatusCode.OK };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebGet(UriTemplate = "/landscsv")]
        public HttpResponseMessage GetLandsCSV(HttpRequestMessage request)
        {
            return new HttpResponseMessage<List<LandCSV>>(landRepository.GetLandsCSV()) { StatusCode = HttpStatusCode.OK };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/generateimages", Method = "POST")]
        public HttpResponseMessage GenerateImages(HttpRequestMessage request)
        {
            try
            {
                //Genero las imágenes
                ImagesGeneratorTool.Run(landRepository, true, null);

                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/generategameimages", Method = "POST")]
        public HttpResponseMessage GenerateGameImages(HttpRequestMessage request)
        {
            try
            {
                //Genero las imágenes
                ImagesGeneratorTool.Run(landRepository, false, null);

                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/updatelandsdemand", Method = "POST")]
        public HttpResponseMessage UpdateLandsDemand(List<Land> lands, HttpRequestMessage<List<Land>> request)
        {
            var connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["EarthwatchersConnection"].ConnectionString;
            var earthwatcher = Session.HasLoggedUser() ? new EarthwatcherRepository(connectionstring).GetEarthwatcher(Session.GetCookieInfo().EarthwatcherName, false) : null;

            var landsToConfirm = lands.Where(l => l.Reset == null || l.Reset == false).ToList();
            var landsToReset = lands.Where(l => l.Reset.HasValue && l.Reset == true).ToList();

            try
            {
                if (landsToReset.Any())
                {
                    landRepository.ForceLandsReset(landsToReset, earthwatcher != null ? earthwatcher.Id : 0);
                }

                if(landsToConfirm.Any())
                {
                    if (landRepository.UpdateLandsDemand(lands, earthwatcher != null ? earthwatcher.Id : 0))
                    {
                        //Genero las imágenes
                        ImagesGeneratorTool.Run(landRepository, true, null);

                        //Send notification emails
                        SendEmail_GreenpeaceConfirmation(lands, earthwatcher);
                    }
                }

                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(ex.Message);
            }

            
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/resetlands/{regionId}", Method = "POST")]
        public HttpResponseMessage ResetLands(int regionId, HttpRequestMessage request)
        {
            if (landRepository.ResetLands(regionId))
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/mar", Method = "POST")]
        public HttpResponseMessage MassiveReassign(HttpRequestMessage request)
        {
            if (landRepository.MassiveReassign(0))
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/addpoll", Method = "POST")]
        public HttpResponseMessage AddPoll(LandMini land, HttpRequestMessage<LandMini> request)
        {
            try
            {
                landRepository.AddPoll(land);
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.Created };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
            }
        }

        [WebInvoke(UriTemplate = "/intersect", Method = "POST")]
        public HttpResponseMessage<List<Land>> GetLandByWkt(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            if (earthwatcher != null && !string.IsNullOrEmpty(earthwatcher.Name))
            {
                // todo: check if wkt is valid...
                // we can use SqlGeometry but there are some issues on Azure with that assembly
                // other option is do a spatial database query for this

                var landCollection = landRepository.GetLandByIntersect(earthwatcher.Name, earthwatcher.Id);

                return new HttpResponseMessage<List<Land>>(landCollection) { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage<List<Land>>(null) { StatusCode = HttpStatusCode.BadRequest };
        }

        [WebInvoke(UriTemplate = "/all", Method = "POST")]
        public HttpResponseMessage<List<Land>> GetAll(Earthwatcher e, HttpRequestMessage<Earthwatcher> request)
        {
            try
            {
                var landCollection = landRepository.GetAll(e.Id, e.PlayingRegion);
                return new HttpResponseMessage<List<Land>>(landCollection) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<Land>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebGet(UriTemplate = "/activity={id}")]
        public HttpResponseMessage<List<Score>> GetActivity(int id, HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<List<Score>>(landRepository.GetLastUsersWithActivityScore(id)) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<Score>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [WebGet(UriTemplate = "/status={landStatus}")]
        public HttpResponseMessage<List<Land>> GetAllByStatus(LandStatus landStatus, HttpRequestMessage request)
        {
            var landCollection = landRepository.GetLandByStatus(landStatus);
            foreach (var land in landCollection)
            {
                LandLinks.AddLinks(land, request);
            }
            return new HttpResponseMessage<List<Land>>(landCollection) { StatusCode = HttpStatusCode.OK };
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/updatestatus", Method = "POST")]
        public HttpResponseMessage<Land> UpdateStatusLand(Land land, HttpRequestMessage<Land> request)
        {
            var landDB = landRepository.GetLand(land.Id);
            if (landDB != null)
            {
                landRepository.UpdateLandStatus(land.Id, land.LandStatus);
                try 
                {
                    ImagesGeneratorTool.Run(landRepository, true, landDB.GeohexKey);
                }
                catch(Exception ex)
                {
                    logger.Error("No se genero la imagen para la land " + landDB.Id.ToString());
                }
                return new HttpResponseMessage<Land>(land) { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage<Land>(HttpStatusCode.BadRequest);
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/confirm", Method = "POST")]
        public HttpResponseMessage Confirm(LandMini land, HttpRequestMessage<LandMini> request)
        {
            try
            {
                LandMini landMini = landRepository.AddVerification(land, false); //TODO: revisar que no llegue nulo
                VerificationScoring(landMini);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = ex.Message };
            }
        }

        [WebGet(UriTemplate = "/getCheckPercentage={regionId}")]
        public HttpResponseMessage<decimal> GetCheckPercentage(int regionId, HttpRequestMessage request)
        {
            try
            {
                decimal percentage = landRepository.GetCheckPercentageByRegionId(regionId);
                return new HttpResponseMessage<decimal>(percentage) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<decimal>(HttpStatusCode.InternalServerError);
            }
        }

        [WebInvoke(UriTemplate = "/getPresicionDenouncePercentage", Method="POST")]
        public HttpResponseMessage<decimal?> GetPresicionPercentage(Earthwatcher e, HttpRequestMessage<Earthwatcher> request)
        {
            try
            {
                decimal? percentage = landRepository.GetPrecicionDenouncePercentageByRegionId(e.PlayingRegion, e.Id);
                return new HttpResponseMessage<decimal?>(percentage) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<decimal?>(HttpStatusCode.InternalServerError);
            }
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/deconfirm", Method = "POST")]
        public HttpResponseMessage Deconfirm(LandMini land, HttpRequestMessage<LandMini> request)
        {
            try
            {
                LandMini landMini = landRepository.AddVerification(land, true);
                VerificationScoring(landMini);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, ReasonPhrase = ex.Message };
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebGet(UriTemplate = "/settutorland/code={geoHexCode}")]
        public bool SetTutorLand(string geoHexCode)
        {
            var landDB = landRepository.GetLandByGeoHexKey(geoHexCode);
            if (landDB != null)
            {
                landRepository.UpdateTutorLand(landDB.Id, landDB.RegionId);
                return true;
            }
            else
                return false;
        }

        private void VerificationScoring(LandMini landMini)
        {
            if (landMini != null)
            {
                try
                {
                    //Mando los mails notificando
                    if (!string.IsNullOrEmpty(landMini.Email))
                    {
                        if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["smtp.enabled"]))
                        {
                            List<System.Net.Mail.MailMessage> messages = new List<System.Net.Mail.MailMessage>();

                            System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(landMini.Email);
                            System.Net.Mail.MailAddress addressFrom = new System.Net.Mail.MailAddress(System.Configuration.ConfigurationManager.AppSettings["smtp.user"], Labels.Labels.GuardiansGreenpeace);
                            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                            message.From = addressFrom;
                            message.To.Add(address);
                            message.Subject = Labels.Labels.LandVerifications.ToString();

                            string domain = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);

                            string htmlTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mail.html"));
                            message.Body = string.Format(htmlTemplate, Labels.Labels.LandVerifications2 
                                , Labels.Labels.LandVerifications3
                                , string.Format("{0}/index.html?geohexcode={1}", domain, landMini.GeohexKey), Labels.Labels.LandVerifications4, Labels.Labels.LandVerifications5, Labels.Labels.LandVerifications6, landMini.Email
                                , Labels.Labels.LandVerifications7
                                , Labels.Labels.LandVerifications8, domain);
                            message.IsBodyHtml = true;
                            message.BodyEncoding = System.Text.Encoding.UTF8;
                            message.DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.None;
                            messages.Add(message);

                            SendMails.Send(messages);
                        }
                    }

                    //Genero la imagen de este land
                    ImagesGeneratorTool.Run(landRepository, true, landMini.GeohexKey);

                    //Notify the land owner if logged in
                    var context = GlobalHost.ConnectionManager.GetHubContext<Hubs>();
                    context.Clients.All.LandVerified(landMini.EarthwatcherId);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
       
        private void SendEmail_GreenpeaceConfirmation(List<Land> lands, Earthwatcher earthwatcher)
        {
                //Mando los mails notificando (solo envio si es que tengo habilitado via web.config el envio
                //TODO: Push Notifications para actualizar
                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["smtp.enabled"]))
                {
                    List<System.Net.Mail.MailMessage> messages = new List<System.Net.Mail.MailMessage>();
                    foreach (var land in lands)
                    {
                        string lawYear = land.RegionId == 1? "2008" : "2013";

                        if (land.Confirmed.HasValue)
                        {
                            System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(land.EarthwatcherName);
                            System.Net.Mail.MailAddress addressFrom = new System.Net.Mail.MailAddress(System.Configuration.ConfigurationManager.AppSettings["smtp.user"], Labels.Labels.GuardiansGreenpeace);
                            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                            message.From = addressFrom;
                            message.To.Add(address);
                            string domain = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);
                            string htmlTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mail.html"));
                            //Confirmados en Amarillo 
                            if (land.Confirmed.Value && land.LandStatus == LandStatus.Alert)
                            {
                                message.Subject = Labels.Labels.LandVerified;
                                message.Body = string.Format(htmlTemplate, Labels.Labels.Congrats
                                , Labels.Labels.LandVerified2
                                , string.Format("{0}/index.html?geohexcode={1}", domain, land.GeohexKey), Labels.Labels.LandVerifications4, Labels.Labels.LandVerifications5, Labels.Labels.LandVerifications6, land.EarthwatcherName
                                , Labels.Labels.LandVerifications7
                                , Labels.Labels.LandVerifications8, domain);
                            }

                            //Confirmados en Verde
                            if (land.Confirmed.Value && land.LandStatus == LandStatus.Ok)
                            {
                                message.Subject = Labels.Labels.LandVerified;
                                message.Body = string.Format(htmlTemplate, Labels.Labels.Congrats
                                , Labels.Labels.ReportVerifiedCorrect
                                , string.Format("{0}/index.html?geohexcode={1}", domain, land.GeohexKey), Labels.Labels.LandVerifications4, Labels.Labels.LandVerifications5, Labels.Labels.LandVerifications6, land.EarthwatcherName
                                , Labels.Labels.LandVerifications7
                                , Labels.Labels.LandVerifications8, domain);
                            }

                            //Rechazados en Verde 
                            if (!land.Confirmed.Value && land.LandStatus == LandStatus.Ok)
                            {
                                message.Subject = Labels.Labels.VerificationReport;
                                message.Body = string.Format(htmlTemplate, Labels.Labels.IncorrectReport
                                ,Labels.Labels.IncorrectReport2 +
                                    @"<table style='padding: 10px; margin: 10px; border-collapse: collapse;' cellpadding='10' cellspacing='0'>
                                        <tr>
                                    <td><img src='" + domain + "/SatelliteImages/demand/" + land.GeohexKey + "-a.jpg' galleryimg='no' /></td><td><img src='" + domain + "/SatelliteImages/demand/" + land.GeohexKey + "-a.jpg' galleryimg='no' /></td></tr><tr><td style='font-family: Arial; font-size: 11px;'>"+ lawYear +"</td><td style='font-family: Arial; font-size: 11px;'>"+Labels.Labels.Now+"</td></tr></table><br /><br />"
                                , string.Format("{0}/index.html?geohexcode={1}", domain, land.GeohexKey), Labels.Labels.LandVerifications4, Labels.Labels.LandVerifications5, Labels.Labels.LandVerifications6, land.EarthwatcherName
                                , Labels.Labels.LandVerifications7
                                , Labels.Labels.LandVerifications8, domain);
                            }

                            //Rechazados en Amarillo 
                            if (!land.Confirmed.Value && land.LandStatus == LandStatus.Alert)
                            {
                                message.Subject = Labels.Labels.VerificationReport;
                                message.Body = string.Format(htmlTemplate, Labels.Labels.IncorrectReport
                                , string.Format(Labels.Labels.IncorrectReport3, lawYear, lawYear) +
                                    @"<table style='padding: 10px; margin: 10px; border-collapse: collapse;' cellpadding='10' cellspacing='0'>
                                        <tr>
                                    <td><img src='" + domain + "/SatelliteImages/demand/" + land.GeohexKey + "-a.jpg' galleryimg='no' /></td><td><img src='" + domain + "/SatelliteImages/demand/" + land.GeohexKey + "-a.jpg' galleryimg='no' /></td></tr><tr><td style='font-family: Arial; font-size: 11px;'>" + lawYear + "</td><td style='font-family: Arial; font-size: 11px;'>" + Labels.Labels.Now + "</td></tr></table><br /><br />"
                                , string.Format("{0}/index.html?geohexcode={1}", domain, land.GeohexKey), Labels.Labels.LandVerifications4, Labels.Labels.LandVerifications5, Labels.Labels.LandVerifications6, land.EarthwatcherName
                                , Labels.Labels.LandVerifications7
                                , Labels.Labels.LandVerifications8, domain);
                            }

                            message.IsBodyHtml = true;
                            message.BodyEncoding = System.Text.Encoding.UTF8;
                            message.DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.None;
                            messages.Add(message);
                        }
                    }

                    SendMails.Send(messages);
                }
        }
    
    }
}