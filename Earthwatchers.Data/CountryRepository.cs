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
    public class CountryRepository : ICountryRepository
    {
        private readonly IDbConnection connection;

        public CountryRepository(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public List<Country>GetAll()
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            List<Country> countries = connection.Query<Country>("EXEC Country_GetAllCountries").ToList();
            connection.Close();
            if (countries != null)
                return countries;
            else
                return null;
        }
        public Country GetByCode(string CountryCode)
        {
            connection.Open();
            var cmd = connection.CreateCommand() as SqlCommand;
            cmd.CommandType = CommandType.StoredProcedure;
            Country country = connection.Query<Country>(string.Format("EXEC Country_GetCountryByCode {0}", CountryCode)).ToList().FirstOrDefault();
            connection.Close();
            if (country != null)
                return country;
            else
                return null;
        }        
    }
}
