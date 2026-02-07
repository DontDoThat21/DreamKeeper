using Microsoft.Extensions.Configuration;

namespace DreamKeeper.Data.Services
{
    /// <summary>
    /// Reads appsettings.json to load the SQLite connection string.
    /// Uses assembly location path traversal to locate the configuration file.
    /// </summary>
    public static class ConfigurationLoader
    {
        private static IConfiguration? _configuration;
        private static string? _databasePath;

        /// <summary>
        /// Sets the database path to use for the connection string.
        /// This should be called early in application startup before any database operations.
        /// </summary>
        public static void SetDatabasePath(string databasePath)
        {
            _databasePath = databasePath;
        }

        public static string GetConnectionString()
        {
            if (_configuration == null)
            {
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                // Try multiple paths to find appsettings.json
                string? configPath = null;

                if (!string.IsNullOrEmpty(assemblyDirectory))
                {
                    // Walk up to find the config file
                    var dir = assemblyDirectory;
                    for (int i = 0; i < 10; i++)
                    {
                        var candidate = Path.Combine(dir, "appsettings.json");
                        if (File.Exists(candidate))
                        {
                            configPath = dir;
                            break;
                        }
                        var parent = Directory.GetParent(dir);
                        if (parent == null) break;
                        dir = parent.FullName;
                    }
                }

                // Fallback: use AppContext.BaseDirectory
                if (configPath == null)
                {
                    configPath = AppContext.BaseDirectory;
                }

                _configuration = new ConfigurationBuilder()
                    .SetBasePath(configPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .Build();
            }

            var connectionString = _configuration.GetConnectionString("SqliteConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Use the configured database path, or fall back to a default
                var dbPath = _databasePath ?? "dream_database.db3";
                connectionString = $"Data Source={dbPath}";
            }

            return connectionString;
        }
    }
}
