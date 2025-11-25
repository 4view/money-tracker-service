namespace MoneyTracker.Core.Interfaces;

public interface ICategoryLimitRepository
{
    public Task<ReturnedLimitDto> GetCategoryLimitAsync(
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    );

    public Task<List<AddedLimitDto>> GetAllLimitsAsync(CancellationToken ct);

    public Task<AddedLimitDto> AddCategoryLimitAsync(BaseLimitDto limit, CancellationToken ct);

    public Task<BaseLimitDto> UpdateCategoryLimitAsync(
        Guid limitId,
        BaseLimitDto limit,
        CancellationToken ct
    );

    public Task DeleteCategoryLimitAsync(Guid id, CancellationToken ct);
}
