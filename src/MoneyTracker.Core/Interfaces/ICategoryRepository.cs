namespace MoneyTracker.Core.Interfaces;

public interface ICategoryRepository
{
    public Task<List<CategoryDto>> GetAllCategoryAsync(CancellationToken ct);

    public CategoryEntity GetCategoryByName(Guid id);

    public Task<CategoryEntity> AddCategoryAsync (CategoryDto category, CancellationToken ct);

    public Task<CategoryEntity> UpdateCategoryAsync(Guid id, CategoryDto dto, CancellationToken ct);

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct);
}