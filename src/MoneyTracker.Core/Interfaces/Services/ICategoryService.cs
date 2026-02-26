namespace MoneyTracker.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(Guid userId, CancellationToken ct);

    Task<CategoryDto> GetByIdAsync(Guid userId, Guid id, CancellationToken ct);

    Task<CategoryDto> AddAsync(Guid userId, CategoryDto dto, CancellationToken ct);

    Task<CategoryDto> UpdateAsync(Guid userId, Guid id, CategoryDto dto, CancellationToken ct);

    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct);
}
