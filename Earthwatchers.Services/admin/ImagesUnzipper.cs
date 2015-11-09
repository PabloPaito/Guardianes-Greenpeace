using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;


namespace Earthwatchers.Services.admin
{
    public class ImagesUnzipper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void ListImagesToUnzip()
        {

        }

        public static void Unzip(string zipLocation, string unzipLocation, string fileName)
        {
            //Unzip in Live Folder  -  //ImagesZip.path / ImagesUnZipped.path
            ZipFile.ExtractToDirectory(zipLocation + "\\" + fileName, unzipLocation + "\\" + fileName.Replace(".zip",""));

            try
            {
            //Move to Archieve
            var archivePath = ConfigurationManager.AppSettings.Get("ImagesExtractedZip.path");
            
            //Rename
            var archiveName = fileName.Replace(".zip", string.Format("_{0}.zip", "archive"));
            File.Move(zipLocation + "\\" + fileName, zipLocation + "\\" + archiveName);

            System.IO.File.Move(zipLocation + "//" + archiveName, archivePath + "//" + archiveName);
            }
            catch(Exception ex)
            {
                logger.Info(string.Format("Exception msje {0}, ex {1}", ex.Message, ex));
                throw ex;
            }
        }

        public static void ZipThisFile(string fileLocation, string fileZippedDestination)
        {
            ZipFile.CreateFromDirectory(fileLocation, fileZippedDestination);

        }
    }
}