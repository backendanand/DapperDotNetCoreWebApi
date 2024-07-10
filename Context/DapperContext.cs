using Microsoft.Data.SqlClient;
using System.Data;

namespace WebApiUsingDapper.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _iconfiguration;
        private readonly string _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _iconfiguration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);


    }
}
