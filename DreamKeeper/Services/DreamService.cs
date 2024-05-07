using DreamKeeper.Data;
using DreamKeeper.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Services
{
    public class DreamService
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public DreamService(DbContextOptions<AppDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public List<Dream> GetDreams()
        {

            try
            {
                using (var dbContext = new AppDbContext(_dbContextOptions))
                {

                    // Retrieve dreams
                    List<Dream> dreams = dbContext.Dreams.ToList();

                    if (dreams.Count == 0)
                    {
                        // Add a default dream
                        var newDream = new Dream
                        {
                            Id = 1,
                            DreamName = "Sample Item",
                            DreamDescription = "This is a sample item",
                            DreamDate = DateTime.Now,
                            DreamRecording = null
                        };
                        dbContext.Dreams.Add(newDream);
                        dbContext.SaveChanges();
                        dreams = dbContext.Dreams.ToList();
                    }

                    return dreams;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving dreams: {ex.Message}");
                throw;
            }
            
        }

    }
}
