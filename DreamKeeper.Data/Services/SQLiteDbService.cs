using Dapper;
using Microsoft.Data.Sqlite;
using Muffle.Data.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Data.Services
{
    public class SQLiteDbService
    {
        private static readonly string _connectionString = ConfigurationLoader.GetConnectionString("SqliteConnection");

        public SQLiteDbService()
        {

        }

        public static IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public static void InitializeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();

            // Create Dreams table
            var createDreamsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Dreams (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DreamName TEXT NOT NULL,
                DreamDescription TEXT,
                DreamDate TEXT NOT NULL,
                DreamRecording BLOB
            );";

            connection.Execute(createDreamsTableQuery);

            // Seed data
            var seedDataQueryDreams =
                @"INSERT INTO Dreams (Id, DreamName, DreamDescription, DreamDate, DreamRecording)
                    VALUES 
                    (0, 'Catching a deer at Christmas on a motorcycle in Detroit.', 'Was initially driving my green Challenger then somehow changed to my motorcycle. It was snowing and the road was slushy. Ended up parking the bike when a baby fawn/doe nearly ran in front of me. I picked her up and we travelled across someones yard. It was Christmas. There was a shivering baby puppy out front, clearly about to be recieved as a gift before I awoke shortly after when I jumped the fence with the fawn.', '2024-05-24', NULL),
                    (1, 'Picking up Dupreeh in the Challenger and robbing a gas station.', 'Was drunk driving Challenger to store, robbed them, got cops to chase me, then awoke.', '2024-05-17', NULL),
                    (2, 'Hanging out with dad watching TV and playing video games.', 'Was in my room in Washington chilling with dad.', '2024-05-22', NULL);";

            try
            {
                connection.Execute(seedDataQueryDreams);
            }
            catch (Exception) { }
        }

        public static void DisposeDatabase()
        {
            using var connection = CreateConnection();
            connection.Open();
            var dropDreamsTable = "DROP TABLE IF EXISTS Dreams";
            connection.Execute(dropDreamsTable);
        }
    }
}
