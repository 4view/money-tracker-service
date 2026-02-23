namespace MoneyTracker.Core.Interfaces;

public interface ICategoryRepository
{
    public Task<List<CategoryDto>> GetAllCategoryAsync(Guid userId, CancellationToken ct);

    public Task<CategoryEntity> GetCategoryByIdAsync(Guid userId, Guid id, CancellationToken ct);

    public Task<CategoryEntity> AddCategoryAsync(
        Guid userId,
        CategoryDto category,
        CancellationToken ct
    );

    public Task<CategoryEntity> UpdateCategoryAsync(
        Guid userId,
        Guid id,
        CategoryDto dto,
        CancellationToken ct
    );

    public Task DeleteCategoryAsync(Guid userId, Guid id, CancellationToken ct);
}
