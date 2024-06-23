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
            try
            {
                using var connection = SQLiteDbService.CreateConnection();
                connection.Open();

                // add dream
                var insertDreamSql = @"
                        INSERT INTO Dreams (DreamName, DreamDescription, DreamDate, DreamRecording)
                        VALUES (@DreamName, @DreamDescription, @DreamDate, @DreamRecording);
                        SELECT last_insert_rowid();";
                // Insert dream into the database
                int dreamId = connection.ExecuteScalar<int>(insertDreamSql, newDream);

                newDream.Id = dreamId;

                //GetDreams();
                return newDream;
            }
            catch (Exception Ex)
            {
                newDream.Id = -1;
                newDream.DreamDescription = Ex.Message;

            }

            return newDream; 
            
        }

        public void DeleteDream(int dreamId)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            connection.Execute("DELETE FROM Dreams WHERE Id = @Id", new { Id = dreamId });
        }

        public void UpdateDream(Dream dream)
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            connection.Execute("UPDATE Dreams SET DreamName = @DreamName, DreamDescription = @DreamDescription, DreamDate = @DreamDate, DreamRecording = @DreamRecording WHERE Id = @Id", dream);
        }

    }
}
