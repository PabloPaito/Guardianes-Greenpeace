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
    public class BasecampRepository : IBasecampRepository
    {
        private readonly IDbConnection connection;

        public BasecampRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public List<Basecamp> Get()
        {
            connection.Open();
            var basecamps = connection.Query<Basecamp>("EXEC Basecamp_Get");
            connection.Close();
            return basecamps.ToList();
        }

        public List<Basecamp> GetByBasecampRegion(int basecampRegion)
        {
            connection.Open();
            var basecamps = connection.Query<Basecamp>(string.Format("EXEC Basecamp_GetByBasecamp {0}", basecampRegion));
            connection.Close();
            return basecamps.ToList();
        }

        public List<Basecamp> GetBaseCamps()
        {
            connection.Open();
            var basecamps = connection.Query<Basecamp>("EXEC Basecamp_GetBaseCamps");
            connection.Close();
            return basecamps.ToList();
        }

        public Basecamp Insert(Basecamp basecamp)
        {
            try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
               
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Basecamp_Insert";
                
                cmd.Parameters.Add(new SqlParameter("@longitude", basecamp.Longitude));
                cmd.Parameters.Add(new SqlParameter("@latitude", basecamp.Latitude));
                cmd.Parameters.Add(new SqlParameter("@hpLongitude", basecamp.HotPointLong));
                cmd.Parameters.Add(new SqlParameter("@hpLatitude", basecamp.HotPointLat));
                cmd.Parameters.Add(new SqlParameter("@probability", basecamp.Probability));
                cmd.Parameters.Add(new SqlParameter("@name", basecamp.DetailName));
                cmd.Parameters.Add(new SqlParameter("@shortText", basecamp.ShortText));
                cmd.Parameters.Add(new SqlParameter("@region", basecamp.RegionId));
                cmd.Parameters.Add(new SqlParameter("@show", basecamp.Show));
                var idParameter = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idParameter);
                cmd.ExecuteNonQuery();

                connection.Close();
                return basecamp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Basecamp GetById(int id)
        {
            connection.Open();
            var basecamp = connection.Query<Basecamp>(string.Format("EXEC Basecamp_GetById {0}", id)).ToList();
            connection.Close();
            return basecamp.FirstOrDefault();
        }

        public Basecamp Edit(Basecamp basecamp)
        {
             try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Basecamp_Edit";
                cmd.Parameters.Add(new SqlParameter("@longitude", basecamp.Longitude));
                cmd.Parameters.Add(new SqlParameter("@latitude", basecamp.Latitude));
                cmd.Parameters.Add(new SqlParameter("@hpLongitude", basecamp.HotPointLong));
                cmd.Parameters.Add(new SqlParameter("@hpLatitude", basecamp.HotPointLat));
                cmd.Parameters.Add(new SqlParameter("@probability", basecamp.Probability));
                cmd.Parameters.Add(new SqlParameter("@name", basecamp.DetailName));
                cmd.Parameters.Add(new SqlParameter("@shortText", basecamp.ShortText));
                cmd.Parameters.Add(new SqlParameter("@region", basecamp.RegionId));
                cmd.Parameters.Add(new SqlParameter("@show", basecamp.Show));
                cmd.Parameters.Add(new SqlParameter("@id", basecamp.IdDb));
                cmd.ExecuteNonQuery();

                connection.Close();
                return basecamp;
            }
             catch (Exception ex)
             {
                 throw ex;
             }
        }

        public void RecalculateDistance(int id, bool allInRegion)
        {
                try
            {
                if (id != 0)
                {
                    connection.Open();
                    var cmd = connection.CreateCommand() as SqlCommand;
                    cmd.CommandTimeout = 7200000;
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    if(allInRegion)
                    {
                        cmd.CommandText = "ReCalculate_LandDistanceFromBasecamps";
                        cmd.Parameters.Add(new SqlParameter("@region", id));
                    }
                    else
                    {
                        cmd.CommandText = "ReCalculate_LandDistanceFromBasecampId";
                        cmd.Parameters.Add(new SqlParameter("@basecampId", id));
                    }

                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(int id)
        {
            try
            {
                connection.Open();
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Basecamp_Delete";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
