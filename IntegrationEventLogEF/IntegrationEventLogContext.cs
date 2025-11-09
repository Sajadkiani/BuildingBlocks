namespace IntegrationEventLogEF;

public class EventLogDbContext : DbContext
{
    public int Manual { get; set; }
    
    private IDbContextTransaction currentTransaction;
    public IDbContextTransaction GetCurrentTransaction() => currentTransaction;
    public bool HasActiveTransaction => currentTransaction != null;
    public EventLogDbContext(DbContextOptions<EventLogDbContext> options) : base(options)
    {
    }

    public DbSet<AppEventLog> EventLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppEventLog>(ConfigureIntegrationEventLogEntry);
    }

    void ConfigureIntegrationEventLogEntry(EntityTypeBuilder<AppEventLog> builder)
    {
        builder.ToTable("EventLogs");

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
