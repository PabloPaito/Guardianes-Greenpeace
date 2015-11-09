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
    public class RegionResource
    {
        private readonly IRegionRepository regionRepository;

        public RegionResource(IRegionRepository _regionRepository)
        {
            this.regionRepository = _regionRepository;
        }

        [WebGet(UriTemplate = "/getall")]
        public HttpResponseMessage<List<Region>> GetAllRegions(HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<List<Region>>(regionRepository.GetAll()) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<Region>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [WebGet(UriTemplate = "/getbyid/id={regionid}")]
        public HttpResponseMessage<Region> GetById(int regionid, HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<Region>(regionRepository.GetById(regionid)) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<Region>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [WebGet(UriTemplate = "/getbycountrycode/countrycode={countrycode}")]   //TODO: REVISAR Y PROBAR CUANDO SE IMPLEMENTE RELLENAR LOS COMBOS
        public HttpResponseMessage<List<Region>> GetRegionsByCountryCode(string countrycode, HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<List<Region>>(regionRepository.GetByCountryCode(countrycode)) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<Region>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "", Method = "POST")]
        public HttpResponseMessage<Region> Post(Region region, HttpRequestMessage<Region> request)
        {
            if (region != null)
            {
                var regionDB = regionRepository.Insert(region);

                var response = new HttpResponseMessage<Region>(regionDB) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            return new HttpResponseMessage<Region>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

        [BasicHttpAuthorization(Role.Admin)]
        [WebInvoke(UriTemplate = "/edit", Method = "POST")]
        public HttpResponseMessage<Region> Edit(Region region, HttpRequestMessage<Region> request)
        {
            if (region != null)
            {
                var basecampDb = regionRepository.Edit(region);

                var response = new HttpResponseMessage<Region>(basecampDb) { StatusCode = HttpStatusCode.Created };
                return response;
            }
            return new HttpResponseMessage<Region>(null) { StatusCode = HttpStatusCode.NotFound, };
        }

    }
}