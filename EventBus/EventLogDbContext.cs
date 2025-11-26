using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventBus;

public class EventDbContext : DbContext
{
    public int Manual { get; set; }
    
    private IDbContextTransaction currentTransaction;
    public IDbContextTransaction GetCurrentTransaction() => currentTransaction;
    public bool HasActiveTransaction => currentTransaction != null;
    public EventDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppEvent> AppEvents { get; set; }
    public DbSet<AppReceivedEvent> AppReceivedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppEvent>(ConfigureAppEventEntry);
        builder.Entity<AppReceivedEvent>(ConfigureAppReceivedEventEntry);
    }

    private void ConfigureAppReceivedEventEntry(EntityTypeBuilder<AppReceivedEvent> builder)
    {
        builder.ToTable("AppReceivedEvents");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EventId)
            .IsRequired()
            .HasMaxLength(33);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.ReceivedOn)
            .IsRequired();

        builder.Property(e => e.State)
            .IsRequired();

        builder.Property(e => e.EventTypeName)
            .IsRequired();    
    }

    void ConfigureAppEventEntry(EntityTypeBuilder<AppEvent> builder)
    {
        builder.ToTable("AppEvents");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.CreationTime)
            .IsRequired();

        builder.Property(e => e.State)
            .IsRequired();

        builder.Property(e => e.TimesSent)
            .IsRequired();

        builder.Property(e => e.EventTypeName)
            .IsRequired();

    }
}
