using System.Net;

namespace MoneyTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _db;

    public ExpenseRepository(ApplicationDbContext context)
    {
        _db = context;
    }

    public async Task<List<ExpenseDto>> GetExpenseByTimeAsync(long startDate, long endDate, CancellationToken ct)
    {

        var expense = await _db.Set<ExpenseEntity>()
            .Where(e => e.TimeUnix >= startDate && e.TimeUnix < endDate)
            .OrderBy(e => e.TimeUnix)
            .Select(e => new ExpenseDto
            {        
                Id = e.Id,
                CategoryId = e.CategoryId,        
                Time = e.TimeUnix,
                Description = e.Description,
                Sum = e.Sum
            }).ToListAsync(ct);

        return expense;
    }

    public async Task<ExpenseEntity> AddExpenseAsync(ExpenseDto dto, CancellationToken ct)
    {
        var expenseCategory = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, ct);

        if (expenseCategory == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        if (dto.Sum <= 0)
        {
            throw new ResponseException(ErrorType.Validation, $"Invalid '{dto.Sum}' sum");
        }

        var newExpense = new ExpenseEntity
        {
            CategoryId = expenseCategory.Id,
            TimeUnix = dto.Time,
            Sum = dto.Sum,
            Description = dto.Description
        };

        await _db.Set<ExpenseEntity>().AddAsync(newExpense, ct);
        await _db.SaveChangesAsync(ct);

        return newExpense;
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken ct)
    {
        var expense = await _db.Set<ExpenseEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (expense == null)
        {
            throw new ResponseException(ErrorType.NotFound, "Expense not found");
        }

        _db.Set<ExpenseEntity>().Remove(expense);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(Guid id, ExpenseDto dto, CancellationToken ct)
    {
        var expense = await _db.Set<ExpenseEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (expense == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Expense not found");
        }

        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        expense.Sum = dto.Sum;
        expense.Description = dto.Description;
        expense.CategoryId = category.Id;
        expense.TimeUnix = dto.Time;

        var updatedExpenseFromDb = await _db.Set<ExpenseEntity>().FirstAsync(ct);

        var result = new ExpenseDto
        {
            Id = id,
            CategoryId = updatedExpenseFromDb.CategoryId,
            Time = updatedExpenseFromDb.TimeUnix,
            Description = updatedExpenseFromDb.Description,
            Sum = updatedExpenseFromDb.Sum
        };

        await _db.SaveChangesAsync(ct);

        return result;
    }
}