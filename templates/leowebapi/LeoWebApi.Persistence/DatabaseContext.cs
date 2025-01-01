using LeoWebApi.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeoWebApi.Persistence;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "LeoWebApi";

    public DbSet<Launch> Launches { get; set; }
    public DbSet<Payload> Payloads { get; set; }
    public DbSet<Rocket> Rockets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureRocket(modelBuilder);
        ConfigurePayload(modelBuilder);
        ConfigureLaunch(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }

    private static void ConfigureLaunch(ModelBuilder modelBuilder)
    {
        var launch = modelBuilder.Entity<Launch>();
        launch.HasKey(l => l.Id);
        // will give us nicely ordered v7 GUIDs
        launch.Property(l => l.Id).ValueGeneratedOnAdd();
        launch.HasOne(l => l.Rocket)
              .WithOne(r => r.Launch)
              .HasForeignKey<Launch>(l => l.RocketId)
              .OnDelete(DeleteBehavior.Cascade);
        launch.HasIndex(l => l.LaunchDate);
        launch.HasIndex(l => l.RocketId).IsUnique();
    }

    private static void ConfigurePayload(ModelBuilder modelBuilder)
    {
        var payload = modelBuilder.Entity<Payload>();
        payload.HasKey(p => p.Id);
        payload.Property(p => p.Id).ValueGeneratedOnAdd();
        payload.Property(p => p.Destination).HasConversion(new EnumToStringConverter<PayloadDestination>());
        payload.HasOne(p => p.Launch)
               .WithMany(l => l.Payloads)
               .HasForeignKey(p => p.LaunchId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureRocket(ModelBuilder modelBuilder)
    {
        var rocket = modelBuilder.Entity<Rocket>();
        rocket.HasKey(r => r.Id);
        rocket.Property(r => r.Id).ValueGeneratedOnAdd();
    }
}
