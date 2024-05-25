using Dapper;
using DreamKeeper.Data.Services;
using DreamKeeper.Models;
using System;
using System.Collections.Generic;
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

        public List<Dream> GetDreams()
        {
            using var connection = SQLiteDbService.CreateConnection();
            connection.Open();

            // get dreams
            var selectDreamsSql = "Select * FROM Dreams;";
            var dreams = connection.Query<Dream>(selectDreamsSql).ToList();
            return dreams;            
        }

        public Dream AddDream(Dream newDream)
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

            GetDreams();
            return newDream;
            
        }

    }
}
