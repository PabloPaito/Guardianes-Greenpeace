using Earthwatchers.Models;
using System.Collections.Generic;

namespace Earthwatchers.Data
{
    public interface ICountryRepository
    {
        List<Country> GetAll();
        Country GetByCode(string CountryCode);
    }
}
