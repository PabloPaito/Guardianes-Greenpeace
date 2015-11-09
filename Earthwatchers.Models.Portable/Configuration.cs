using System;
using System.Net;

namespace Earthwatchers.Models
{
    public static class Configuration
    {
        //TODO: mejorar este hardcodeo del usuario greenpeace
        public static readonly int GreenpeaceId = 17;
        public static readonly string GreenpeaceName = "greenpeace";
        public static readonly string ImagesPath = "http://guardianes.greenpeace.org.ar/SatelliteImages";
        public static bool HideHexagons = true; //ocultar los hexagonos a un zoom alto, toma el valor en el web.config

        //SALTA  GeoHexKey = NY8582044 -  Id = 149826
        //CHACO  GeoHexKey = NY845311 -  Id = 561299
        }
}
