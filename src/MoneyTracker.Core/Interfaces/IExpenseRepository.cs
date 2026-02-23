namespace MoneyTracker.Core.Interfaces;

public interface IExpenseRepository
{
    public Task<List<ExpenseDto>> GetExpenseByTimeAsync(
        Guid userId,
        long startDate,
        long endDate,
        CancellationToken ct
    );

    public Task<ExpenseEntity> AddExpenseAsync(
        Guid userid,
        ExpenseDto expense,
        CancellationToken ct
    );

    public Task<ExpenseDto> UpdateExpenseAsync(
        Guid userId,
        Guid id,
        ExpenseDto dto,
        CancellationToken ct
    );

    public Task DeleteExpenseAsync(Guid userId, Guid id, CancellationToken ct);

    Task<CategoryEntity?> GetCategoryByNameAsync(Guid userId, string name, CancellationToken ct);

    Task<CategoryEntity> GetOrCreateDefaultCategoryAsync(Guid userId, CancellationToken ct);

    public Task<decimal> GetSumByCategoryAndPeriodAsync(
        Guid userId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    );
}
