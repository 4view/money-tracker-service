namespace MoneyTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExpenseRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _db = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(
            ClaimTypes.NameIdentifier
        );
        if (userIdClaim == null)
            throw new ResponseException(ErrorType.Validation, "Пользователь не авторизован");

        return Guid.Parse(userIdClaim.Value);
    }

    public async Task<List<ExpenseDto>> GetExpenseByTimeAsync(
        Guid userId,
        long startDate,
        long endDate,
        CancellationToken ct
    )
    {
        var expenses = await _db.Set<ExpenseEntity>()
            .Where(e => e.UserId == userId)
            .Where(e => e.TimeUnix >= startDate && e.TimeUnix < endDate)
            .OrderBy(e => e.TimeUnix)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                Time = e.TimeUnix,
                Description = e.Description,
                Sum = e.Sum,
            })
            .ToListAsync(ct);

        return expenses;
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
            .Where(e => e.UserId == userId)
            .Where(e => e.CategoryId == categoryId)
            .Where(e => e.TimeUnix >= from && e.TimeUnix < to)
            .SumAsync(e => e.Sum, ct);
    }

    public async Task<ExpenseEntity> AddExpenseAsync(
        Guid userId,
        ExpenseDto dto,
        CancellationToken ct
    )
    {
        var expenseCategory = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId, ct);

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
            Id = Guid.NewGuid(),
            CategoryId = expenseCategory.Id,
            Category = expenseCategory,
            TimeUnix = dto.Time,
            Sum = dto.Sum,
            Description = dto.Description,
            UserId = userId,
        };

        await _db.Set<ExpenseEntity>().AddAsync(newExpense, ct);
        await _db.SaveChangesAsync(ct);

        return newExpense;
    }

    public async Task DeleteExpenseAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var expense = await _db.Set<ExpenseEntity>()
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);

        if (expense == null)
        {
            throw new ResponseException(ErrorType.NotFound, "Expense not found");
        }

        _db.Set<ExpenseEntity>().Remove(expense);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(
        Guid userId,
        Guid id,
        ExpenseDto dto,
        CancellationToken ct
    )
    {
        var expense = await _db.Set<ExpenseEntity>()
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);

        if (expense == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Expense not found");
        }

        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        expense.Sum = dto.Sum;
        expense.Description = dto.Description;
        expense.CategoryId = category.Id;
        expense.Category = category;
        expense.TimeUnix = dto.Time;

        await _db.SaveChangesAsync(ct);

        return new ExpenseDto
        {
            Id = id,
            CategoryId = expense.CategoryId,
            Time = expense.TimeUnix,
            Description = expense.Description,
            Sum = expense.Sum,
        };
    }

    public async Task<CategoryEntity?> GetCategoryByNameAsync(
        Guid userId,
        string name,
        CancellationToken ct
    )
    {
        return await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<CategoryEntity> GetOrCreateDefaultCategoryAsync(
        Guid userId,
        CancellationToken ct
    )
    {
        const string defaultCategoryName = "Другое";

        var defaultCategory = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == defaultCategoryName, ct);

        if (defaultCategory == null)
        {
            defaultCategory = new CategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = defaultCategoryName,
                UserId = userId,
            };

            await _db.Set<CategoryEntity>().AddAsync(defaultCategory, ct);
            await _db.SaveChangesAsync(ct);
        }

        return defaultCategory;
    }

    public async Task<decimal> GetSumByCategoryAndPeriodAsync(
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        var userId = GetCurrentUserId();

        return await _db.Set<ExpenseEntity>()
            .Where(e => e.UserId == userId)
            .Where(e => e.CategoryId == categoryId)
            .Where(e => e.TimeUnix >= from && e.TimeUnix < to)
            .SumAsync(e => e.Sum, ct);
    }
}
