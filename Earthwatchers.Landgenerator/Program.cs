using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Earthwatchers.Data;
using Earthwatchers.Models;
using Earthwatchers.Models.KmlModels;

namespace Earthwatchers.Landgenerator
{

    //var currentLon = topLeft.X;
    //var currentLat = topLeft.Y;

    class Program
    {
        private static ComputableLayer _forestlaw = null;
        private static ComputableLayer _basecamps = null;
        private static string _connectionString = "Server=.;Database=EarthwatchersRegions;Trusted_Connection=yes;";

        static void Main(string[] args)
        {
            try
            {
                var st = DateTime.Now;
                var fincasName = "";
                var forestLawName = "";

                ////PRUEBA TAMAÑOS
                //var topLeft = new PointD(-62.087462, -23.842284); //Long/Lat
                //var bottomRight = new PointD(-61.838865, -24.080809); //Long/Lat
                //int regionId = 5; 

                //var topLeft = new PointD(-65.742, -21.988);     //UBICACIONES DEL CUADRADO GRANDE SALTA
                //var bottomRight = new PointD(-62.057556, -26.558016); 

                ////Salta - Argentina
                //var topLeft = new PointD(-63.655230, -22.055296); //Long/Lat
                //var bottomRight = new PointD(-63.098436, -22.508956); //Long/Lat
                //fincasName = "FincasLayer1"; 
                //forestLawName = "OTBN";
                //int regionId = 1;

                ////Chaco - Argentina
                var topLeft = new PointD(-63.553825, -23.976782); //Long/Lat
                var bottomRight = new PointD(-60.235678, -26.294722); //Long/Lat
                fincasName = "FincasLayer2"; //Nombre que debe tener el kml que contiene las fincas de ese pais
                forestLawName = "OTBN2"; //Nombre de la ley de bosques del pais
                int regionId = 2; 


                //China -  Yaan
                //var topLeft = new PointD(101.488403, 30.889660); //Long/Lat  COMPLETO
                //var bottomRight = new PointD(103.621349, 28.774082); //Long/Lat COMPLETO
                //var topLeft = new PointD(102.552107, 30.673024); //Long/Lat
                //var bottomRight = new PointD(102.901923, 29.774818); //Long/Lat
                //fincasName = "FincasLayer3"; //Nombre que debe tener el kml que contiene las fincas de ese pais
                //forestLawName = "OTBN3"; //Nombre de la ley de bosques del pais
                //int regionId = 3;

                ////China - zhejian
                ////var topLeft = new PointD(117.998174, 31.243206); //Long/Lat  COMPLETO
                ////var bottomRight = new PointD(121.583889, 26.797424); //Long/Lat  COMPLETO
                //var topLeft = new PointD(119.101848, 29.848194); //Long/Lat
                //var bottomRight = new PointD(119.412014, 29.732894); //Long/Lat
                //fincasName = "FincasLayer4"; //Nombre que debe tener el kml que contiene las fincas de ese pais
                //forestLawName = "OTBN4"; //Nombre de la ley de bosques del pais
                //int regionId = 4; 
                
                ////Canada 
                //var topLeft = new PointD(-111.236353, 57.299930); //Long/Lat
                //var bottomRight = new PointD(-110.900724, 57.107068); //Long/Lat
                //fincasName = "FincasLayer5"; //Nombre que debe tener el kml que contiene las fincas de ese pais
                //forestLawName = "OTBN5"; //Nombre de la ley de bosques del pais
                //int regionId = 5; 

                var repo = new LayerRepository(_connectionString);

                Console.WriteLine("Cargando Fincas en memoria...");
                Layer basecampsLayer = null;// repo.GetLayerByName(fincasName);

                Console.WriteLine("Cargando Ley de bosques en memoria...");
                Layer forestLawLayer = null;//repo.GetLayerByName(forestLawName);  //

                if (basecampsLayer != null)
                {
                    var bclist = basecampsLayer.Zones.Select(z => new ComputableZone(z)).ToList();
                    _basecamps = new ComputableLayer(bclist);
                }

                if (forestLawLayer != null)
                {
                    var lawlist = forestLawLayer.Zones.Select(z => new ComputableZone(z)).ToList();
                    _forestlaw = new ComputableLayer(lawlist);
                }


                Console.WriteLine("Intersectando Lands con Fincas y ley de bosques...");
                var newLands = GenerateLands(topLeft, bottomRight, 7, regionId);


                // write land to database...
                //var landRepository = new LandRepository("Data Source=dfrvf2t76i.database.windows.net;Initial Catalog=EarthwatchersRegions;Persist Security Info=True;User ID=Editor;Asynchronous Processing=True;Password=8p3k00l!!!!");
                var conbase = newLands.Where(x => x.BasecampId != null);
                Console.WriteLine("Guardando las " + newLands.Count + " lands generadas...");
                var landRepository = new LandRepository(_connectionString);
                landRepository.CreateLand(newLands);

                //Se usan aun, pero habria que mejorar el metodo que calcula la amenaza y el basecamp
                #region Obsoleto
                //Console.WriteLine("Cargando Threat levels...");
                //landRepository.LoadThreatLevel();
                //Console.WriteLine("Asignando BasecampId a lands..."); 
                //landRepository.LoadLandBasecamp();
                #endregion

                Console.WriteLine("Carga Completa");
                var horaFin = DateTime.Now;
                Console.WriteLine("Hora Fin CreateLand: " + horaFin); 

                Console.WriteLine("Klaar"); //Listo Holandés
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static List<Land> GenerateLands(PointD topLeft, PointD bottomRight, int level, int regionId)
        {
            var horaInicio = DateTime.Now;
            Console.WriteLine("Hora de inicio GenerateLands: " + horaInicio); 
            var newLand = new List<Land>();

            //Tamaño Mediano (Salta)
            //const double increase = 0.0075;
            //level = 7;

            //Tamaño Grande (Chaco 10K Total - Las que no sirben)
            const double increase = 0.0175;
            level = 6;

            //Tamaño Chico
            //const double increase = 0.00175;
            //level = 8;

            int assignables = 0;
            int unassignables = 0;

            for (var i = topLeft.X; i <= bottomRight.X; i += increase)
            {
                for (var j = topLeft.Y; j >= bottomRight.Y; j -= increase)
                {
                    var hexKey = GeoHex.Encode(i, j, level);
                    var land = new Land();
                    land.Longitude = i;
                    land.Latitude = j;
                    land.GeohexKey = hexKey;
                    land.RegionId = regionId;
                    land.BasecampId = 999;       //FORMA FACIL DE IDENTIFICAR A LAS NUEVAS PARCELAS CREADAS PARA TEST
                    if ((!newLand.Any(l => l.GeohexKey == hexKey)))
                    {
                        if (_forestlaw != null)
                        {
                            if (ComputeLandThreat(land))
                            {
                                //ComputeBasecampIntersection(land);
                                newLand.Add(land);
                                assignables++;
                            }
                            else unassignables++;
                        }
                        else
                        {
                            unassignables++;
                            newLand.Add(land);
                        }
                    }
                }
            }
            var horaFin = DateTime.Now;
            Console.WriteLine("Hora Fin GenerateLands: " + horaFin);
            Console.WriteLine("ASIGNABLES: " + assignables);
            Console.WriteLine("NO ASIGNABLES: " + unassignables); 

return newLand;
        }

        /// <summary>
        /// Realiza todos los calculos para evaluar el nivel de amenaza de una parcela. Y asigna el nivel de amenaza a la zona.
        /// </summary>
        /// <param name="land"></param>
        public static bool ComputeLandThreat(Land land)
        {
            bool assignable = false;
            var zone = _forestlaw.GetConainerZone(land.Latitude, land.Longitude);
            if (zone != null)
            {
                if (zone.Name == "Zona Roja")
                {
                    land.LandThreat = LandThreat.High;
                    assignable = true;

                }
                else if (zone.Name == "Zona Amarilla")
                {
                    land.LandThreat = LandThreat.Intermediate;
                    assignable = true;
                }
                else
                {
                    land.LandThreat = LandThreat.Zero;
                }
            }
                return assignable;
        }

        /// <summary>
        /// Realiza los calculos de interseccion de la land contra las fincas existentes, obteniendo y asignando el basecampId. 
        /// </summary>
        /// <param name="land"></param>
        public static void ComputeBasecampIntersection(Land land) 
        {
            int bcId = 0;
            var zone = _basecamps.GetConainerZone(land.Latitude, land.Longitude);
            if (zone != null)
            {
                int.TryParse(zone.Param1, out bcId);
                if(bcId != 0)
                {
                    land.BasecampId = bcId;
                }
            }
        }

        public class ComputableZone
        {
            private Models.KmlModels.Zone _zone;
            private List<PolygonMath> _polygonsMath;

            public ComputableZone(Models.KmlModels.Zone zone)
            {
                _zone = zone;
                _polygonsMath = new List<PolygonMath>();

                foreach (var p in _zone.Polygons)
                {
                    var pm = new PolygonMath();
                    foreach (var l in p.Locations)
                    {
                        if (l.Latitude.HasValue && l.Longitude.HasValue)
                            pm.Points.Add(new Vector(l.Latitude.Value, l.Longitude.Value));
                    }
                    _polygonsMath.Add(pm);
                }
            }

            public bool Contains(double lat, double lon)
            {
                var contains = false;

                var landCenter = new PolygonMath();
                landCenter.Points.Add(new Vector(lat, lon));

                var vel = new Vector(0, 0);
                foreach (var p in _polygonsMath)
                {
                    if (p.PolygonCollision(landCenter, vel).Intersect)
                    {
                        contains = true;
                        break;
                    }
                }

                return contains;
            }

            public Models.KmlModels.Zone GetZone()
            {
                return _zone;
            }
        }

        public class ComputableLayer : List<ComputableZone>
        {
            public ComputableLayer(List<ComputableZone> initialdata)
            {
                this.AddRange(initialdata);
            }

            public Models.KmlModels.Zone GetConainerZone(double lat, double lon)
            {
                Models.KmlModels.Zone zone = null;
                foreach (var f in this)
                {
                    if (f.Contains(lat, lon))
                    {
                        zone = f.GetZone();
                        break;
                    }
                }
                return zone;
            }
        }


        //public class PointD
        //{
        //    public PointD(double x, double y)
        //    {
        //        this.X = x;
        //        this.Y = y;
        //    }

        //    public double X { get; set; }
        //    public double Y { get; set; }

        //}

    }
}