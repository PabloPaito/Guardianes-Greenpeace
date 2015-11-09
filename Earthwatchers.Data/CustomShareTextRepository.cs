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
    public class CustomShareTextRepository : ICustomShareTextRepository
    {
        private readonly IDbConnection connection;

        public CustomShareTextRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public List<CustomShareText> GetAll()
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            List<CustomShareText> shareTexts = connection.Query<CustomShareText>("EXEC CustomShareText_GetAll").ToList();
            connection.Close();
            if (shareTexts != null)
                return shareTexts;
            else
                return null;
        }

        public CustomShareText GetById(int id)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            CustomShareText shareText = connection.Query<CustomShareText>(string.Format("EXEC CustomShareText_GetById {0}", id)).FirstOrDefault();
            connection.Close();
            return shareText;
        }

        public CustomShareText GetByRegionIdAndLanguage(int regionId, string language)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            CustomShareText shareTexts = connection.Query<CustomShareText>(string.Format("EXEC CustomShareText_GetByRegionAndLanguage {0}, '{1}'", regionId, language)).FirstOrDefault();
            connection.Close();
            return shareTexts;
        }

        public CustomShareText Insert(CustomShareText shareText)
        {
            try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CustomShareText_Insert";

                cmd.Parameters.Add(new SqlParameter("@RegionId", shareText.RegionId));
                cmd.Parameters.Add(new SqlParameter("@Language", shareText.Language));
                cmd.Parameters.Add(new SqlParameter("@ShareOk", shareText.ShareOk));
                cmd.Parameters.Add(new SqlParameter("@ShareAlert", shareText.ShareAlert));
                cmd.Parameters.Add(new SqlParameter("@ShareAlertFinca", shareText.ShareAlertFinca));
                cmd.Parameters.Add(new SqlParameter("@HashTagRegister", shareText.HashTagRegister));
                cmd.Parameters.Add(new SqlParameter("@HashTagReportConfirmed", shareText.HashTagReportConfirmed));
                cmd.Parameters.Add(new SqlParameter("@HashTagRanking", shareText.HashTagRanking));
                cmd.Parameters.Add(new SqlParameter("@HashTagTop1", shareText.HashTagTop1));
                cmd.Parameters.Add(new SqlParameter("@HashTagCheck", shareText.HashTagCheck));
                cmd.Parameters.Add(new SqlParameter("@HashTagVerification", shareText.HashTagVerification));
                cmd.Parameters.Add(new SqlParameter("@HashTagDenounce", shareText.HashTagDenounce));
                cmd.Parameters.Add(new SqlParameter("@HashTagContestWon", shareText.HashTagContestWon));
                var idParameter = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idParameter);
                cmd.ExecuteNonQuery();
                connection.Close();
                
                return shareText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public CustomShareText Edit(CustomShareText shareText)
        {
            try
            {
                connection.Open();

                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CustomShareText_Edit";
                cmd.Parameters.Add(new SqlParameter("@RegionId", shareText.RegionId));
                cmd.Parameters.Add(new SqlParameter("@Language", shareText.Language));
                cmd.Parameters.Add(new SqlParameter("@ShareOk", shareText.ShareOk));
                cmd.Parameters.Add(new SqlParameter("@ShareAlert", shareText.ShareAlert));
                cmd.Parameters.Add(new SqlParameter("@ShareAlertFinca", shareText.ShareAlertFinca));
                cmd.Parameters.Add(new SqlParameter("@HashTagRegister", shareText.HashTagRegister));
                cmd.Parameters.Add(new SqlParameter("@HashTagReportConfirmed", shareText.HashTagReportConfirmed));
                cmd.Parameters.Add(new SqlParameter("@HashTagRanking", shareText.HashTagRanking));
                cmd.Parameters.Add(new SqlParameter("@HashTagTop1", shareText.HashTagTop1));
                cmd.Parameters.Add(new SqlParameter("@HashTagCheck", shareText.HashTagCheck));
                cmd.Parameters.Add(new SqlParameter("@HashTagVerification", shareText.HashTagVerification));
                cmd.Parameters.Add(new SqlParameter("@HashTagDenounce", shareText.HashTagDenounce));
                cmd.Parameters.Add(new SqlParameter("@HashTagContestWon", shareText.HashTagContestWon));
                cmd.Parameters.Add(new SqlParameter("@Id", shareText.Id));
                cmd.ExecuteNonQuery();

                connection.Close();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(int id)
        {
            connection.Open();

            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CustomShareText_Delete";
            cmd.Parameters.Add(new SqlParameter("@Id", id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}
