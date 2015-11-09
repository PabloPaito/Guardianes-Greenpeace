using Earthwatchers.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace Earthwatchers.Data
{
    public class RegionRepository : IRegionRepository
    {
        private readonly IDbConnection connection;

        public RegionRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public Region GetById(int regionId)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            Region region = connection.Query<Region>(string.Format("EXEC Region_GetById {0}", regionId)).FirstOrDefault();
            connection.Close();
            if (region != null)
                return region;
            else
                return null;
        }

        public Region GetByName(string Name)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            Region region = connection.Query<Region>(string.Format("EXEC Region_GetByName {0}", Name)).FirstOrDefault();
            connection.Close();
            if (region != null)
                return region;
            else
                return null;
        }

        public List<Region>GetAll()
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            List<Region> regions = connection.Query<Region>("EXEC Region_GetAllRegions").ToList();
            connection.Close();
            if (regions != null)
                return regions;
            else
                return null;
        }

        public List<Region> GetByCountryCode(string countryCode)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            List<Region> regions = connection.Query<Region>(string.Format("EXEC Region_GetByCountryCode {0}", countryCode)).ToList();
            connection.Close();
            if (regions != null)
                return regions;
            else
                return null;
        }

        public Region Insert(Region region)
        {
            try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Region_Insert";

                cmd.Parameters.Add(new SqlParameter("@Name", region.Name));
                cmd.Parameters.Add(new SqlParameter("@CountryCode", !string.IsNullOrWhiteSpace(region.CountryCode) ? region.CountryCode : "AR")); 
                cmd.Parameters.Add(new SqlParameter("@LowThreshold", region.LowThreshold));
                cmd.Parameters.Add(new SqlParameter("@HighThreshold", region.HighThreshold));
                cmd.Parameters.Add(new SqlParameter("@NormalPoints", region.NormalPoints));
                cmd.Parameters.Add(new SqlParameter("@BonusPoints", region.BonusPoints));
                cmd.Parameters.Add(new SqlParameter("@PenaltyPoints", region.PenaltyPoints));
                var idParameter = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idParameter);
                cmd.ExecuteNonQuery();

                connection.Close();
                return region;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Region Edit(Region region)
        {
            try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Region_Edit";
                cmd.Parameters.Add(new SqlParameter("@Name", region.Name));
                cmd.Parameters.Add(new SqlParameter("@CountryCode", !string.IsNullOrWhiteSpace(region.CountryCode)? region.CountryCode : "AR")); 
                cmd.Parameters.Add(new SqlParameter("@LowThreshold", region.LowThreshold));
                cmd.Parameters.Add(new SqlParameter("@HighThreshold", region.HighThreshold));
                cmd.Parameters.Add(new SqlParameter("@NormalPoints", region.NormalPoints));
                cmd.Parameters.Add(new SqlParameter("@BonusPoints", region.BonusPoints));
                cmd.Parameters.Add(new SqlParameter("@PenaltyPoints", region.PenaltyPoints));
                cmd.Parameters.Add(new SqlParameter("@Id", region.Id));
                cmd.ExecuteNonQuery();

                connection.Close();
                return region;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
