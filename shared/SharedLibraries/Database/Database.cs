using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SharedLibraries.model;

namespace SharedLibraries.Database
{
    public class DatabaseModel : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<Company> Company { get; set; }

        public DbSet<CounterpartyModel> Counterparties { get; set; }

        public DbSet<GroupModel> Groups { get; set; }
        public DbSet<GroupFiles> GroupsFiles { get; set; }

        public DbSet<SubleaseModel> Subleases { get; set; }
        public DbSet<SubleaseNotes> SubleasesNotes { get; set; }
        public DbSet<SubleaseFiles> SubleasesFiles { get; set; }

        public DbSet<GuardModel> Guards { get; set; }
        public DbSet<GuardNotes> GuardsNotes { get; set; }
        public DbSet<GuardFiles> GuardsFiles { get; set; }

        public DatabaseModel(DbContextOptions<DatabaseModel> options) : base(options)
        {
        }
        public DatabaseModel() : base()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasMany(e => e.CompanyUsers)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);
       
            modelBuilder.Entity<Company>()
                .HasMany(e => e.CompanyUsers)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Counterparties)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(e => e.Groups)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CounterpartyModel>()
               .HasMany(e => e.Groups)
               .WithOne(e => e.Counterparty)
               .HasForeignKey(e => e.CounterpartyId)
               .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupModel>()
               .HasMany(e => e.Sublease)
               .WithOne(e => e.Group)
               .HasForeignKey(e => e.GroupId)
               .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupModel>()
              .HasMany(e => e.Guard)
              .WithOne(e => e.Group)
              .HasForeignKey(e => e.GroupId)
              .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubleaseModel>()
              .HasMany(e => e.SubleaseNotes)
              .WithOne(e => e.Sublease)
              .HasForeignKey(e => e.SubleaseId)
              .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubleaseModel>()
              .HasMany(e => e.SubleaseFiles)
              .WithOne(e => e.Sublease)
              .HasForeignKey(e => e.SubleaseId)
              .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuardModel>()
              .HasMany(e => e.GuardNotes)
              .WithOne(e => e.Guard)
              .HasForeignKey(e => e.GuardId)
              .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GuardModel>()
              .HasMany(e => e.GuardFiles)
              .WithOne(e => e.Guard)
              .HasForeignKey(e => e.GuardId)
              .IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<CompanyUser>()
                .HasIndex(cu => new { cu.UserId, cu.CompanyId })
                .IsUnique();

            modelBuilder.Entity<CompanyUser>()
                .HasIndex(cu => new { cu.CompanyId, cu.IsActive });

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
