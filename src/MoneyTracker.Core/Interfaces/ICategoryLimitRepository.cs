namespace MoneyTracker.Core.Interfaces;

public interface ICategoryLimitRepository
{
    public Task<ReturnedLimitDto> GetCategoryLimitAsync(
        Guid userId,
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    );

    public Task<List<AddedLimitDto>> GetAllLimitsAsync(Guid userId, CancellationToken ct);

    public Task<AddedLimitDto> AddCategoryLimitAsync(
        Guid userId,
        BaseLimitDto limit,
        CancellationToken ct
    );

    public Task<BaseLimitDto> UpdateCategoryLimitAsync(
        Guid userId,
        Guid limitId,
        BaseLimitDto limit,
        CancellationToken ct
    );

    public Task DeleteCategoryLimitAsync(Guid userId, Guid id, CancellationToken ct);
}
