namespace MoneyTracker.Core.Interfaces;

public interface IExpenseRepository
{
    public Task<List<ExpenseDto>> GetExpenseByTimeAsync(long startDate, long endDate, CancellationToken ct);

    public Task<ExpenseEntity> AddExpenseAsync(ExpenseDto expense, CancellationToken ct);

    public Task<ExpenseDto> UpdateExpenseAsync(Guid id, ExpenseDto dto, CancellationToken ct); 

    public Task DeleteExpenseAsync(Guid id, CancellationToken ct);
}