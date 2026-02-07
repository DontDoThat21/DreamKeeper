using Dapper;
using DreamKeeper.Data.Models;
using Microsoft.Data.Sqlite;

namespace DreamKeeper.Data.Services
{
    /// <summary>
    /// Manages the raw SQLite connection and database lifecycle.
    /// </summary>
    public static class SQLiteDbService
    {
        /// <summary>
        /// Returns a new SqliteConnection using the connection string from appsettings.json.
        /// </summary>
        public static SqliteConnection CreateConnection()
        {
            var connectionString = ConfigurationLoader.GetConnectionString();
            return new SqliteConnection(connectionString);
        }

        /// <summary>
        /// Creates the Dreams table (if not exists) and inserts seed data in DEBUG mode.
        /// </summary>
        public static void InitializeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Dreams (
                    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                    DreamName         TEXT NOT NULL,
                    DreamDescription  TEXT,
                    DreamDate         TEXT NOT NULL,
                    DreamRecording    BLOB
                );
            ");

#if DEBUG
            // Insert seed data only if the table is empty
            var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Dreams;");
            if (count == 0)
            {
                connection.Execute(@"
                    INSERT INTO Dreams (DreamName, DreamDescription, DreamDate) VALUES
                    (@Name1, @Desc1, @Date1),
                    (@Name2, @Desc2, @Date2),
                    (@Name3, @Desc3, @Date3);
                ", new
                {
                    Name1 = "Flying Over Mountains",
                    Desc1 = "I was soaring over snow-capped mountains with eagles. The wind was warm and I could see rivers winding through valleys below. It felt incredibly peaceful and free.",
                    Date1 = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"),

                    Name2 = "Underwater City",
                    Desc2 = "Discovered a hidden city beneath the ocean. The buildings were made of coral and crystal, and the inhabitants communicated through bioluminescent patterns. I could breathe underwater effortlessly.",
                    Date2 = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd"),

                    Name3 = "Time Travel Library",
                    Desc3 = "Found a library where each book was a portal to a different time period. I opened one and stepped into ancient Rome during a festival. The colors and sounds were vivid and overwhelming.",
                    Date3 = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd")
                });
            }
#endif
        }

        /// <summary>
        /// Drops the Dreams table (available but not called by default).
        /// </summary>
        public static void DisposeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();
            connection.Execute("DROP TABLE IF EXISTS Dreams;");
        }

        /// <summary>
        /// Inserts an audio recording BLOB.
        /// </summary>
        public static async Task SaveRecordingAsync(AudioRecording recording)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                "INSERT INTO AudioRecordings (AudioData) VALUES (@AudioData);",
                new { recording.AudioData });
        }

        /// <summary>
        /// Retrieves an audio recording by ID.
        /// </summary>
        public static async Task<AudioRecording?> GetRecordingAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<AudioRecording>(
                "SELECT Id, AudioData FROM AudioRecordings WHERE Id = @Id;",
                new { Id = id });
        }
    }
}
