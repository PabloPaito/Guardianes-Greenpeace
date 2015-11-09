using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Earthwatchers.Models;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace Earthwatchers.Data
{
    public class SatelliteImageRepository : ISatelliteImageRepository
    {
        private readonly IDbConnection connection;

        public SatelliteImageRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public SatelliteImage Get(int id)
        {
            connection.Open();
            var satelliteImage = connection.Query<SatelliteImage>(string.Format("EXEC SatelliteImage_Get {0}", id)).FirstOrDefault();
            connection.Close();
            AddExtent(satelliteImage);
            return satelliteImage;
        }

        public SatelliteImage GetById(int id)
        {
            connection.Open();
            var satelliteImage = connection.Query<SatelliteImage>(string.Format("EXEC SatelliteImages_BE_GetById {0}", id)).FirstOrDefault();
            connection.Close();
            return satelliteImage;
        }

        public List<SatelliteImage> GetAll(int regionId )
        {
            connection.Open();
            var satelliteImages = new List<SatelliteImage>();
            if(regionId == 0)
            {
                satelliteImages = connection.Query<SatelliteImage>("EXEC SatelliteImages_BE_GetAll").ToList();
                connection.Close();
            }
            else
            {
                satelliteImages = connection.Query<SatelliteImage>(string.Format("EXEC SatelliteImage_GetAll {0}", regionId)).ToList();
                connection.Close();
                UpdateList(satelliteImages);
            }
            return satelliteImages;
        } 

        public List<SatelliteImage> Intersects(string wkt)
        {
            connection.Open();
            var satelliteImages = connection.Query<SatelliteImage>(string.Format("EXEC SatelliteImage_Intersects {0}" , wkt)).ToList();
            connection.Close();
            UpdateList(satelliteImages);
            return satelliteImages.ToList();
        }

        private void AddExtent(SatelliteImage satelliteImage)
        {
            satelliteImage.Extent = new Extent(satelliteImage.xmin, satelliteImage.ymin, satelliteImage.xmax, satelliteImage.ymax, 4326);
        }

        public void UpdateList(List<SatelliteImage> images)
        {
            foreach (var satelliteImage in images)
            {
                AddExtent(satelliteImage);
            }
        }

        public SatelliteImage Update(SatelliteImage satelliteImage)
        {
            try
            {
                connection.Open();
                if (satelliteImage.Published == null) satelliteImage.Published = DateTime.UtcNow.Date;
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SatelliteImage_Update";

                cmd.Parameters.Add(new SqlParameter("@id", satelliteImage.Id));
                cmd.Parameters.Add(new SqlParameter("@extent", satelliteImage.Wkt));
                cmd.Parameters.Add(new SqlParameter("@name", satelliteImage.Name));
                cmd.Parameters.Add(new SqlParameter("@published", satelliteImage.Published));
                cmd.Parameters.Add(new SqlParameter("@minlevel", satelliteImage.MinLevel));
                cmd.Parameters.Add(new SqlParameter("@maxlevel", satelliteImage.MaxLevel));
                cmd.Parameters.Add(new SqlParameter("@iscloudy", satelliteImage.IsCloudy));
                cmd.Parameters.Add(new SqlParameter("@isForestLaw", satelliteImage.IsForestLaw));
                cmd.Parameters.Add(new SqlParameter("@regionId", satelliteImage.RegionId));
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return satelliteImage;
        }

        public SatelliteImage Insert(SatelliteImage satelliteImage)
        {
            try
            {
                connection.Open();
                if (satelliteImage.Published == null) satelliteImage.Published = DateTime.UtcNow.Date;
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SatelliteImage_Insert";

                cmd.Parameters.Add(new SqlParameter("@extent", satelliteImage.Wkt));
                cmd.Parameters.Add(new SqlParameter("@name", satelliteImage.Name));
                cmd.Parameters.Add(new SqlParameter("@urltilecache", (object)satelliteImage.UrlTileCache ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@published", satelliteImage.Published));
                cmd.Parameters.Add(new SqlParameter("@minlevel", satelliteImage.MinLevel));
                cmd.Parameters.Add(new SqlParameter("@maxlevel", satelliteImage.MaxLevel));
                cmd.Parameters.Add(new SqlParameter("@iscloudy", satelliteImage.IsCloudy));
                cmd.Parameters.Add(new SqlParameter("@isForestLaw", satelliteImage.IsForestLaw));
                cmd.Parameters.Add(new SqlParameter("@regionId", satelliteImage.RegionId));
                var idParameter = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idParameter);
                cmd.ExecuteNonQuery();
                connection.Close();
                return satelliteImage;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(int id)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SatelliteImage_Delete";
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public List<string> GetImagesUrlPath()
        {
            connection.Open();
            var satelliteImages = connection.Query<string>("EXEC SatelliteImages_BE_GetImagesUrlPath").ToList();
            connection.Close();
            return satelliteImages.ToList();
        }

    }
}
