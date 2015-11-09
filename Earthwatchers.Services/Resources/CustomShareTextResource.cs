using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using Earthwatchers.Data;
using Earthwatchers.Models;
using Earthwatchers.Services.Security;
namespace Earthwatchers.Services.Resources
{
    [ServiceContract]
    public class CustomShareTextResource
    {
        private readonly ICustomShareTextRepository customShareTextRepository;

        public CustomShareTextResource(ICustomShareTextRepository _customShareTextRepository)
        {
            this.customShareTextRepository = _customShareTextRepository;
        }

        [WebGet(UriTemplate = "/getall")]
        public HttpResponseMessage<List<CustomShareText>> GetAllShareTexts(HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<List<CustomShareText>>(customShareTextRepository.GetAll()) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<CustomShareText>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [WebGet(UriTemplate = "/getbyid/id={id}")]
        public HttpResponseMessage<CustomShareText> GetById(int id, HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<CustomShareText>(customShareTextRepository.GetById(id)) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<CustomShareText>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [WebInvoke(UriTemplate = "/getbyregionidandlanguage", Method = "POST")]
        public HttpResponseMessage<CustomShareText> GetByRegionIdAndLanguage(CustomShareText shareText, HttpRequestMessage<CustomShareText> request)
        {
            try
            {
                return new HttpResponseMessage<CustomShareText>(customShareTextRepository.GetByRegionIdAndLanguage(shareText.RegionId, shareText.Language)) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<CustomShareText>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "", Method = "POST")]
        public HttpResponseMessage<CustomShareText> Post(CustomShareText shareText, HttpRequestMessage<CustomShareText> request)
        {
            if (shareText != null)
            {
                var shareTextDB = customShareTextRepository.Insert(shareText);

                var response = new HttpResponseMessage<CustomShareText>(shareTextDB) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            return new HttpResponseMessage<CustomShareText>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/edit", Method = "POST")]
        public HttpResponseMessage<CustomShareText> Edit(CustomShareText shareText, HttpRequestMessage<CustomShareText> request)
        {
            if (shareText != null)
            {
                var shareTextDb = customShareTextRepository.Edit(shareText);

                return new HttpResponseMessage<CustomShareText>(shareTextDb) { StatusCode = HttpStatusCode.Created };
            }
            return new HttpResponseMessage<CustomShareText>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

        [WebGet(UriTemplate = "/delete/id={id}")]
        public HttpResponseMessage Delete(int id, HttpRequestMessage request)
        {
            try
            {
                customShareTextRepository.Delete(id);
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest };
            }
        }

    }
}