using Npgsql;
using System;

namespace CafeApp.Models
{
    public class AppConfig
    {
        public string ConnectionString { get; set; } = "Host=localhost;Username=postgres;Password=6645;Database=Cafe";

        public static NpgsqlConnection GetSqlConnection()
        {
            NpgsqlConnection conn =
                new NpgsqlConnection("Host=host.docker.internal;Username=postgres;Password=6645;Database=new");
            conn.Open();
            return conn;
        }
    }
}