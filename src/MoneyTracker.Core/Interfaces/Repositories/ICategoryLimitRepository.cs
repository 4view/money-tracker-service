namespace MoneyTracker.Core.Interfaces.Repositories;

public interface ICategoryLimitRepository
{
    Task<List<CategoryLimitEntity>> GetAllAsync(Guid userId, CancellationToken ct);

    Task<CategoryLimitEntity?> GetByIdAsync(Guid userId, Guid limitId, CancellationToken ct);

    Task<bool> ExistsForCategoryAsync(Guid userId, Guid categoryId, CancellationToken ct);

    Task<CategoryLimitEntity> AddAsync(CategoryLimitEntity limit, CancellationToken ct);

    Task UpdateAsync(CategoryLimitEntity limit, CancellationToken ct);

    Task DeleteAsync(CategoryLimitEntity limit, CancellationToken ct);
}
