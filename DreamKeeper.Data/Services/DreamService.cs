using System.Collections.ObjectModel;
using Dapper;
using DreamKeeper.Data.Models;

namespace DreamKeeper.Data.Services
{
    /// <summary>
    /// Business logic layer for Dream CRUD operations using Dapper over SQLite.
    /// </summary>
    public class DreamService
    {
        /// <summary>
        /// Returns ObservableCollection of all dreams, sorted by date descending.
        /// </summary>
        public ObservableCollection<Dream> GetDreams()
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            var dreams = connection.Query<Dream>(
                "SELECT Id, DreamName, DreamDescription, DreamDate, DreamRecording FROM Dreams ORDER BY DreamDate DESC;"
            ).ToList();

            // Reset dirty flags since these are fresh from DB
            foreach (var dream in dreams)
            {
                dream.HasUnsavedChanges = false;
            }

            return new ObservableCollection<Dream>(dreams);
        }

        /// <summary>
        /// Inserts a new dream (resets Id to 0, calls UpsertDream).
        /// </summary>
        public Dream AddDream(Dream dream)
        {
            dream.Id = 0;
            return UpsertDream(dream);
        }

        /// <summary>
        /// Updates an existing dream (calls UpsertDream).
        /// </summary>
        public Dream UpdateDream(Dream dream)
        {
            return UpsertDream(dream);
        }

        /// <summary>
        /// Deletes a dream by primary key.
        /// </summary>
        public void DeleteDream(int dreamId)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();
            connection.Execute("DELETE FROM Dreams WHERE Id = @Id;", new { Id = dreamId });
        }

        /// <summary>
        /// INSERT or UPDATE based on Id; returns dream with updated ID.
        /// </summary>
        public Dream UpsertDream(Dream dream)
        {
            try
            {
                using var connection = SQLiteDbService.CreateConnection();
                connection.Open();

                if (dream.Id <= 0)
                {
                    // INSERT
                    var id = connection.ExecuteScalar<int>(@"
                        INSERT INTO Dreams (DreamName, DreamDescription, DreamDate, DreamRecording)
                        VALUES (@DreamName, @DreamDescription, @DreamDate, @DreamRecording);
                        SELECT last_insert_rowid();
                    ", new
                    {
                        dream.DreamName,
                        dream.DreamDescription,
                        DreamDate = dream.DreamDate.ToString("yyyy-MM-dd"),
                        dream.DreamRecording
                    });

                    dream.Id = id;
                }
                else
                {
                    // UPDATE
                    connection.Execute(@"
                        UPDATE Dreams 
                        SET DreamName = @DreamName,
                            DreamDescription = @DreamDescription,
                            DreamDate = @DreamDate,
                            DreamRecording = @DreamRecording
                        WHERE Id = @Id;
                    ", new
                    {
                        dream.Id,
                        dream.DreamName,
                        dream.DreamDescription,
                        DreamDate = dream.DreamDate.ToString("yyyy-MM-dd"),
                        dream.DreamRecording
                    });
                }

                dream.MarkAsSaved();
            }
            catch (Exception ex)
            {
                dream.Id = -1;
                dream.DreamDescription = ex.Message;
            }

            return dream;
        }
    }
}
