using HangfireWatermarker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HangfireWatermarker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<JobItem> JobItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
