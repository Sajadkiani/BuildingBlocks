using AppDomain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventBus;

public class EventDbContext : DbContext
{
    private readonly IMediator _mediator;
    public int Manual { get; set; }
    
    private IDbContextTransaction currentTransaction;
    public IDbContextTransaction GetCurrentTransaction() => currentTransaction;
    public bool HasActiveTransaction => currentTransaction != null;
    public EventDbContext(
        DbContextOptions options,
        IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<AppEvent> AppEvents { get; set; }
    public DbSet<AppReceivedEvent> AppReceivedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<AppEvent>(ConfigureAppEventEntry);
        builder.Entity<AppReceivedEvent>(ConfigureAppReceivedEventEntry);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
            .ToList();
        
        if (!entries.Any())
            return 0;
        
        var events = entries.Where(item => item.Entity is Entity)
            .Select(item => (Entity)item.Entity)
            .SelectMany(item => item.DomainEvents)
            .ToList();
        
        foreach (var @event in events)
        {
            await _mediator.Publish(@event, cancellationToken);
        }
        
        return await base.SaveChangesAsync(cancellationToken);
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
