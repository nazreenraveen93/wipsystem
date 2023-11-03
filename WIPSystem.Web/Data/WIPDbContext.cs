using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.Data
{
    public class WIPDbContext:DbContext
    {
        public WIPDbContext(DbContextOptions<WIPDbContext> options) : base(options) 
        { 
            
           
        }

        //this process name is refer to database table
        public DbSet<Process> Process { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductProcessMapping> ProductProcessMappings { get; set; }


    }
}
