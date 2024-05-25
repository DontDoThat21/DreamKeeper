using System.Reflection;
using System.Text.Json;

namespace Muffle.Data.Services
{
    public static class ConfigurationLoader
    {
        public static string GetConnectionString(string name)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string assemblyLocation = Path.GetDirectoryName(assembly.Location);
            string projectDirectory = Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation));
            string csProjDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(projectDirectory)));

            var configPath = Path.Combine(csProjDir, "appsettings.json");

            var jsonString = File.ReadAllText(configPath);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                connectionStrings.TryGetProperty(name, out var connectionString))
            {
                return connectionString.GetString();
            }

            throw new KeyNotFoundException($"Connection string '{name}' not found.");
        }
    }
}
