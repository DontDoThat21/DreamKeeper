using Dapper;
using DreamKeeper.Data.Services;
using DreamKeeper.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Services
{
    public class DreamService
    {
        //private readonly DbContextOptions<DreamsAppDbContext> _dbContextOptions;

        //public DreamService(DbContextOptions<DreamsAppDbContext> dbContextOptions)
        //{
        //    _dbContextOptions = dbContextOptions;
        //}

        public async Task<ObservableCollection<Dream>> GetDreams()
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            // get dreams
            var selectDreamsSql = "Select * FROM Dreams;";
            var dreamsList = await connection.QueryAsync<Dream>(selectDreamsSql);
            var Dreams = new ObservableCollection<Dream>(dreamsList);
            return Dreams;            
        }

        public Dream AddDream(Dream newDream)
        {
            newDream.Id = 0;
            return UpsertDream(newDream).Result;
        }

        public async Task DeleteDream(int dreamId)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            await connection.ExecuteAsync("DELETE FROM Dreams WHERE Id = @Id", new { Id = dreamId });
        }

        public void UpdateDream(Dream dream)
        {
            UpsertDream(dream).Wait();
        }

        public async Task<Dream> UpsertDream(Dream dream)
        {
            try
            {
                using var connection = SQLiteDbService.CreateConnection();
                connection.Open();

                if (dream.Id <= 0)
                {
                    var insertSql = @"
                        INSERT INTO Dreams (DreamName, DreamDescription, DreamDate, DreamRecording)
                        VALUES (@DreamName, @DreamDescription, @DreamDate, @DreamRecording);
                        SELECT last_insert_rowid();";
                    
                    int dreamId = await connection.ExecuteScalarAsync<int>(insertSql, dream);
                    dream.Id = dreamId;
                }
                else
                {
                    var updateSql = @"
                        UPDATE Dreams 
                        SET DreamName = @DreamName, DreamDescription = @DreamDescription, DreamDate = @DreamDate, DreamRecording = @DreamRecording 
                        WHERE Id = @Id";
                    
                    await connection.ExecuteAsync(updateSql, dream);
                }

                return dream;
            }
            catch (Exception ex)
            {
                dream.Id = -1;
                dream.DreamDescription = ex.Message;
                return dream;
            }
        }

    }
}
