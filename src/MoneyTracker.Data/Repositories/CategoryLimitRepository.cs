namespace MoneyTracker.Data.Repositories;

public class CategoryLimitRepository : ICategoryLimitRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryLimitRepository(ApplicationDbContext _context)
    {
        _db = _context;
    }

    public async Task<List<AddedLimitDto>> GetAllLimitsAsync(CancellationToken ct)
    {
        var limitsList = await _db.Set<CategoryLimitEntity>()
            .Select(cl => new AddedLimitDto
            {
                Id = cl.Id,
                CategoryId = cl.Category.Id,
                Limit = cl.Limit,
            })
            .ToListAsync(ct);

        return limitsList;
    }

    public async Task<ReturnedLimitDto> GetCategoryLimitAsync(
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, "Category not found");
        }

        var categoryLimit = await _db.Set<CategoryLimitEntity>()
            .FirstOrDefaultAsync(cl => cl.Id == limitId, ct);

        if (categoryLimit == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category limit not found");
        }

        int year = DateTime.UtcNow.Year;
        int month = DateTime.UtcNow.Month;

        var spentAmount = await _db.Set<ExpenseEntity>()
            .Where(e => e.Category.Id == categoryId)
            .Where(e => e.TimeUnix >= from && e.TimeUnix < to)
            .SumAsync(e => e.Sum, ct);

        var outputLimit = new ReturnedLimitDto
        {
            Id = limitId,
            CategoryId = categoryId,
            Limit = categoryLimit.Limit,
            Remaining = categoryLimit.Limit - spentAmount,
        };

        return outputLimit;
    }

    public async Task<AddedLimitDto> AddCategoryLimitAsync(BaseLimitDto dto, CancellationToken ct)
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, ct);

        var existedLimit = await _db.Set<CategoryLimitEntity>()
            .AnyAsync(cl => cl.Category.Id == dto.CategoryId);

        if (existedLimit)
        {
            throw new ResponseException(ErrorType.Conflict, "Category already has limit");
        }

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found.");
        }

        if (dto.Limit <= 0)
        {
            throw new ResponseException(ErrorType.Validation, "Limit must be greater then zero");
        }

        var newLimit = new CategoryLimitEntity
        {
            Id = Guid.NewGuid(),
            Category = category,
            Limit = dto.Limit,
        };

        _db.Set<CategoryLimitEntity>().Add(newLimit);
        await _db.SaveChangesAsync();

        return new AddedLimitDto
        {
            Id = newLimit.Id,
            CategoryId = category.Id,
            Limit = dto.Limit,
        };
    }

    public async Task<BaseLimitDto> UpdateCategoryLimitAsync(
        Guid limitId,
        BaseLimitDto dto,
        CancellationToken ct
    )
    {
        if (dto.Limit <= 0 || dto.Limit > decimal.MaxValue)
        {
            throw new ResponseException(ErrorType.Validation, $"Incorrect limit: {dto.Limit}");
        }

        var existingLimit = await _db.Set<CategoryLimitEntity>()
            .FirstOrDefaultAsync(cl => cl.Id == limitId, ct);

        if (existingLimit == null)
        {
            throw new ResponseException(ErrorType.Conflict, "Category don`t have a limit");
        }

        existingLimit.Limit = dto.Limit;

        await _db.SaveChangesAsync(ct);

        var updatedLimit = new BaseLimitDto
        {
            Id = existingLimit.Id,
            CategoryId = existingLimit.Category.Id,
            Limit = existingLimit.Limit,
        };

        return updatedLimit;
    }

    public async Task DeleteCategoryLimitAsync(Guid limitId, CancellationToken ct)
    {
        var categoryLimit = await _db.Set<CategoryLimitEntity>()
            .FirstOrDefaultAsync(cl => cl.Id == limitId);

        if (categoryLimit == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Limit not found");
        }

        _db.Set<CategoryLimitEntity>().Remove(categoryLimit);
        await _db.SaveChangesAsync();
    }
}