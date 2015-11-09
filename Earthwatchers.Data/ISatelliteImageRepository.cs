using System.Collections.Generic;
using Earthwatchers.Models;

namespace Earthwatchers.Data
{
    public interface ISatelliteImageRepository
    {
        void Delete(int id);
        SatelliteImage Get(int id);
        SatelliteImage GetById(int id);
        SatelliteImage Insert(SatelliteImage satelliteImage);
        SatelliteImage Update(SatelliteImage satelliteImage);
        List<SatelliteImage> GetAll(int regionId);
        List<SatelliteImage> Intersects(string wkt);
        List<string> GetImagesUrlPath();
        void UpdateList(List<SatelliteImage> images);
    }
}
