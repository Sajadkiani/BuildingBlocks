using AppDomain.SeedWork;
using EventBus.Services;
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

    protected async Task<int> SaveChangesAndSendEventsAsync(IDbContextTransaction contextTransaction,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await Database.UseTransactionAsync(contextTransaction.GetDbTransaction(), cancellationToken);
        return await SaveChangesAsync(cancellationToken);
    }
    
    private async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => (x.State == EntityState.Modified || x.State == EntityState.Added)
                        && x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        //domain events
        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();
        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        //save integrated events in outbox
        var integrationEvents = domainEntities
            .SelectMany(item => item.IntegratedEvents)
            .ToList();
        
        foreach (var integrationEvent in integrationEvents)
        {
            var appEvent = new AppEvent(integrationEvent, currentTransaction.TransactionId, integrationEvent.GetType());
            await AppEvents.AddAsync(appEvent, cancellationToken);
        }
        
        var countOfChanges = await base.SaveChangesAsync(cancellationToken);
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return countOfChanges;
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
