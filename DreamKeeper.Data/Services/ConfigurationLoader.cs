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

            return _configuration.GetConnectionString("SqliteConnection")
                ?? "Data Source=dream_database.db3";
        }
    }
}
