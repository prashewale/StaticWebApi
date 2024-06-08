using Microsoft.EntityFrameworkCore;
using Static.Services.Models;
namespace Static.Services.Repository.DbContexts;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public virtual DbSet<Instrument> Instruments { get; set; }
    public virtual DbSet<OptionExpiry> OptionExpiries { get; set; }
    public virtual DbSet<Strike> Strikes { get; set; }
    public virtual DbSet<OptionCandle> OptionCandles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(RepositoryConstant.APPLICATION_DB_CONNECTION_STRING);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instrument>()
            .Property(c => c.Exchange)
            .HasConversion<int>();


        modelBuilder.Entity<OptionCandle>()
            .Property(c => c.OptionType)
            .HasConversion<int>();

        modelBuilder.Entity<OptionCandle>()
            .Property(c => c.Duration)
            .HasConversion<int>();

        base.OnModelCreating(modelBuilder);
    }
}
