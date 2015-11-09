using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using Earthwatchers.Data;
using Earthwatchers.Models;
using Earthwatchers.Services.Security;
using System.Data.SqlTypes;
using Earthwatchers.Services.admin;
using System.Linq;
using System.Configuration;
using System;
using NLog;

namespace Earthwatchers.Services.Resources
{
    [ServiceContract]
    public class SatelliteImageResource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ISatelliteImageRepository satelliteImageRepository;

        public SatelliteImageResource(ISatelliteImageRepository satelliteImageRepository)
        {
            this.satelliteImageRepository = satelliteImageRepository;
        }

        [WebGet(UriTemplate = "{id}")]
        public HttpResponseMessage<SatelliteImage> Get(int id, HttpRequestMessage request)
        {
            var satelliteImage = satelliteImageRepository.Get(id);
            if (satelliteImage == null)
            {
                return new HttpResponseMessage<SatelliteImage>(HttpStatusCode.NotFound);
            }
            return new HttpResponseMessage<SatelliteImage>(satelliteImage) { StatusCode = HttpStatusCode.OK };
        }

        [WebInvoke(UriTemplate = "/getById", Method = "POST")]
        public HttpResponseMessage<SatelliteImage> GetById(int id, HttpRequestMessage<int> request)
        {
            var satelliteImage = satelliteImageRepository.GetById(id);
            return new HttpResponseMessage<SatelliteImage>(satelliteImage) { StatusCode = HttpStatusCode.OK };
        }

        [WebGet(UriTemplate = "/region={regionId}")]
        public HttpResponseMessage<List<SatelliteImage>> GetAllSatellites(int regionId, HttpRequestMessage request)
        {
            var satelliteImageCollection = satelliteImageRepository.GetAll(regionId);
            return new HttpResponseMessage<List<SatelliteImage>>(satelliteImageCollection) { StatusCode = HttpStatusCode.OK };
        }

        [WebGet(UriTemplate = "/all")]
        public HttpResponseMessage<List<SatelliteImage>> GetAll()
        {
            var satelliteImageCollection = satelliteImageRepository.GetAll(0);
            return new HttpResponseMessage<List<SatelliteImage>>(satelliteImageCollection) { StatusCode = HttpStatusCode.OK };
        }

        [WebInvoke(UriTemplate = "/intersect", Method = "POST")]
        public HttpResponseMessage<List<SatelliteImage>> GetSatelliteImageByWkt(string wkt, HttpRequestMessage<string> request)
        {
            if (!string.IsNullOrEmpty(wkt))
            {
                var satelliteImageCollection = satelliteImageRepository.Intersects(wkt);
                return new HttpResponseMessage<List<SatelliteImage>>(satelliteImageCollection) { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage<List<SatelliteImage>>(null) { StatusCode = HttpStatusCode.BadRequest };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "", Method = "POST")]
        public HttpResponseMessage<SatelliteImage> Post(SatelliteImage satelliteImage, HttpRequestMessage<SatelliteImage> request)
        {
            if (satelliteImage != null)
            {
                var satelliteImageDb = satelliteImageRepository.Insert(satelliteImage);

                var response = new HttpResponseMessage<SatelliteImage>(satelliteImageDb) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            return new HttpResponseMessage<SatelliteImage>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/update", Method = "POST")]
        public HttpResponseMessage<SatelliteImage> Update(SatelliteImage satelliteImage, HttpRequestMessage<SatelliteImage> request)
        {
            if (satelliteImage != null)
            {
                var satelliteImageDb = satelliteImageRepository.Update(satelliteImage);

                var response = new HttpResponseMessage<SatelliteImage>(satelliteImageDb) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            return new HttpResponseMessage<SatelliteImage>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/del", Method = "POST")]
        public HttpResponseMessage Delete(int id, HttpRequestMessage<int> request)
        {
            var satelliteImage = satelliteImageRepository.Get(id);

            if (satelliteImage != null)
            {
                satelliteImageRepository.Delete(satelliteImage.Id);

                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/listImages", Method = "POST")]
        public HttpResponseMessage<List<string>> ListImages()
        {
            var databaseImagesNames = new List<string>();
            var allImages = satelliteImageRepository.GetImagesUrlPath();
            
            foreach(var i in allImages)
            {
                var urlParts = i.Split('/');
                var last = urlParts.Last();
                databaseImagesNames.Add(last);
            }
            List<string> serverImagesNames = XmlParserSatelliteImages.ListImages();

            List<string> imagesToHide = serverImagesNames.Intersect(databaseImagesNames).ToList();
            List<string> imagesToShow = serverImagesNames.Except(imagesToHide).ToList();
            return new HttpResponseMessage<List<string>>(imagesToShow) { StatusCode = HttpStatusCode.OK };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/unzipImages", Method = "POST")]
        public HttpResponseMessage UnzipImages(string filename)
        {
            try
            {
                ImagesUnzipper.Unzip(ConfigurationManager.AppSettings.Get("ImagesZip.path"), ConfigurationManager.AppSettings.Get("ImagesUnZipped.path"), filename);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent(ex.Message.ToString())};
            }

        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/listUnzippedImages", Method = "POST")]
        public HttpResponseMessage<List<string>> ListUnzippedImages()
        {
            //GetImageNames
            List<string> serverImagesToUnzipNames = XmlParserSatelliteImages.ListImagesToUnzip();

            return new HttpResponseMessage<List<string>>(serverImagesToUnzipNames) { StatusCode = HttpStatusCode.OK };

        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/importandaddimage", Method = "POST")]
        public HttpResponseMessage<SatelliteImage> ImportAndAddImage(string imageName, HttpRequestMessage<string> request)
        {
            SatelliteImage parsedImage;
            HttpResponseMessage<SatelliteImage> response;
            try
            {
                //Parse Image
                parsedImage = XmlParserSatelliteImages.ReadXmlFileAndParseImage(imageName);
                
                logger.Info("listo para guardarlo");
                logger.Info(parsedImage.Name + " " + parsedImage.RegionId + " " + parsedImage.UrlTileCache + " " + parsedImage.Wkt);

                //Add Image
                var satelliteImageDb = satelliteImageRepository.Insert(parsedImage);
                logger.Info(satelliteImageDb.Name + " " + satelliteImageDb.RegionId + " " + satelliteImageDb.UrlTileCache + " " + satelliteImageDb.Wkt);

                response = new HttpResponseMessage<SatelliteImage>(satelliteImageDb) { StatusCode = HttpStatusCode.Created };
            }
            catch (Exception ex)
            {
                logger.Error("Exception: " + ex);
                response = new HttpResponseMessage<SatelliteImage>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
            return response;
        }

    }
}
