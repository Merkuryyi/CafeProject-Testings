namespace CafeApp.Models
{
    public class AppConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        
        public getConnection()
        {
            ConnectionString = "Host=localhost;Port=5432;Database=Cafe;Username=postgres;Password=;";
        }
    }
}