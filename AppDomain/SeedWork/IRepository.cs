namespace AppDomain.SeedWork;

public interface IRepository<TEntity, TId> where TEntity : IAggregateRoot
{
    Task<TEntity?> GetByIdAsync(TId id);
    IUnitOfWork UnitOfWork { get; }
}
