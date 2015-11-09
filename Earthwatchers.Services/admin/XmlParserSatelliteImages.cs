using Earthwatchers.Models;
using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace Earthwatchers.Services.admin
{
    public static class XmlParserSatelliteImages
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static List<string> ListImages()
        {
            var uri = ConfigurationManager.AppSettings.Get("xml.greenpeaceweb.path") + ConfigurationManager.AppSettings.Get("xml.satelliteimagesregion.path");
            List<string> regionsName = GetAllFolders(uri);

            return regionsName;
        }

        public static List<string> ListImagesToUnzip()
        {
            var uri = ConfigurationManager.AppSettings.Get("xml.greenpeaceweb.path") + ConfigurationManager.AppSettings.Get("ImagesListZip.path");
            List<string> imagesNamesToUnzip = GetAllFolders(uri);
            return imagesNamesToUnzip;
        }

        public static List<string> GetAllFolders(string uri)
        {
            List<string> regionsName = new List<string>();
            List<string> folderNamesAndUrl;
            string Rstring = string.Empty;
            WebRequest myWebRequest;
            WebResponse myWebResponse;
            HtmlDocument doc = new HtmlDocument();
            
            try
            {
                myWebRequest = WebRequest.Create(uri);
                myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource
                Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
                StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
                Rstring = sreader.ReadToEnd();//reads it to the end
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

            doc.LoadHtml(Rstring);

            List<string> allRegionsName = (from x in doc.DocumentNode.Descendants()
                           where x.Name == "a"
                           && x.Attributes["href"] != null
                           select x.Attributes["href"].Value).ToList<String>();

            folderNamesAndUrl = allRegionsName.Take(allRegionsName.Count - 1).Skip(1).ToList(); //Saco la primera (Base URL) y la ultima (WebConfig)
            
            foreach(var f in folderNamesAndUrl)
            {
                regionsName.Add((f.Split('/')[3]));
            }
            return regionsName;
        }

        public static SatelliteImage ReadXmlFileAndParseImage(string imageName) // Name Format:  RegionId_ImageName
        {
            //Paths and loading document
            var path = ConfigurationManager.AppSettings.Get("xml.greenpeaceweb.path") + ConfigurationManager.AppSettings.Get("xml.satelliteimagesregion.path");
            //TODO: Agegar el regionId al path /1/2008 /X/imageName
            var unifiedPath = path + imageName + "/tilemapresource.xml";
            string Rstring = string.Empty;
            WebRequest myWebRequest;
            WebResponse myWebResponse;

            try
            {
                myWebRequest = WebRequest.Create(unifiedPath);
                myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource
                Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
                StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
                Rstring = sreader.ReadToEnd();//reads it to the end
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error reading file. unifiedPath: {0}, exception: {1}", unifiedPath, ex));
            }
            XDocument xDoc = XDocument.Parse(Rstring);

            //Variables declarations
            int minLevel, maxLevel, regionId = 0;
            double xMin, xMax, yMin, yMax = 0;
            string name;
            var doc = xDoc.Descendants("TileMap").ToList();

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            //Xmin Xmax Ymin Ymax
            var xyMinMax = doc.Elements("BoundingBox").First();
            Double.TryParse(xyMinMax.Attribute("minx").Value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out xMin);
            Double.TryParse(xyMinMax.Attribute("maxx").Value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out xMax);
            Double.TryParse(xyMinMax.Attribute("miny").Value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out yMin);
            Double.TryParse(xyMinMax.Attribute("maxy").Value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out yMax);

            logger.Info(string.Format("culture: xmin - xmax - ymin - ymax:  {0} --- {1} --- {2} --- {3}", xMin, xMax, yMin, yMax));

            //Zoom Levels
            var zoomLevels = doc.Elements("TileSets").First();
            var levels = zoomLevels.Elements().ToList();
            var l1 = levels.First();
            var l2 = levels.Last();
            minLevel = Convert.ToInt32(l1.Attribute("order").Value);
            maxLevel = Convert.ToInt32(l2.Attribute("order").Value);
            
            //Name Data
            try
            {
                regionId = Convert.ToInt32(imageName.Split('_').FirstOrDefault());
            }
            catch
            {
                regionId = 1;
            }
            name = imageName;//.Substring(imageName.IndexOf("_") + 1);
            
            SatelliteImage newImage = new SatelliteImage();
            newImage.Published = DateTime.Now;
            newImage.IsCloudy = false;
            newImage.IsForestLaw = false;
            newImage.Name = name;
            newImage.xmin = xMin;
            newImage.xmax = xMax;
            newImage.ymin = yMin;
            newImage.ymax = yMax;
            newImage.Wkt = "POLYGON((" + yMin + " " + xMin + ", " + yMax + " " + xMin + ", " + yMax + " " + xMax + ", " + yMin + " " + xMax + ", " + yMin + " " + xMin + "))";
            newImage.MinLevel = minLevel;
            newImage.MaxLevel = maxLevel;
            newImage.RegionId = regionId;
            newImage.UrlTileCache = path + imageName;
            
            return newImage;
        }
    }
}