namespace CedarDelivery.Repositories.Interfaces
{
    // Basic CRUD operations shared by every entity with an int Id.
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        // Saves changes made to entities that were loaded through this repository.
        // EF tracks loaded entities, so after editing them only a save is needed.
        Task SaveAsync();
    }
}
