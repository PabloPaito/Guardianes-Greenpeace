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
    public class CountryResource
    {
        private readonly ICountryRepository countryRepository;

        public CountryResource(ICountryRepository _countryRepository)
        {
            this.countryRepository = _countryRepository;
        }

        [WebGet(UriTemplate = "/getall")]
        public HttpResponseMessage<List<Country>> GetAllCountries(HttpRequestMessage request)
        {
            try
            {
                return new HttpResponseMessage<List<Country>>(countryRepository.GetAll()) { StatusCode = HttpStatusCode.OK };
            }
            catch
            {
                return new HttpResponseMessage<List<Country>>(null) { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        //[WebInvoke(UriTemplate = "/getbycode", Method = "POST")]
        //public HttpResponseMessage<Country> GetAllContests(Country country, HttpRequestMessage<Country> request)
        //{
        //    try
        //    {
        //        return new HttpResponseMessage<Country>(countryRepository.GetByCode(country.Code)) { StatusCode = HttpStatusCode.OK };
        //    }
        //    catch
        //    {
        //        return new HttpResponseMessage<Country>(null) { StatusCode = HttpStatusCode.BadRequest };
        //    }
        //}

    }
}