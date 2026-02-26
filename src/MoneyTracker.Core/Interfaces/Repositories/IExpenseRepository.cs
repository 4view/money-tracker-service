namespace MoneyTracker.Core.Interfaces.Repositories;

public interface IExpenseRepository
{
    Task<List<ExpenseEntity>> GetByPeriodAsync(
        Guid userId,
        long from,
        long to,
        CancellationToken ct
    );

    Task<ExpenseEntity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct);

    Task<decimal> GetSumByCategoryAndPeriodAsync(
        Guid userId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    );

    Task<ExpenseEntity> AddAsync(ExpenseEntity expense, CancellationToken ct);

    Task UpdateAsync(ExpenseEntity expense, CancellationToken ct);

    Task DeleteAsync(ExpenseEntity expense, CancellationToken ct);
}
