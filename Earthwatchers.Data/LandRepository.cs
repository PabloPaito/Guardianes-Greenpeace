using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Data;
using Dapper;
using Earthwatchers.Models;

namespace Earthwatchers.Data
{
    public class LandRepository : ILandRepository
    {
        private readonly IDbConnection connection;

        public LandRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Inserta los registros de la nueva land en la tabla lands
        /// </summary>
        /// <param name="newLand"></param>
        public void CreateLand(List<Land> newLand)
        {
            var nrland = newLand.Count.ToString();
            try
            {
                Debug.WriteLine("Start creating" + nrland + " land.");

                DataTable dt = new DataTable();
                dt.Columns.Add("GeohexKey", typeof(string));
                dt.Columns.Add("Confirmed", typeof(bool));
                dt.Columns.Add("LandStatus", typeof(int));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Observations", typeof(string));
                dt.Columns.Add("Lat", typeof(double));
                dt.Columns.Add("Long", typeof(double));
                dt.Columns.Add("BasecampId", typeof(int));
                dt.Columns.Add("LandThreat", typeof(int));
                dt.Columns.Add("RegionId", typeof(int));
                

                foreach (var land in newLand)
                {
                    DataRow row = dt.NewRow();
                    row[0] = land.GeohexKey;
                    row[1] = false;
                    row[2] = 0;
                    row[3] = null;
                    row[4] = null;
                    row[5] = land.Latitude;
                    row[6] = land.Longitude;
                    row[7] = land.BasecampId ?? (object)DBNull.Value;
                    row[8] = land.LandThreat.GetHashCode();
                    row[9] = land.RegionId;
                    dt.Rows.Add(row);

                    Debug.WriteLine(land.GeohexKey);
                }
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Lands_Create";
                command.Parameters.Add(new SqlParameter { ParameterName = "@lands", SqlDbType = System.Data.SqlDbType.Structured, Value = dt });
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public List<LandCSV> GetLandsCSV()
        {
            try
            {
                connection.Open();
                var l = connection.Query<LandCSV>("EXEC Land_GetLandsCSV");
                var lands = l.ToList();
                return lands;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        

        /// <summary>
        /// Trae los geoHexKey de las lands lockeadas o para demandar si es poll, sino trae los geoHexKey de las que estan confirmadas
        /// </summary>
        /// <param name="earthwatcherId"></param>
        /// <param name="isPoll"></param>
        /// <returns></returns>
        public List<string> GetVerifiedLandsGeoHexCodes(int earthwatcherId, bool isPoll)
        {
            List<string> values = new List<string>();
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                string sql = string.Empty;
                if (isPoll)
                {
                    sql = "EXEC Land_GetVerifiedLandsGeoHexCodes_1"; //Solo si es poll
                    if (earthwatcherId != 0)
                    {
                        sql = string.Format("EXEC Land_GetVerifiedLandsGeoHexCodes_2 {0}", earthwatcherId); //Solo si es poll y tiene EwId
                    }
                }
                else
                {
                    sql = "EXEC Land_GetVerifiedLandsGeoHexCodes_3";
                }

                command.CommandText = sql;
                values = connection.Query<string>(sql).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return values;
        }

        /// <summary>
        /// Trae los datos de la tabla de estadisticas
        /// </summary>
        /// <returns></returns>
        public List<Statistic> GetStats()
        {
            try
            {
                connection.Open();
                var stats = connection.Query<Statistic>("EXEC Lands_Stats");
                connection.Close();
                return stats.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Trae las parcelas a la pantalla de Land Backend para confirmar o deconfirmar
        /// </summary>
        /// <param name="page"></param>
        /// <param name="showVerifieds"></param>
        /// <returns></returns>
        public List<LandCSV> GetLandsToConfirm(int page, bool showVerifieds = false, int regionId = 0) 
        {
            try
            {
                connection.Open();

                var l = connection.Query<LandCSV>(string.Format("EXEC Land_GetLandsToConfirm {0}", regionId));
                connection.Close();

                //TODO: Llevar esta logica al StoredProcedure y resolver en BBDD.
                var pagesize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["backend.pagination.pagesize"]);
                IEnumerable<LandCSV> lands = null;
                if (showVerifieds)
                {
                    lands = l.Where(x => x.DemandAuthorities).ToList(); //only ready to demand
                }
                else
                {
                    lands = l.Where(x => !x.Confirmed.HasValue).ToList(); //only unverified
                }

                lands = lands.Skip((page - 1) * pagesize).Take(pagesize).ToList(); //pagination

                return lands.ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Resetea el estado de las parcelas verdes (acceso desde Backend)
        /// </summary>
        /// <returns></returns>
        public bool ResetLands(int regionId)
        {
            bool result = true;
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_ResetLands";
                command.Parameters.Add(new SqlParameter("@RegionId", regionId));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        /// <summary>
        /// Resetea las parcelas que fueron confirmadas como (sin desmonte) y las deja asignables nuevamente
        /// </summary>
        /// <param name="lands"></param>
        /// <param name="earthwatcherId"></param>
        /// <returns></returns>
        public bool ForceLandsReset(List<Land> lands, int earthwatcherId = 0) 
        { 
            bool result = true;
            if (lands.Any(x => x.Reset.HasValue && x.Reset.Value))
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("GeohexKey", typeof(string));
                    dt.Columns.Add("Confirmed", typeof(bool));
                    dt.Columns.Add("LandStatus", typeof(int));
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("Observations", typeof(string));
                    dt.Columns.Add("Lat", typeof(double));
                    dt.Columns.Add("Long", typeof(double));

                    foreach (var l in lands.Where(x => x.Reset.HasValue && x.Reset.Value))
                    {
                        DataRow row = dt.NewRow();
                        row[0] = l.GeohexKey;
                        row[1] = l.Confirmed;
                        row[2] = l.LandStatus;
                        row[3] = string.Empty;
                        row[4] = l.LastUsersWithActivity;
                        dt.Rows.Add(row);
                    }

                    connection.Open();

                    var command = connection.CreateCommand() as SqlCommand;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "Lands_ForceLandsReset";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@lands", SqlDbType = System.Data.SqlDbType.Structured, Value = dt });
                    command.Parameters.Add(new SqlParameter("@EarthwatcherId", earthwatcherId));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Deja rojas las parcelas que greenpeace dijo ser amarillas, y cambia el estado de las mal confirmadas.
        /// </summary>
        /// <param name="lands"></param>
        /// <param name="earthwatcherId"></param>
        /// <returns></returns>
        public bool UpdateLandsDemand(List<Land> lands, int earthwatcherId = 0)
        {
            bool result = true;
            if (lands.Any(x => x.Confirmed.HasValue))
            {
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("GeohexKey", typeof(string));
                    dt.Columns.Add("Confirmed", typeof(bool));
                    dt.Columns.Add("LandStatus", typeof(int));
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("Observations", typeof(string));
                    dt.Columns.Add("Lat", typeof(double));
                    dt.Columns.Add("Long", typeof(double));

                    foreach (var l in lands.Where(x => x.Confirmed.HasValue))
                    {
                        DataRow row = dt.NewRow();
                        row[0] = l.GeohexKey;
                        row[1] = l.Confirmed;
                        row[2] = l.LandStatus;
                        row[3] = string.Empty;
                        row[4] = l.LastUsersWithActivity;
                        dt.Rows.Add(row);
                    }

                    connection.Open();

                    var command = connection.CreateCommand() as SqlCommand;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "Lands_UpdateDemand";
                    command.Parameters.Add(new SqlParameter { ParameterName = "@lands", SqlDbType = System.Data.SqlDbType.Structured, Value = dt });
                    command.Parameters.Add(new SqlParameter("@EarthwatcherId", earthwatcherId));
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    result = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                result = false;
            }
            return result;
        } 

        /// <summary>
        /// Trae la imagen satelital de 2008
        /// </summary>
        /// <param name="isBase"></param>
        /// <returns></returns>
        public string GetImageBaseUrl(bool isBase, string geoHexKey)
        {
            string whereclause = isBase ? "Name like '%base%' and RegionId = (select regionId from land where GeohexKey = '" + geoHexKey + "')" : "Published = (Select MAX(Published) From SatelliteImage) and RegionId = (select regionId from land where GeohexKey = '" + geoHexKey + "')";  //TODO: STORED PROCEDURE PARA ESTE QUERY
            string sql = string.Format("Select UrlTileCache From SatelliteImage Where {0}", whereclause);
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                var baseUrl = command.ExecuteScalar();
                if (baseUrl != null && baseUrl != DBNull.Value)
                {
                    return baseUrl.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Es llamado por varios metodos, Hace un select en la base de datos con los parametros pasados
        /// </summary>
        /// <param name="id"></param>
        /// <param name="getAll"></param>
        /// <param name="earthwatcherId"></param>
        /// <param name="name"></param>
        /// <param name="geohexKey"></param>
        /// <param name="wkt"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Land> GetLands(int id = 0, bool getAll = false, int earthwatcherId = 0, string name = "", string geohexKey = "", string wkt = "POLYGON EMPTY", int status = 0, int playingRegion = 0)
        {
            try
            {
                connection.Open();
                var lands = connection.Query<Land>(string.Format("EXEC Lands_Get {0}, {1}, {2}, '{3}', '{4}', '{5}', {6}, '{7}'", id, getAll, earthwatcherId, name, geohexKey, wkt, status, playingRegion),null,null,true,60000);
                return lands.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        //REVISAR SP, USA UNA TABLA QUE NO EXISTE
        public void LoadThreatLevel() 
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Update_LandThreat";
                command.CommandTimeout = 7200000;
                command.ExecuteNonQuery();
            }
            catch
            {

            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Le pone el BasecampId a la land si es que intersecta con alguno, sino pone null
        /// </summary>
        public void LoadLandBasecamp(int fincaRegionId)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = string.Format("EXEC Update_LandBasecamp {0}", fincaRegionId);
                command.CommandTimeout = 7200000;
                command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Trae ciertos datos de todas las Lands de la base
        /// </summary>
        /// <returns>Id, Latitude, Longitude, GeohexKey, Distance, LandThreat</returns>
        public List<Land> GetAllLands()
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                var lands = connection.Query<Land>("EXEC Land_GetAllLands");
                return lands.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Devuelve La land que coincida con el Id pasado por parametro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Land GetLand(int id)  
        {
            return GetLands(id).FirstOrDefault();
        }

        /// <summary>
        /// Devuelve todas las tierras de un Earthwatcher pasando su Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Land> GetLandByEarthwatcherName(string name, int playingRegion)
        {
            return GetLands(0, false, 0, name, "", "POLYGON EMPTY", 0, playingRegion);
        }

        /// <summary>
        /// Devuelve todas las tierras de un Earthwatcher pasando su Id
        /// </summary>
        /// <param name="earthwatcherId"></param>
        /// <returns></returns>
        public List<Land> GetAll(int earthwatcherId, int playingRegion)
        {
            return GetLands(0, true, earthwatcherId, "", "", "POLYGON EMPTY", 0, playingRegion);
        }

        public List<Land> GetLandByIntersect(string wkt, int landId)
        {
            return GetLands(landId, false, 0, string.Empty, string.Empty, wkt);
        }

        /// <summary>
        /// Llama al metodo GetLands y le pasa el status
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Listado de lands con ese status</returns>
        public List<Land> GetLandByStatus(LandStatus status)
        {
            return GetLands(0, false, 0, string.Empty, string.Empty, "POLYGON EMPTY", (int)status);
        }

        /// <summary>
        /// Llama al metodo GetLands y solo le pasa el geoHexKey
        /// </summary>
        /// <param name="geoHexKey"></param>
        /// <returns>La primera Land que encuentra con ese GeoHexKey</returns>
        public Land GetLandByGeoHexKey(string geoHexKey)
        {
            return GetLands(0, false, 0, string.Empty, geoHexKey).FirstOrDefault();
        }

        /// <summary>
        /// Si ya la habia reportado la pasa a greenpeace, sino la deja para asignar
        /// </summary>
        /// <param name="earthwatcherId"></param>
        /// <param name="basecamp"></param>
        /// <returns></returns>
        public LandMini ReassignLand(int earthwatcherId) 
        {
            try
            {

            var earthwatcherRepository = new EarthwatcherRepository(connection.ConnectionString);
            var earthwatcher = earthwatcherRepository.GetEarthwatcher(earthwatcherId);

            var newLand = earthwatcherRepository.GetFreeLandByEarthwatcherIdAndPlayingRegion(earthwatcherId, earthwatcher.PlayingRegion);
            if (newLand != null && !String.IsNullOrEmpty(newLand.GeohexKey))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_ReassignLandByEarthwatcherId";
                command.Parameters.Add(new SqlParameter("@RegionId", earthwatcher.PlayingRegion));
                command.Parameters.Add(new SqlParameter("@EarthwatcherId", earthwatcherId));
                command.Parameters.Add(new SqlParameter("@newGeoHexKey", newLand.GeohexKey));
                command.ExecuteNonQuery();
                connection.Close();

                return newLand;
            }
            else return null;
            }
                catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool MassiveReassign(int regionId) //JAMAS SE USO, SE LLAMA DEL Backend
        {
            Dictionary<int, string> updateLands = new Dictionary<int, string>();
            string newGeoHexKeys = string.Empty;
            bool success = true;

            try
            {
                //1. Traigo todos los lands asignados
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                var lands = connection.Query<Land>("EXEC Land_MassiveReassign_sql").ToList();
                connection.Close();

                EarthwatcherRepository earthwatcherRepository = new EarthwatcherRepository(this.connection.ConnectionString);

                Random rand = new Random();
                //2. Genero un Random para tener 1/3 de posibilidades de asignar una tierra nueva y 2/3 de una tierra usada 
                int r = rand.Next(1, 4);

                for (int i = 0; i < lands.Count; i++)
                {
                    var land = lands[i];
                    if (updateLands.ContainsKey(land.EarthwatcherId.Value))
                    {
                        continue;
                    }

                    //3. El 1 y el 2 corresponden a asignar una parcela usada y el 
                    if (i > 0)
                    {
                        r++;
                        if (r > 3)
                        {
                            r = 1;
                        }
                    }

                    if (lands.Count > (updateLands.Count + 1))
                    {
                        if (r < 3)
                        {
                            //Parcela Usada
                            int ru = rand.Next(0, lands.Count - 1 - updateLands.Count);
                            //var l = lands.Where(x => x.Id != land.Id && !updateLands.ContainsKey(x.EarthwatcherId.Value) && !updateLands.ContainsValue(x.GeohexKey)).Skip(ru).FirstOrDefault();
                            var oldland = (from l in lands
                                           where (from u in updateLands where u.Key == l.EarthwatcherId.Value select u).Count() == 0
                                           select l).Skip(ru).FirstOrDefault();

                            if (oldland != null)
                            {
                                updateLands.Add(land.EarthwatcherId.Value, oldland.GeohexKey);
                                if (!updateLands.ContainsKey(oldland.EarthwatcherId.Value))
                                {
                                    updateLands.Add(oldland.EarthwatcherId.Value, land.GeohexKey);
                                }
                            }
                        }
                        else
                        {
                            var newFreeLand = earthwatcherRepository.GetFreeLand(regionId, newGeoHexKeys);
                            if (newFreeLand != null)
                            {
                                //Parcela Nueva
                                if (newGeoHexKeys != string.Empty)
                                {
                                    newGeoHexKeys += ",";
                                }
                                newGeoHexKeys += "'" + land.GeohexKey + "'";
                                newGeoHexKeys += ",'" + newFreeLand.GeohexKey + "'";

                                updateLands.Add(land.EarthwatcherId.Value, newFreeLand.GeohexKey);
                            }
                        }
                    }
                    else
                    {
                        //Ultima parcela impar
                        //Parcela Nueva
                        var newland = (from l in lands
                                       where (from u in updateLands where u.Key == l.EarthwatcherId.Value select u).Count() == 0
                                       select l).FirstOrDefault();

                        if (newland != null)
                        {
                            var newFreeLand = earthwatcherRepository.GetFreeLand(regionId, newGeoHexKeys);
                            if (newFreeLand != null)
                            {
                                if (newGeoHexKeys != string.Empty)
                                {
                                    newGeoHexKeys += ",";
                                }
                                newGeoHexKeys += "'" + land.GeohexKey + "'";
                                newGeoHexKeys += ",'" + newFreeLand.GeohexKey + "'";
                                updateLands.Add(newland.EarthwatcherId.Value, newFreeLand.GeohexKey);
                            }
                        }
                        break;
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("EarthwatcherId", typeof(int));
                dt.Columns.Add("GeohexKey", typeof(string));

                foreach (var land in updateLands)
                {
                    DataRow row = dt.NewRow();
                    row[0] = land.Key;
                    row[1] = land.Value;
                    dt.Rows.Add(row);
                }

                connection.Open();

                command.CommandText = "Lands_Reassign";
                command.Parameters.Add(new SqlParameter { ParameterName = "@lands", SqlDbType = System.Data.SqlDbType.Structured, Value = dt });
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                success = false;
                throw ex;
            }
            finally
            {
                connection.Close();
            }

            return success;
        }

        /// <summary>
        /// Cambia el status de a parcela (Id) por el que fue pasado por parametro
        /// </summary>
        /// <param name="id"></param>
        /// <param name="landStatus"></param>
        public void UpdateLandStatus(int id, LandStatus landStatus)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_UpdateLandStatus";
                command.Parameters.Add(new SqlParameter("@StatusChangedDateTime", DateTime.UtcNow));
                command.Parameters.Add(new SqlParameter("@LandStatus", landStatus));
                command.Parameters.Add(new SqlParameter("@Id", id));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Desasigna la parcela de su dueño, se la asigna al nuevo(earthwatcherid) y le pone status 2(sin revisar)
        /// </summary>
        /// <param name="geohex"></param>
        /// <param name="earthwatcherid"></param>
        public void AssignLandToEarthwatcher(string geohex, int earthwatcherid, int playingRegion)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_AssignLandToEarthwatcher";
                command.Parameters.Add(new SqlParameter("@PlayingRegion", playingRegion));
                command.Parameters.Add(new SqlParameter("@GeohexKey", geohex));
                command.Parameters.Add(new SqlParameter("@EarthwatcherId", earthwatcherid));
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //NO SE USA
        /// <summary>
        /// Elimina de EarthwatcherLands esa parcela y le cambia unicamente el status a 1(sin asignar)
        /// </summary>
        /// <param name="id"></param>
        public void UnassignLand(int id) 
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Land_UnassignLand";
            command.Parameters.Add(new SqlParameter("@Id", id));
            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Trae la lista de los usuarios que hicieron verificaciones en esa parcela
        /// </summary>
        /// <param name="landId"></param>
        /// <returns></returns>
        public List<Score> GetLastUsersWithActivityScore(int landId)
        {
            try
            {
                connection.Open();
                var scores = connection.Query<Score>(string.Format("EXEC Land_GetLastUsersWithActivityScore {0}", landId));
                return scores.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Agrega a la tabla verifications la verificacion del usuario
        /// </summary>
        /// <param name="land"></param>
        /// <param name="isAlert"></param>
        /// <returns></returns>
        public LandMini AddVerification(LandMini land, bool isAlert)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Land_AddVerification";
            //bool isAlertBool = isAlert ? 1 : 0;
            int verifications = connection.Query<int>(string.Format("EXEC Land_AddVerification {0}, {1}, {2}", land.LandId, land.Id, isAlert)).First();
            connection.Close();
            return VerificationScoring(land, verifications);
        }

        /// <summary>
        /// Guarda en la tabla PollResults el resultado de la poll
        /// </summary>
        /// <param name="land"></param>
        public void AddPoll(LandMini land)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_AddPoll";
                command.Parameters.Add(new SqlParameter("@GeohexKey", land.GeohexKey));
                command.Parameters.Add(new SqlParameter("@EarthwatcherId", land.EarthwatcherId));
                command.Parameters.Add(new SqlParameter("@IsUsed", land.IsUsed ? 1 : 0));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Busca al primer usuario que reporto esa parcela con 30 verif, le asigna los puntos, lockea la parcela y la pasa a greenpeace.
        /// </summary>
        /// <param name="land"></param>
        /// <param name="verifications"></param>
        /// <returns></returns>
        private LandMini VerificationScoring(LandMini land, int verifications)
        {
            if (verifications >= 30) //cantidad de Verifications
            {
                //Si es el usuario de Greenpeace debería buscar el owner original
                if (land.EarthwatcherId == Configuration.GreenpeaceId)
                {
                    connection.Open();

                    int? ewId = connection.Query<int>(string.Format("EXEC Land_VerificationScoring {0}, {1}", land.LandId, Configuration.GreenpeaceId)).SingleOrDefault();
                    if (ewId != null)
                    {
                        land.EarthwatcherId = Convert.ToInt32(ewId);
                    }
                    connection.Close();
                }

                //Obtengo el mail del owner original para mandarle la notificacion por mail
                connection.Open();
                string idobjMail = connection.Query<string>(string.Format("EXEC Land_VerificationScoring_2 {0}, {1}", land.LandId, Configuration.GreenpeaceId)).SingleOrDefault();
                if (idobjMail != null)
                {
                    land.Email = idobjMail;
                }
                connection.Close();

                //Obtengo el earthwatcher para agregarle los puntos
                connection.Open();
                Earthwatcher earthwatcher = connection.Query<Earthwatcher>(string.Format("EXEC Earthwatcher_GetEarthwatcher {0}", land.EarthwatcherId)).FirstOrDefault();
                connection.Close();

                //Le asigno los 2000 puntos
                var scoreRepository = new ScoreRepository(connection.ConnectionString);
                scoreRepository.PostScore(new Score(earthwatcher.Id, ActionPoints.Action.LandVerified.ToString(), ActionPoints.Points(ActionPoints.Action.LandVerified), earthwatcher.PlayingRegion,land.LandId));

                //El owner de la parcela pasa a ser Greenpeace y la parcela se lockea
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Land_VerificationScoring_3";
                cmd.Parameters.Add(new SqlParameter("@LandId", land.LandId));
                cmd.ExecuteNonQuery();
                connection.Close();
                return land;
            }
            return null;
        }

        /// <summary>
        /// Saca el porcentaje de parcelas revisadas (y para denuncia), 
        /// </summary>
        public decimal GetCheckPercentageByRegionId(int regionId)
        {
            decimal percentage = 0;
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                var sql = string.Format("EXEC Land_GetCheckPercentage {0}", regionId);
                command.CommandText = sql;
                percentage = connection.Query<decimal>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return percentage;
        }

        /// <summary>
        /// Saca el porcentaje de precisión en las denuncias, 
        /// </summary>
        public decimal? GetPrecicionDenouncePercentageByRegionId(int regionId, int earthwatcherId)
        {
            decimal? percentage = null;
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                var sql = string.Format("EXEC Land_PresicionDemand {0}, {1}", regionId, earthwatcherId);
                command.CommandText = sql;
                percentage = connection.Query<decimal?>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return percentage;
        }

        /// <summary>
        /// Modifica el tutorLand en la region por el LandId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="landStatus"></param>
        public void UpdateTutorLand(int landId, int regionId)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Land_UpdateTutorLand";
                command.Parameters.Add(new SqlParameter("@LandId", landId));
                command.Parameters.Add(new SqlParameter("@RegionId", regionId));
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public Land GetTutorLand(int regionId)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand() as SqlCommand;
                command.CommandType = CommandType.StoredProcedure;
                var tutorLand = connection.Query<Land>(string.Format("EXEC Land_GetTutorLand {0}", regionId));
                return tutorLand.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }
        public bool IntersectLandsWithZone(int regionId, int layerId)
        {
            try
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandTimeout = 14400000; //4 Hs
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Update_LandThreat";
                command.Parameters.Add(new SqlParameter("@RegionId", regionId));
                command.Parameters.Add(new SqlParameter("@LayerId", layerId));
                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
