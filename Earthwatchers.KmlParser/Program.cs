using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Earthwatchers.Models.KmlModels;
using Earthwatchers.Data;
using System.Globalization;
using System.Threading;
using System.Configuration;

namespace Earthwatcher.KmlParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;

            //var parser = new KmlParserV2();
            var parser = new KmlParser();
            var layer = parser.ReadKmlFile();
            List<string> errors = parser.ListErrors(layer);

            if (!errors.Any())
            {
                Console.WriteLine(" Ingrese el regionId (numeros)");
                int regionId = Convert.ToInt32(Console.ReadLine());
                layer.RegionId = regionId;

                Console.Write("\n Archivo leido correctamente, no contiene erorres" + "\n");
                Console.WriteLine(" Importando archivos a la base de datos...");
                LayerRepository la = new LayerRepository(ConfigurationManager.ConnectionStrings["Earthwatchers_DSN"].ConnectionString);
                la.SaveLayerFull(layer);

                Console.WriteLine(" Archivos importados correctamente");
                Console.WriteLine(" Desea intersectar las lands con los poligonos? (( Y / N ))");

                var intersects = Console.ReadLine();
                if (intersects == "Y" || intersects == "y")
                {
                    LandRepository landrepo = new LandRepository(ConfigurationManager.ConnectionStrings["Earthwatchers_DSN"].ConnectionString);

                    var layerId = layer.Id;

                    Console.WriteLine(" Intersectando Lands con zona, Hora de inicio: " + DateTime.Now);

                    bool succeed = landrepo.IntersectLandsWithZone(regionId, layerId);
                    if (succeed)
                    {
                        Console.WriteLine(" Lands Actualizadas Correctamente");
                        Console.WriteLine(" Hora de fin: " + DateTime.Now);

                    }
                    else
                    {
                        Console.WriteLine(" Error al actualizar las lands");
                        Console.WriteLine(" Hora de fin: " + DateTime.Now);
                    }
                }
                
            }
            else
            {
                Console.WriteLine("\n Archivo leido correctamente, contiene erorres en los siguientes campos: " + "\n");

                foreach (string er in errors)
                {
                    Console.WriteLine(" " + er.ToString());
                }
                Console.WriteLine("\n Solucione los problemas pendientes y vuelva a cargar el archivo");
                Console.ReadLine();
            }
        }
    }
}