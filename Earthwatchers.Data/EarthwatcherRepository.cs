using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Earthwatchers.Models;
using System.Data.SqlClient;
using Dapper;

namespace Earthwatchers.Data
{
    public class EarthwatcherRepository : IEarthwatcherRepository
    {
        private readonly IDbConnection connection;

        public EarthwatcherRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Devuelve todos los scores de un Earthwatcher pasando su Id
        /// </summary>
        /// <param name="Earthwatcher"></param>
        /// <returns></returns>
        private int GetTotalScore(int Earthwatcher)
        {
            var scoreRepository = new ScoreRepository(connection.ConnectionString);
            return scoreRepository.GetScore(Earthwatcher);
        }

        /// <summary>
        /// Devuelve los datos del earthwatcher Pasando su Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Earthwatcher GetEarthwatcher(int id)
        {
            connection.Open();
            var earthwatchers = connection.Query<Earthwatcher>(string.Format("EXEC Earthwatcher_GetEarthwatcher {0}", id));
            connection.Close();
            var earthwatcher = earthwatchers.FirstOrDefault();
            if (earthwatcher != null)
            {
                earthwatcher.Lands = GetEarthwatcherLands(earthwatcher.Name, earthwatcher.PlayingRegion);
            }
            return earthwatcher;
        }

        /// <summary>
        /// Devuelve true or false, si es que existe en la base de datos un earthwatcher con ese nombre
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool EarthwatcherExists(string name)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_EarthwatcherExists";
            cmd.Parameters.Add(new SqlParameter("@name", name));
            object obj = cmd.ExecuteScalar();
            connection.Close();
            if (obj == null)
            {
                return false;
            }
            return true;
        }
      
        /// <summary>
        /// Devuelve todos los campos de un ApiEW pasando el nombre de la api y el id del usuario
        /// </summary>
        /// <param name="api">Social Network Name</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ApiEw GetApiEw(string api, string userId)  
        {
            connection.Open();
            var earthwatchers = connection.Query<ApiEw>(string.Format("EXEC Earthwatcher_GetApiEw {0}, {1}", api, userId));
            connection.Close();
            var earthwatcher = earthwatchers.FirstOrDefault();
            
            return earthwatcher;
        }

        /// <summary>
        /// Devuelve todos los campos de un ApiEW pasando el id del ApiEW
        /// </summary>
        /// <param name="apiEwId"></param>
        /// <returns></returns>
        public ApiEw GetApiEwById(int apiEwId) 
        {
            connection.Open();
            var earthwatchers = connection.Query<ApiEw>(string.Format("EXEC Earthwatcher_GetApiEwById {0}", apiEwId));
            connection.Close();
            var earthwatcher = earthwatchers.FirstOrDefault();

            return earthwatcher;
        }

        /// <summary>
        /// Inserta el registro en la tabla ApiEwLogin
        /// </summary>
        /// <param name="ew"></param>
        /// <returns></returns>
        public ApiEw CreateApiEwLogin(ApiEw ew)  
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_CreateApiEwLogin";
            cmd.Parameters.Add(new SqlParameter("@Api", ew.Api));
            cmd.Parameters.Add(new SqlParameter("@UserId", ew.UserId));
            cmd.Parameters.Add(new SqlParameter("@NickName", ew.NickName));
            cmd.Parameters.Add(new SqlParameter("@SecretToken", ew.SecretToken));
            cmd.Parameters.Add(new SqlParameter("@AccessToken", ew.AccessToken));
            cmd.Parameters.Add(new SqlParameter("@Mail", ew.Mail));
            var idParameter = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(idParameter);
            cmd.ExecuteNonQuery();
            var id = (int)idParameter.Value;
            ew.Id = id;
            connection.Close();
            return ew;
        }
    
        /// <summary>
        /// Updatea la tabla ApiEwLogin con el EarthwatcherId y la tabla Earthwatcher con el UserId
        /// </summary>
        /// <param name="apiEwId"></param>
        /// <param name="EwId"></param>
        public void LinkApiAndEarthwatcher(int apiEwId, int EwId)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_LinkApiAndEarthwatcher";
            cmd.Parameters.Add(new SqlParameter("@ApiEwId", apiEwId));
            cmd.Parameters.Add(new SqlParameter("@EwId", EwId));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Updatea el access token del guardian cada vez que se loguea con una red social
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="AccessToken"></param>
        public void UpdateAccessToken(int Id, string AccessToken)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_UpdateAccessToken";
            cmd.Parameters.Add(new SqlParameter("@ApiEwId", Id));
            cmd.Parameters.Add(new SqlParameter("@AccessToken", AccessToken));
            cmd.ExecuteNonQuery();
            connection.Close();
        }
      
        /// <summary>
        /// Devuelve todos los datos de un earthwatcher, inclusive con su listado de lands, dependiendo del parametro getLands
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getLands"></param>
        /// <returns></returns>
        public Earthwatcher GetEarthwatcher(string name, bool getLands)
        {
            try
            {
                connection.Open();
                //var earthwatchers = connection.Query<Earthwatcher>(string.Format("EXEC Earthwatcher_GetEarthwatcherByName {0}", name));
                var earthwatchers = connection.Query<Earthwatcher>("select Id, EarthwatcherGuid as Guid, Name, Role, PasswordPrefix, hashedpassword, country, avatar, FullName, email, Telephone, IsPowerUser, Language, Region, NotifyMe, NickName, CreatedDate, ApiEwId, AllowAutoShare, PlayingRegion, PlayingCountry from Earthwatcher where Name = @Name", new { Name = name });
                connection.Close();

                var earthwatcher = earthwatchers.FirstOrDefault();
                if (earthwatcher != null && getLands)
                {
                    //Traigo las lands del usuario
                    earthwatcher.Lands = GetEarthwatcherLands(earthwatcher.Name, earthwatcher.PlayingRegion);

                    //Si no tiene ninguna, para que no pinche le asigno una
                    if (earthwatcher.Lands.Count == 0)
                    {
                        var newLand = AssignLandToEarthwatcher(earthwatcher.Id, string.Empty);
                        if(newLand != null)
                        {
                            var landRepository = new LandRepository(connection.ConnectionString);
                            Land newLandObj = landRepository.GetLandByGeoHexKey(newLand.GeohexKey);
                            if (newLandObj != null)
                            {
                                earthwatcher.Lands.Add(newLandObj);
                            }
                        }
                    }
                }
                return earthwatcher;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Devuelve todos los datos de un earthwatcher pasandole su guid
        /// </summary>
        public Earthwatcher GetEarthwatcherByGuid(Guid guid)
        {
            connection.Open();
            var earthwatchers = connection.Query<Earthwatcher>(string.Format("EXEC Earthwatcher_GetEarthwatcherByGuid '{0}'", guid));
            connection.Close();
            var earthwatcher = earthwatchers.FirstOrDefault();
            if (earthwatcher != null)
            {
                earthwatcher.Lands = GetEarthwatcherLands(earthwatcher.Name, earthwatcher.PlayingRegion);
            }
            return earthwatcher;
        }

        /// <summary>
        /// Devuelve las Lands de un earthwatcher pasando su nombre
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<Land> GetEarthwatcherLands(string name, int playingRegion)
        {
            var landRepository = new LandRepository(connection.ConnectionString);
            return landRepository.GetLandByEarthwatcherName(name, playingRegion);
        }


        /// <summary>
        /// Inserta en la tabla earthwatcher al usuario creado
        /// </summary>
        /// <param name="earthwatcher"></param>
        /// <returns></returns>
        public Earthwatcher CreateEarthwatcher(Earthwatcher earthwatcher)
        {
            var passwordPrefix = "";
            var hashedPassword = "";
            if (earthwatcher.Password != null)
            {
               passwordPrefix = PasswordChecker.GeneratePrefix();
               hashedPassword = PasswordChecker.GenerateHashedPassword(earthwatcher.Password, passwordPrefix);
            }
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_CreateEarthwatcher";  
            cmd.Parameters.Add(new SqlParameter("@guid", earthwatcher.Guid));
            if(string.IsNullOrEmpty(earthwatcher.Name))
            {
                cmd.Parameters.Add(new SqlParameter("@name", ""));
            }
            else
            {
                cmd.Parameters.Add(new SqlParameter("@name", earthwatcher.Name));
            }
            cmd.Parameters.Add(new SqlParameter("@role", earthwatcher.Role));
            cmd.Parameters.Add(new SqlParameter("@prefix", passwordPrefix));
            cmd.Parameters.Add(new SqlParameter("@hash", hashedPassword));
            cmd.Parameters.Add(new SqlParameter("@country", earthwatcher.Country));
            cmd.Parameters.Add(new SqlParameter("@language", earthwatcher.Language));
            cmd.Parameters.Add(new SqlParameter("@playingCountry", earthwatcher.PlayingCountry));
            cmd.Parameters.Add(new SqlParameter("@playingRegion", earthwatcher.PlayingRegion));
            if (!string.IsNullOrEmpty(earthwatcher.NickName))
            {
                cmd.Parameters.Add(new SqlParameter("@nick", earthwatcher.NickName));
            }
            if (earthwatcher.PlayingRegion == null || earthwatcher.PlayingRegion == 0)
            {
                var regionRepository = new RegionRepository(connection.ConnectionString);
                List<Region> regions = regionRepository.GetByCountryCode(earthwatcher.Country);
                earthwatcher.PlayingRegion = regions.FirstOrDefault().Id;
            }
            
            var idParameter = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(idParameter);
            cmd.ExecuteNonQuery();
            var id = (int)idParameter.Value;
            earthwatcher.Id = id;
            connection.Close();
            return earthwatcher;
        }

        /// <summary>
        /// Actualiza el password de un usuario
        /// </summary>
        /// <param name="earthwatcher"></param>
        /// <param name="passwordPrefix"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public bool UpdatePassword(Earthwatcher earthwatcher, string passwordPrefix, string hashedPassword)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_UpdatePassword";
            cmd.Parameters.Add(new SqlParameter("@name", earthwatcher.Name));
            cmd.Parameters.Add(new SqlParameter("@prefix", passwordPrefix));
            cmd.Parameters.Add(new SqlParameter("@hash", hashedPassword));
            cmd.ExecuteNonQuery();
            connection.Close();
            return true;
        }

        /// <summary>
        /// Cambia el campo IsPowerUser de false a true de un earthwatcher (solo asigna un badge distinto)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="earthwatcher"></param>
        public void SetEarthwatcherAsPowerUser(int id, Earthwatcher earthwatcher)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_SetEarthwatcherAsPowerUser";
            cmd.Parameters.Add(new SqlParameter("@isPowerUser", true)); 
            cmd.Parameters.Add(new SqlParameter("@id", earthwatcher.Id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Actualiza los datos de un earthwatcher (excepto el password)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="earthwatcher"></param>
        public void UpdateEarthwatcher(int id, Earthwatcher earthwatcher)
        {
            try
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Earthwatcher_UpdateEarthwatcher";
                cmd.Parameters.Add(new SqlParameter("@role", earthwatcher.Role));
                cmd.Parameters.Add(new SqlParameter("@country", earthwatcher.Country != null? earthwatcher.Country : ""));
                cmd.Parameters.Add(new SqlParameter("@name", earthwatcher.Name));  //PARA CAMBIAR EL MAIL
                if (!string.IsNullOrEmpty(earthwatcher.Region))
                {
                    cmd.Parameters.Add(new SqlParameter("@region", earthwatcher.Region));
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@region", DBNull.Value));
                }
                cmd.Parameters.Add(new SqlParameter("@language", earthwatcher.Language != null ? earthwatcher.Language : ""));
                cmd.Parameters.Add(new SqlParameter("@notifyMe", earthwatcher.NotifyMe));
                cmd.Parameters.Add(new SqlParameter("@allowAutoShare", earthwatcher.AllowAutoShare));
                cmd.Parameters.Add(new SqlParameter("@nickname", earthwatcher.NickName));
                cmd.Parameters.Add(new SqlParameter("@id", earthwatcher.Id));
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un earthwatcher pasando su Id (HAY QUE REVISAR LA INTEGRIDAD REFERENCIAL DE ESTE SP)
        /// </summary>
        /// <param name="id"></param>
        public void DeleteEarthwatcher(int id)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_DeleteEarthwatcher";
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Trae el listado completo de earthwatchers
        /// </summary>
        /// <returns></returns>
        public List<Earthwatcher> GetAllEarthwatchers()
        {
            connection.Open();
            var earthwatchers = connection.Query<Earthwatcher>("EXEC Earthwatcher_GetAllEarthwatchers");
            connection.Close();

            return earthwatchers.ToList();
        }

        public LandMini AssignLandToEarthwatcher(int earthwatcherId, string geohexKey)
        {
            var earthwatcher = GetEarthwatcher(earthwatcherId);

            var land = GetFreeLandByEarthwatcherIdAndPlayingRegion(earthwatcherId, earthwatcher.PlayingRegion);
            if (land != null)
            {
                var landRepository = new LandRepository(connection.ConnectionString);
                landRepository.AssignLandToEarthwatcher(land.GeohexKey, earthwatcherId, earthwatcher.PlayingRegion);
            }
            return land;
        }

        /// <summary>
        /// Busca una Land para asignarle al Earthwatcher
        /// Primero busca una finca para asignarle (segun la probabilidad)
        /// Luego busca una Land que cumpla con los requisitos para ser asignada, tomando en cuenta el Basecamp asignado
        /// </summary>
        /// <param name="baseCamp"></param>
        /// <param name="geohexKey"></param>
        /// <returns></returns>
        public LandMini GetFreeLand(int regionId, string geohexKey)
        {
            //Primero asigno un basecamp
            var basecampRepository = new BasecampRepository(connection.ConnectionString);
            List<Basecamp> basecamps = basecampRepository.GetByBasecampRegion(regionId);
            bool usedLand = false;
            string newgeohexKey = null;
            if (basecamps != null && basecamps.Count > 0)
            {
                int total = basecamps.Sum(x => x.Probability);
                int acumprob = 0;
                Random rand = new Random();
                int r = rand.Next(1, total + 1);
                int basecampid = 0;
                foreach (var basecamp in basecamps)
                {
                    acumprob += basecamp.Probability;
                    if (r < acumprob)
                    {
                        basecampid = basecamp.Id;
                        break;
                    }
                }

                if (basecampid != 0)
                {
                    var sql = "Earthwatcher_GetFreeLand";
                    connection.Open();
                    var cmd = connection.CreateCommand() as SqlCommand;
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BasecampDetailId", basecampid);
                    //cmd.Parameters.AddWithValue("@TutorLandId", Configuration.getTutorLandIdByRegion(regionId));
                    cmd.Parameters.AddWithValue("@GeoHexKey", geohexKey);
                    cmd.Parameters.AddWithValue("@RegionId", regionId);

                    var reader = cmd.ExecuteReader();
                    var hasResult = reader.Read();
                    int landId = 0;
                    if (hasResult)
                    {
                        landId = reader.GetInt32(0);
                        newgeohexKey = reader.GetString(2);
                        usedLand = reader.GetBoolean(3);
                    }
                    connection.Close();
                    return new LandMini { GeohexKey = newgeohexKey, IsUsed = usedLand, Id = landId };
                }

            }

            return null;
        }

        public LandMini GetFreeLandByEarthwatcherIdAndPlayingRegion(int earthwatcherId, int playingRegion)
        {
            //Primero asigno un basecamp
            var basecampRepository = new BasecampRepository(connection.ConnectionString);
            List<Basecamp> basecamps = basecampRepository.GetByBasecampRegion(playingRegion); 

            bool usedLand = false;
            string newgeohexKey = null;
            int basecampid = 0;

            //Si el pais tiene basecamps eligo segun la probabilidad
            if (basecamps != null && basecamps.Count > 0)
            {
                int total = basecamps.Sum(x => x.Probability);
                int acumprob = 0;
                Random rand = new Random();
                int r = rand.Next(1, total + 1);
                foreach (var basecamp in basecamps)
                {
                    acumprob += basecamp.Probability;
                    if (r < acumprob)
                    {
                        basecampid = basecamp.Id;
                        break;
                    }
                }
            }

            //Busco una land nueva para el usuario, dependiendo del pais, el basecamp, y el hot point de la parcela.
            try
            {
                var sql = "Earthwatcher_GetFreeLandByEarthwatcherId";
                connection.Open();
                var cmd = connection.CreateCommand() as SqlCommand;
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BasecampDetailId", basecampid);
                //cmd.Parameters.AddWithValue("@TutorLandId", Configuration.getTutorLandIdByRegion(playingRegion));
                cmd.Parameters.AddWithValue("@EarthwatcherId", earthwatcherId);
                cmd.Parameters.AddWithValue("@PlayingRegion", playingRegion);

                var reader = cmd.ExecuteReader();
                var hasResult = reader.Read();
                int landId = 0;
                if (hasResult)
                {
                    landId = reader.GetInt32(0);
                    newgeohexKey = reader.GetString(2);
                    usedLand = reader.GetBoolean(3);
                }
                connection.Close();
                return new LandMini { GeohexKey = newgeohexKey, IsUsed = usedLand, Id = landId };
            }
            catch (Exception ex)
            {
                //throw new Exception("Ha ocurrido una excepcion :", ex); //TODO: HANDLER DE ESTA EXCEPTION
                return null;
            }

        }

        public void ChangePlayingRegion(Earthwatcher earthwatcher)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Earthwatcher_ChangePlayingRegion";
            cmd.Parameters.Add(new SqlParameter("@playingRegion", earthwatcher.PlayingRegion));
            cmd.Parameters.Add(new SqlParameter("@playingCountry", earthwatcher.PlayingCountry));
            cmd.Parameters.Add(new SqlParameter("@id", earthwatcher.Id));
            cmd.ExecuteNonQuery();
            connection.Close();
        }

    }
}
