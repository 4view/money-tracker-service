namespace MoneyTracker.Core.Interfaces.Services;

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetByPeriodAsync(Guid userId, long from, long to, CancellationToken ct);

    Task<ExpenseDto> AddAsync(Guid userId, ExpenseDto dto, CancellationToken ct);

    Task<ExpenseDto> AddFromQrAsync(Guid userId, ExpenseQrDto dto, CancellationToken ct);

    Task<ExpenseDto> UpdateAsync(Guid userId, Guid id, ExpenseDto dto, CancellationToken ct);

    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct);
}
