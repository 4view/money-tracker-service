namespace MoneyTracker.Core.Interfaces.Services;

public interface ILimitService
{
    Task<List<AddedLimitDto>> GetAllAsync(Guid userId, CancellationToken ct);

    Task<ReturnedLimitDto> GetWithCalculationAsync(
        Guid userId,
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    );

    Task<AddedLimitDto> AddAsync(Guid userId, BaseLimitDto dto, CancellationToken ct);

    Task UpdateAsync(Guid userId, Guid limitId, BaseLimitDto dto, CancellationToken ct);

    Task DeleteAsync(Guid userId, Guid limitId, CancellationToken ct);
}
