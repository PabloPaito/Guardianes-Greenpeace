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
using System.Web;
using Microsoft.AspNet.SignalR;
using NLog;
using Earthwatchers.Services.Localization;

namespace Earthwatchers.Services.Resources
{
    [ServiceContract]
    public class EarthwatcherResource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IEarthwatcherRepository earthwatcherRepository;
        private readonly ILandRepository landRepository;
        private readonly IRegionRepository regionRepository;

        public EarthwatcherResource(IEarthwatcherRepository earthwatcherRepository, ILandRepository landRepository, IRegionRepository regionRepository)
        {
            this.earthwatcherRepository = earthwatcherRepository;
            this.landRepository = landRepository;
            this.regionRepository = regionRepository;
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebGet(UriTemplate = "{id}")]
        public HttpResponseMessage<Earthwatcher> Get(int id, HttpRequestMessage request)
        {
            var earthwatcher = earthwatcherRepository.GetEarthwatcher(id);
            if (earthwatcher == null)
            {
                return new HttpResponseMessage<Earthwatcher>(HttpStatusCode.NotFound);
            }
            //EarthwatcherLinks.AddLinks(earthwatcher, request);
            return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.OK };
        }

        [WebInvoke(UriTemplate = "/resetPassword", Method = "POST")]
        public HttpResponseMessage ResetPassword(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            if (earthwatcher != null)
            {
                try
                {
                    //Date
                    var encodedDateBytes = System.Convert.FromBase64String(earthwatcher.Region);
                    string encodedDate = System.Text.Encoding.UTF8.GetString(encodedDateBytes);
                    DateTime date = DateTime.MinValue;
                    DateTime.TryParse(encodedDate, out date);
                    if (date == DateTime.MinValue || date.AddDays(1) < DateTime.UtcNow)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        return response;
                    }

                    var earthwatcherDb = earthwatcherRepository.GetEarthwatcherByGuid(earthwatcher.Guid);
                    if (earthwatcherDb == null)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                        return response;
                    }

                    PasswordResource pwd = new PasswordResource();
                    earthwatcherDb.Password = earthwatcher.Password;
                    pwd.GenerateAndUpdatePassword(earthwatcherDb);
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
                }
            }
            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest };
        }

        [WebInvoke(UriTemplate = "/forgotPassword", Method = "POST")]
        public HttpResponseMessage ForgotPassword(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            if (earthwatcher != null)
            {
                var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(earthwatcher.Name, false);
                if (earthwatcherDb == null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    return response;
                }

                //Mando el mail
                try
                {
                    var dateToEncode = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString());
                    string encodedDate = System.Convert.ToBase64String(dateToEncode);

                    if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["smtp.enabled"]))
                    {
                        List<System.Net.Mail.MailMessage> messages = new List<System.Net.Mail.MailMessage>();

                        System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(earthwatcherDb.Name);
                        System.Net.Mail.MailAddress addressFrom = new System.Net.Mail.MailAddress(System.Configuration.ConfigurationManager.AppSettings["smtp.user"], "Guardianes - Greenpeace");
                        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                        message.From = addressFrom;
                        message.To.Add(address);
                        message.Subject = "Reseteá tu contraseña de Guardianes - Reset your Guardians Password";

                        string domain = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);

                        string htmlTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mail.html"));
                        message.Body = string.Format(htmlTemplate, "Estimado Guardian,", "Para resetear tu contraseña hacé un click en el botón verde [Resetear Contraseña].<br><br> Por motivos de seguridad, este cambio se podrá hacer en el transcurso de las próximas 24 horas"
                            , string.Format("{0}/resetpwd.html?guid={1}&ed={2}", domain, earthwatcherDb.Guid, encodedDate), "Click aquí para resetear tu contraseña", "Resetear Contraseña", "Este mensaje se envío a", earthwatcherDb.Name
                            , ". Si no quieres recibir más notificaciones en un futuro podés acceder al Panel de Control del usuario y deshabilitar la opción de recibir notificaciones."
                            , "Greenpeace Argentina. Todos los derechos reservados.", domain);
                        message.IsBodyHtml = true;
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        message.DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.None;
                        messages.Add(message);

                        SendMails.Send(messages);
                    }
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };

                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
                }


            }
            return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest };
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/getbyname", Method = "POST")]
        public HttpResponseMessage<Earthwatcher> GetByName(string name, HttpRequestMessage<string> request)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var earthwatcher = earthwatcherRepository.GetEarthwatcher(name, true);
                if (earthwatcher == null)
                {
                    var response = new HttpResponseMessage<Earthwatcher>(HttpStatusCode.NotFound);
                    return response;
                }

                //Si no tiene una parcela asignada, se le asigna una
                if (earthwatcher.Lands.Count == 0)
                {
                    // assign land
                    var newLand = earthwatcherRepository.AssignLandToEarthwatcher(earthwatcher.Id, string.Empty);
                    Land newLandObj = null;

                    if (newLand == null) //Si la region esta completa le asigno la land del tutor
                    {
                        newLandObj = landRepository.GetTutorLand(earthwatcher.PlayingRegion);
                    }
                    if(newLandObj == null) //Si no esta completa la region
                    {
                        newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                    }
                        //Comunico a los usuarios conectados si es que la nueva land es de un usuario existente
                        NotificateUsers(newLand, earthwatcher.Id);

                        earthwatcher.Lands.Add(newLandObj);
                        return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.OK };                    
                }

                //EarthwatcherLinks.AddLinks(earthwatcher, request);
                return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage<Earthwatcher>(null) { StatusCode = HttpStatusCode.BadRequest };
        }

        //[BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/loginwithapi", Method = "POST")]
        public HttpResponseMessage<Earthwatcher> LoginWithApi(ApiEw ew, HttpRequestMessage<ApiEw> request)
        {
            var localization = new LocalizationService();
            string locale = localization.GetLocale();

            if(!string.IsNullOrEmpty(ew.UserId))
            {
                ApiEw apiEw = earthwatcherRepository.GetApiEw(ew.Api, ew.UserId);

                if (apiEw == null)  //Si no existe en la tabla ApiEwLogin lo inserta y ademas Crea el EARTHWATCHER
                {
                    //INSERTA EL ApiEw en la tabla ApiEwLogin
                    ApiEw newApiEw = earthwatcherRepository.CreateApiEwLogin(ew);

                    if(ew.Api == "Facebook")
                    {
                        Earthwatcher ewIdemFbMail = earthwatcherRepository.GetEarthwatcher(ew.Mail, false);
                       
                        if(ewIdemFbMail != null)  //Si en FB tiene el mismo mail que en guardianes lo relaciona
                        {
                            //Relaciona al ApiEw de facebook con el Earthwatcher del mismo mail
                            earthwatcherRepository.LinkApiAndEarthwatcher(newApiEw.Id, ewIdemFbMail.Id);

                            //Le pasa el UserId y AccessToken para conectarse
                            ewIdemFbMail.UserId = ew.UserId;
                            ewIdemFbMail.AccessToken = ew.AccessToken;

                            //Devuelve el Ew que YA EXISTE relacionado con ese mail de FB
                            return new HttpResponseMessage<Earthwatcher>(ewIdemFbMail) { StatusCode = HttpStatusCode.OK };                        
                        }
                        else
                        {
                            //INSERTA EL EARTHWATCHER
                            var earthwatcher = new Earthwatcher();
                            if(!string.IsNullOrEmpty(ew.Mail))
                            {
                                earthwatcher.Name = ew.Mail;   //Ingreso el mail valido de ese Ew
                            }
                            else
                            {
                                earthwatcher.Name = ew.NickName + ew.UserId.ToString();  //Ingreso un customMail para ese EW
                            }
                            earthwatcher.Password = ew.NickName + earthwatcher.NickName + ew.UserId.ToString(); //Pass = Doble nombre mas userId
                            earthwatcher.NickName = ew.NickName;
                            earthwatcher.Language = (locale != null) ? locale.Substring(0, 5) : "en-CA";
                            earthwatcher.Country = (locale != null) ? locale.Substring(3, 2) : "es-AR";
                            earthwatcher.PlayingRegion = (locale != null) ? Convert.ToInt32(locale.Substring(6, 1)) : 1;
                            earthwatcher.PlayingCountry = (regionRepository.GetById(earthwatcher.PlayingRegion)).CountryCode;
                            earthwatcher.Guid = Guid.NewGuid();

                            Earthwatcher newEarthwatcher = earthwatcherRepository.CreateEarthwatcher(earthwatcher);

                            //Relaciona al ApiEw con el Earthwatcher
                            earthwatcherRepository.LinkApiAndEarthwatcher(newApiEw.Id, newEarthwatcher.Id);

                            //ASIGNO UNA LAND AL NUEVO EW
                            var newLand = earthwatcherRepository.AssignLandToEarthwatcher(earthwatcher.Id, string.Empty);
                            Land newLandObj = null;
                            Console.WriteLine("Me paso la new land " + newLand.Id + " " + newLand.LandId);
                            if (newLand == null) //Si la region esta completa le asigno la land del tutor
                            {
                                newLandObj = landRepository.GetTutorLand(earthwatcher.PlayingRegion);
                            }
                            if (newLandObj == null) //Si no esta completa la region
                            {
                                newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                            }
                            earthwatcher.Lands = new List<Land>();
                            earthwatcher.Lands.Add(newLandObj);
                            //Comunico a los usuarios conectados si es que la nueva land es de un usuario existente
                            NotificateUsers(newLand, earthwatcher.Id);

                            //Le pasa el D:\Dev\Greenpeace\Guardianes\Earthwatchers.UI\Requests\OpengeocoderRequests.csUserId y AccessToken para conectarse
                            newEarthwatcher.UserId = ew.UserId;
                            newEarthwatcher.AccessToken = ew.AccessToken;

                            //Devuelve el ew NUEVO relacionado con esa nueva cuenta de FB
                            return new HttpResponseMessage<Earthwatcher>(newEarthwatcher) { StatusCode = HttpStatusCode.OK };                        
                        }

                    }
                    if (ew.Api != "Facebook")
                    {
                        //INSERTA EL EARTHWATCHER
                        var earthwatcher = new Earthwatcher();
                        earthwatcher.Name = ew.NickName + ew.UserId.ToString();
                        earthwatcher.Password = ew.NickName + earthwatcher.NickName + ew.UserId.ToString(); //Pass = Doble nombre mas userId
                        earthwatcher.NickName = ew.NickName;
                        earthwatcher.Language = (locale != null) ? locale.Substring(0, 5) : "en-CA";
                        earthwatcher.Country = (locale != null) ? locale.Substring(3, 2) : "es-AR";
                        earthwatcher.PlayingRegion = (locale != null) ? Convert.ToInt32(locale.Substring(6, 1)) : 1;
                        earthwatcher.PlayingCountry = (regionRepository.GetById(earthwatcher.PlayingRegion)).CountryCode;
                        earthwatcher.Guid = Guid.NewGuid();

                        Earthwatcher newEarthwatcher = earthwatcherRepository.CreateEarthwatcher(earthwatcher);

                        //Relaciona al ApiEw con el Earthwatcher
                        earthwatcherRepository.LinkApiAndEarthwatcher(newApiEw.Id, newEarthwatcher.Id);

                        var newLand = earthwatcherRepository.AssignLandToEarthwatcher(earthwatcher.Id, string.Empty);
                        Land newLandObj = null;
                        Console.WriteLine("Me paso la new land " + newLand.Id + " " + newLand.LandId);
                        if (newLand == null) //Si la region esta completa le asigno la land del tutor
                        {
                            newLandObj = landRepository.GetTutorLand(earthwatcher.PlayingRegion);
                        }
                        if (newLandObj == null) //Si no esta completa la region
                        {
                            newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                        }
                        earthwatcher.Lands = new List<Land>();
                        earthwatcher.Lands.Add(newLandObj);
                        //Comunico a los usuarios conectados si es que la nueva land es de un usuario existente
                        NotificateUsers(newLand, earthwatcher.Id);

                        //Le pasa el UserId y AccessToken para conectarse
                        newEarthwatcher.UserId = ew.UserId;
                        newEarthwatcher.AccessToken = ew.AccessToken;

                        return new HttpResponseMessage<Earthwatcher>(newEarthwatcher) { StatusCode = HttpStatusCode.OK };
                    }
                }
                else
                {
                    //Si ya existe en mi tabla ApiLogin Le updateo el accessToken
                    if(ew.AccessToken != apiEw.AccessToken)
                    {
                        earthwatcherRepository.UpdateAccessToken(apiEw.Id, ew.AccessToken);
                    }
                    //Lo busco por el Id del EW relacionado
                    Earthwatcher earthwatcher = earthwatcherRepository.GetEarthwatcher(apiEw.EarthwatcherId);
                    
                    //Le Agrega el UserId al Earthwatcher y lo devuelve
                    earthwatcher.UserId = apiEw.UserId;
                    earthwatcher.AccessToken = ew.AccessToken;
                    return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.OK };
                }
            }
            return new HttpResponseMessage<Earthwatcher>(null) { StatusCode = HttpStatusCode.BadRequest };
        }

        [WebGet(UriTemplate = "/getlogged")]
        public HttpResponseMessage<Earthwatcher> GetLogged(HttpRequestMessage request)
        {
            Earthwatcher logged = null;
            try
            {
                if (Session.HasLoggedUser())
                {
                    logged = earthwatcherRepository.GetEarthwatcher(Session.GetCookieInfo().EarthwatcherName, true);
                    if(logged.Lands.Count == 0) //Si te estas logueando sin land y no hay land que agregarte, te asigno la del tutor
                    {
                        logged.Lands.Add(landRepository.GetTutorLand(logged.PlayingRegion));
                    }
                    return new HttpResponseMessage<Earthwatcher>(logged) { StatusCode = HttpStatusCode.OK };
                }
                return new HttpResponseMessage<Earthwatcher>(logged) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<Earthwatcher>(null) { StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        [WebGet(UriTemplate = "/islogged")]
        public HttpResponseMessage<bool> IsLogged(HttpRequestMessage request)
        {
            try
            {
                bool isLogged = Session.HasLoggedUser();
                return new HttpResponseMessage<bool>(isLogged) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<bool>(false) { StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        [WebGet(UriTemplate = "/exists={name}")]
        public HttpResponseMessage<bool> Exists(string name, HttpRequestMessage request)
        {
            var decodedMail = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(name));
            bool exists = earthwatcherRepository.EarthwatcherExists(decodedMail);

            return new HttpResponseMessage<bool>(exists) { StatusCode = HttpStatusCode.OK };
        }

        [WebGet(UriTemplate = "/isAdmin={name}")]
        public HttpResponseMessage<bool> IsAdmin(string name, HttpRequestMessage request)
        {
            var decodedMail = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(name));
            var ew = earthwatcherRepository.GetEarthwatcher(decodedMail, false);

            if (ew != null && ew.Role == Role.Admin)
            {
                return new HttpResponseMessage<bool>(true) { StatusCode = HttpStatusCode.OK };
            }
            else
            {
                return new HttpResponseMessage<bool>(false) { StatusCode = HttpStatusCode.OK };
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebGet(UriTemplate = "")]
        public HttpResponseMessage<List<Earthwatcher>> GetAll(HttpRequestMessage request)
        {
            var earthwatchers = earthwatcherRepository.GetAllEarthwatchers();
            return new HttpResponseMessage<List<Earthwatcher>>(earthwatchers) { StatusCode = HttpStatusCode.OK };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "{id}", Method = "DELETE")]
        public HttpResponseMessage Delete(int id, HttpRequestMessage<Earthwatcher> request)
        {
            var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(id);

            if (earthwatcherDb != null)
            {
                earthwatcherRepository.DeleteEarthwatcher(earthwatcherDb.Id);

                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/updateearthwatcher", Method = "POST")]
        public HttpResponseMessage UpdateEarthWatcher(Earthwatcher ew, HttpRequestMessage<Earthwatcher> request)
        {
            if (ew.MailChanged == true && ew.Name != null)
            {
                bool exists = earthwatcherRepository.EarthwatcherExists(ew.Name);
                if (exists == true)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.MultipleChoices };
                }
            }

            if (ew != null)
            {
                earthwatcherRepository.UpdateEarthwatcher(ew.Id, ew);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/updateearthwatcherrole", Method = "POST")]
        public HttpResponseMessage UpdateEarthWatcherRole(Earthwatcher ew, HttpRequestMessage<Earthwatcher> request)
        {
            var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(ew.Name, false);
            if (earthwatcherDb != null)
            {
                earthwatcherDb.Role = ew.Role;
                earthwatcherRepository.UpdateEarthwatcher(earthwatcherDb.Id, earthwatcherDb);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "{id}", Method = "PUT")]
        public HttpResponseMessage UpdateEarthwatcherAdmin(int id, Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(id);
            if (earthwatcherDb != null)
            {
                earthwatcherRepository.UpdateEarthwatcher(earthwatcher.Id, earthwatcher);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        //[BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "", Method = "POST")]
        public HttpResponseMessage<Earthwatcher> Post(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {

            // quick and easy model validation  
            if (String.IsNullOrEmpty(earthwatcher.Name) |
                String.IsNullOrEmpty(earthwatcher.Password) |
                String.IsNullOrEmpty(earthwatcher.Country))
            {
                return new HttpResponseMessage<Earthwatcher>(HttpStatusCode.BadRequest) { ReasonPhrase = "Model is not valid" };
            }

            var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(earthwatcher.Name, false);
            if (earthwatcherDb == null)
            {
                //set localization preferences
                var locSrv = new LocalizationService();
                string locale = locSrv.GetLocale();
                if (locSrv.IsLocaleSetted())
                {
                    earthwatcher.Country = (locale != null) ? locale.Substring(3, 2) : "es-AR";
                    earthwatcher.Language = (locale != null) ? locale.Substring(0, 5) : "en-CA";
                    earthwatcher.PlayingRegion = (locale != null) ? Convert.ToInt32(locale.Substring(6, 1)) : 1;
                    earthwatcher.PlayingCountry = (regionRepository.GetById(earthwatcher.PlayingRegion)).CountryCode;
                    
                }

                earthwatcher.Guid = Guid.NewGuid();
                earthwatcherRepository.CreateEarthwatcher(earthwatcher);
                // assign land

                var newLand = earthwatcherRepository.AssignLandToEarthwatcher(earthwatcher.Id, string.Empty);
                Land newLandObj = null;
                if (newLand == null) //Si la region esta completa le asigno la land del tutor
                {
                    newLandObj = landRepository.GetTutorLand(earthwatcher.PlayingRegion);
                }
                if (newLandObj == null) //Si no esta completa la region
                {
                    newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                }
                earthwatcher.Lands = new List<Land>();
                earthwatcher.Lands.Add(newLandObj);
                //Comunico a los usuarios conectados si es que la nueva land es de un usuario existente
                NotificateUsers(newLand, earthwatcher.Id);

                //Mando el mail de bienvenida
                try
                {
                    if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["smtp.enabled"]))
                    {
                        List<System.Net.Mail.MailMessage> messages = new List<System.Net.Mail.MailMessage>();

                        System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(earthwatcher.Name);
                        System.Net.Mail.MailAddress addressFrom = new System.Net.Mail.MailAddress(System.Configuration.ConfigurationManager.AppSettings["smtp.user"], "Guardianes - Greenpeace");
                        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                        message.From = addressFrom;
                        message.To.Add(address);
                        message.Subject = Labels.Labels.MailBienvenidaAsunto;

                        string domain = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);

                        string htmlTemplate = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mail.html"));
                        message.Body = string.Format(htmlTemplate, Labels.Labels.MailBienvenidaTitulo
                            , Labels.Labels.MailBienvenidaCuerpo
                            , string.Format("{0}/index.html", domain), Labels.Labels.MailBienvenidaBotonTooltip, Labels.Labels.MailBienvenidaBoton, Labels.Labels.LandVerifications6, earthwatcher.Name
                            , Labels.Labels.LandVerifications7
                            , Labels.Labels.LandVerifications8, domain);
                        message.IsBodyHtml = true;
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        message.DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.None;
                        messages.Add(message);
                        SendMails.Send(messages);
                    }

                    return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.Created };
                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage<Earthwatcher>(earthwatcher) { StatusCode = HttpStatusCode.InternalServerError, ReasonPhrase = ex.Message };
                }
            }
            throw new HttpResponseException("error: user already exists");
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/setaspower", Method = "POST")]
        public HttpResponseMessage SetEarthwatcherAsPowerUser(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            var earthwatcherDb = earthwatcherRepository.GetEarthwatcher(earthwatcher.Name, false);
            if (earthwatcherDb != null)
            {
                earthwatcherRepository.SetEarthwatcherAsPowerUser(earthwatcherDb.Id, earthwatcherDb);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        private void NotificateUsers(LandMini newLand, int earthwatcherId)
        {
            if (newLand != null && newLand.IsUsed)
            {
                try
                {
                    //Comunico a los usuarios conectados si es que la nueva land es de un usuario existente
                    var context = GlobalHost.ConnectionManager.GetHubContext<Hubs>();
                    context.Clients.All.LandChanged(newLand.GeohexKey, earthwatcherId);
                    logger.Info("Paso bien por el NotificateUsers");
                }
                catch (Exception ex)
                {
                    logger.Error("Ocurrio una excepcion en el Notificate Users. Message: {0},  StackTrace:{1}", ex.Message, ex.StackTrace);
                }
            }
        }

        [WebInvoke(UriTemplate = "/logSlInstall", Method = "POST")]
        public void logSlInstall()
        {
            var connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["EarthwatchersConnection"].ConnectionString;
            IScoreRepository repo = new ScoreRepository(connectionstring);
            Score s = new Score(17, ActionPoints.Action.Log.ToString(), ActionPoints.Points(ActionPoints.Action.Log), 0, 100, null, null, "SL_Install");
            repo.PostScore(s);
        }

        [BasicHttpAuthorization(Role.Earthwatcher)]
        [WebInvoke(UriTemplate = "/reassignland", Method = "POST")]
        public HttpResponseMessage<Land> ReassignLand(Earthwatcher e, HttpRequestMessage<Earthwatcher> request)
        {
            if (e.Id!= 0)
            {
                try
                {
                    var newLand = landRepository.ReassignLand(e.Id);

                    if (newLand != null)
                    {
                        Land newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                        if (newLandObj != null)
                        {
                            newLandObj.EarthwatcherId = e.Id;
                            NotificateUsers(newLand, e.Id);
                            return new HttpResponseMessage<Land>(newLandObj) { StatusCode = HttpStatusCode.Created };
                        }
                    }
                    else
                    {
                        Land newLandObj = landRepository.GetTutorLand(e.PlayingRegion);
                        return new HttpResponseMessage<Land>(newLandObj) { StatusCode = HttpStatusCode.Created, ReasonPhrase = Labels.Labels.NoMoreLands };
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Ocurrio una excepcion en el ReasignLand. Message: {0},  StackTrace:{1}", ex.Message, ex.StackTrace);
                    return new HttpResponseMessage<Land>(null) { StatusCode = HttpStatusCode.Conflict, ReasonPhrase = ex.Message };
                }
            }

            return new HttpResponseMessage<Land>(null) { StatusCode = HttpStatusCode.BadRequest };

        }

        [WebInvoke(UriTemplate = "/changeplayingregion", Method = "POST")]
        public HttpResponseMessage ChangePlayingRegion(Earthwatcher earthwatcher, HttpRequestMessage<Earthwatcher> request)
        {
            try
            {
                earthwatcherRepository.ChangePlayingRegion(earthwatcher);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };
            }
        }

        //[WebInvoke(UriTemplate = "registerPetition", Method = "POST")]
        //public HttpResponseMessage RegisterPetition(PetitionsSigned petition, HttpRequestMessage<PetitionsSigned> request)
        //{
        //    if (petition != null)
        //    {
        //        try
        //        {
        //            earthwatcherRepository.SavePetitionSigned(petition);
        //            return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
        //        }
        //        catch (Exception ex)
        //        {
        //            return new HttpResponseMessage<PetitionsSigned>(petition) { StatusCode = HttpStatusCode.BadRequest };
        //        }
        //    }
        //    else
        //    {
        //        return new HttpResponseMessage() { StatusCode = HttpStatusCode.NoContent };
        //    }
        //}

        //[WebGet(UriTemplate = "hasSigned")]
        //public PetitionsSigned HasSigned(PetitionsSigned petition, HttpRequestMessage<PetitionsSigned> request)
        //{
        //    return earthwatcherRepository.HasSigned(petition);
        //}
    }
}





