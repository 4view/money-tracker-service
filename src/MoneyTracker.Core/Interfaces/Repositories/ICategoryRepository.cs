namespace MoneyTracker.Core.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryEntity>> GetAllAsync(Guid userId, CancellationToken ct);

    Task<CategoryEntity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct);

    Task<CategoryEntity?> GetByNameAsync(Guid userId, string name, CancellationToken ct);

    Task<bool> ExistsWithNameAsync(Guid userId, string name, Guid? excludeId, CancellationToken ct);

    Task<bool> HasExpensesAsync(Guid userId, Guid categoryId, CancellationToken ct);

    Task<int> CountExpensesAsync(Guid userId, Guid categoryId, CancellationToken ct);

    Task<CategoryEntity> AddAsync(CategoryEntity category, CancellationToken ct);

    Task UpdateAsync(CategoryEntity category, CancellationToken ct);

    Task DeleteAsync(CategoryEntity category, CancellationToken ct);
}
