using Earthwatchers.Models;
using System.Collections.Generic;

namespace Earthwatchers.Data
{
    public interface IRegionRepository
    {
        List<Region> GetAll();
        List<Region> GetByCountryCode(string countryCode);
        Region GetById(int regionId);
        Region GetByName(string Name);
        Region Insert(Region region);
        Region Edit(Region region);

    }
}
