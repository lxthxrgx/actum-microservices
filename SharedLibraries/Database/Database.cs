using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SharedLibraries.model;

namespace SharedLibraries.Database
{
    public class DatabaseModel : DbContext
    {
        public DbSet<CounterpartyModel> Counterparties { get; set; }
        public DbSet<GroupModel> Groups { get; set; }
        public DbSet<SubleaseModel> Subleases { get; set; }
        public DbSet<GuardModel> Guards { get; set; }

        public DatabaseModel(DbContextOptions<DatabaseModel> options) : base(options)
        {
        }
        public DatabaseModel() : base()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupModel>()
                .HasMany(e => e.Counterparty)
                .WithOne(e => e.GroupTable)
                .HasForeignKey(e => e.GroupId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupModel>()
                .HasMany(e => e.Guard)
                .WithOne(e => e.GroupTable)
                .HasForeignKey(e => e.GroupId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupModel>()
                .HasMany(e => e.Sublease)
                .WithOne(e => e.GroupTable)
                .HasForeignKey(e => e.GroupId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class DatabaseModelFactory : IDesignTimeDbContextFactory<DatabaseModel>
    {
        public DatabaseModel CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseModel>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);

            return new DatabaseModel(optionsBuilder.Options);
        }
    }
}
