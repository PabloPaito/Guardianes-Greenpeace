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
    public class ScoreRepository : IScoreRepository
    {
        private readonly IDbConnection connection;

        public ScoreRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public int GetScore(int id)
        {
            return Convert.ToInt32(GetScoresByUserId(id).Sum(s => s.Points));
        }
       
        public List<Score> GetScoresByUserId(int id)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            var scores = connection.Query<Score>(string.Format("EXEC Score_GetScoresByUserId {0}", id));
            connection.Close();
            return scores.ToList();
        }

        /// <summary>
        /// Trae todos los scores de los EW hechos en el pais de juego actual de cada uno, ordenados por puntaje de mayor a menor.
        /// </summary>
        /// <param name="isContest">Si es true se buscan scores hechos solo en las fechas del concurso</param>
        /// <returns></returns>
        public List<Rank> GetLeaderBoardNationalRanking(bool isContest, int RegionId)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            var ranking = connection.Query<Rank>(string.Format("EXEC Score_GetNationalLeaderBoard {0}, {1}", isContest, RegionId));
            connection.Close();
            return ranking.ToList();
        }

        /// <summary>
        /// Trae todos los puntajes de todos los Ew hechos en todos los paises
        /// </summary>
        /// <returns></returns>
        public List<Rank> GetLeaderBoardInternationalRanking()
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            var ranking = connection.Query<Rank>("EXEC Score_GetInternationalLeaderBoard");
            connection.Close();
            return ranking.ToList();
        }

        public Score UpdateScore(Score score)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Score_UpdateScore";
            cmd.Parameters.Add(new SqlParameter("@earthwatcherid", score.EarthwatcherId));
            cmd.Parameters.Add(new SqlParameter("@action", score.Action));
            cmd.Parameters.Add(new SqlParameter("@points", score.Points));
            cmd.ExecuteNonQuery();
            connection.Close();
            score.Published = DateTime.UtcNow;
            return score;
        }

        public bool AddLoginScore(string name)
        {
            try
            {
                connection.Open();
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Score_AddLoginScore";
                cmd.Parameters.Add(new SqlParameter("@name", name));
                cmd.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Score PostScore(Score score)
        {
            try
            {
                connection.Open();
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Score_PostScore";
                cmd.Parameters.Add(new SqlParameter("@earthwatcherid", score.EarthwatcherId));
                cmd.Parameters.Add(new SqlParameter("@action", score.Action));
                cmd.Parameters.Add(new SqlParameter("@points", score.Points));
                cmd.Parameters.Add(new SqlParameter("@landId", score.LandId));
                cmd.Parameters.Add(new SqlParameter("@regionId", score.RegionId));
                cmd.Parameters.Add(new SqlParameter("@param1", score.Param1));
                cmd.Parameters.Add(new SqlParameter("@param2", score.Param2));
                var idParameter = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(idParameter);
                cmd.ExecuteNonQuery();
                connection.Close();
                return score;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
