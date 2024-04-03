using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WIPSystem.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WIPSystem.Web.Data
{
    public class WIPDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly IConfiguration _configuration;

        public WIPDbContext(DbContextOptions<WIPDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        // DbSet properties for your application
        public DbSet<Process> Process { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductProcessMapping> ProductProcessMappings { get; set; }
        public DbSet<LotTraveller> LotTravellers { get; set; }
        public DbSet<SplitLot> SplitLot { get; set; }
        public DbSet<SplitDetail> SplitDetails { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Jig> Jigs { get; set; }
        public DbSet<IncomingProcess> IncomingProcesses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserEntity> UserEntities { get; set; }
        public DbSet<CurrentStatus> CurrentStatus { get; set; }
       


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    // You can specify other logging providers as well
                });

                optionsBuilder.UseSqlServer(connectionString)
                    .UseLoggerFactory(loggerFactory)
                    .LogTo(Console.WriteLine);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Call the base method here
                                           // Configure the relationship between CurrentStatus and Product
                                           // Convert SinterProcessStatus enum to string and vice versa for the Status property in Sinter1000
            //builder.Entity<ThousandSinter>()
            //       .Property(s => s.Status)
            //       .HasConversion(
            //           v => v.ToString(),
            //           v => (SinterProcessStatus)Enum.Parse(typeof(SinterProcessStatus), v));

            builder.Entity<CurrentStatus>()
                .HasOne(cs => cs.Product)
                .WithMany() // If Product has a collection of CurrentStatus, replace WithMany() with WithMany(p => p.CurrentStatuses)
                .HasForeignKey(cs => cs.ProductId); // Configure the foreign key
        }
    }
}
