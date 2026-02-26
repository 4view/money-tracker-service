namespace MoneyTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _db;

    public ExpenseRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExpenseEntity>> GetByPeriodAsync(
        Guid userId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        return await _db.Set<ExpenseEntity>()
            .Where(e => e.UserId == userId && e.TimeUnix >= from && e.TimeUnix < to)
            .OrderBy(e => e.TimeUnix)
            .ToListAsync(ct);
    }

    public async Task<ExpenseEntity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        return await _db.Set<ExpenseEntity>()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);
    }

    public async Task<decimal> GetSumByCategoryAndPeriodAsync(
        Guid userId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        return await _db.Set<ExpenseEntity>()
            .Where(e =>
                e.UserId == userId
                && e.CategoryId == categoryId
                && e.TimeUnix >= from
                && e.TimeUnix < to
            )
            .SumAsync(e => e.Sum, ct);
    }

    public async Task<ExpenseEntity> AddAsync(ExpenseEntity expense, CancellationToken ct)
    {
        await _db.Set<ExpenseEntity>().AddAsync(expense, ct);
        await _db.SaveChangesAsync(ct);
        return expense;
    }

    public async Task UpdateAsync(ExpenseEntity expense, CancellationToken ct)
    {
        _db.Set<ExpenseEntity>().Update(expense);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ExpenseEntity expense, CancellationToken ct)
    {
        _db.Set<ExpenseEntity>().Remove(expense);
        await _db.SaveChangesAsync(ct);
    }
}
