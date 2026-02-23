namespace MoneyTracker.Data.Repositories;

public class CategoryLimitRepository : ICategoryLimitRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryLimitRepository(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor
    )
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

    public async Task<List<AddedLimitDto>> GetAllLimitsAsync(Guid userId, CancellationToken ct)
    {
        var limitsList = await _db.Set<CategoryLimitEntity>()
            .Where(cl => cl.UserId == userId)
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
        Guid userId,
        Guid limitId,
        Guid categoryId,
        long from,
        long to,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, "Category not found");
        }

        var categoryLimit = await _db.Set<CategoryLimitEntity>()
            .FirstOrDefaultAsync(cl => cl.Id == limitId && cl.UserId == userId, ct);

        if (categoryLimit == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category limit not found");
        }

        var expenseRepo = new ExpenseRepository(_db, _httpContextAccessor);
        var spentAmount = await expenseRepo.GetSumByCategoryAndPeriodAsync(
            categoryId,
            from,
            to,
            ct
        );

        var outputLimit = new ReturnedLimitDto
        {
            Id = limitId,
            CategoryId = categoryId,
            Limit = categoryLimit.Limit,
            Remaining = categoryLimit.Limit - spentAmount,
        };

        return outputLimit;
    }

    public async Task<AddedLimitDto> AddCategoryLimitAsync(
        Guid userId,
        BaseLimitDto dto,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.UserId == userId, ct);

        var existedLimit = await _db.Set<CategoryLimitEntity>()
            .AnyAsync(cl => cl.Category.Id == dto.CategoryId && cl.UserId == userId, ct);

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
            throw new ResponseException(ErrorType.Validation, "Limit must be greater than zero");
        }

        var newLimit = new CategoryLimitEntity
        {
            Id = Guid.NewGuid(),
            Category = category,
            Limit = dto.Limit,
            UserId = userId,
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
        Guid userId,
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
            .FirstOrDefaultAsync(cl => cl.Id == limitId && cl.UserId == userId, ct);

        if (existingLimit == null)
        {
            throw new ResponseException(ErrorType.NotFound, "Category doesn't have a limit");
        }

        existingLimit.Limit = dto.Limit;

        await _db.SaveChangesAsync(ct);

        return new BaseLimitDto
        {
            Id = existingLimit.Id,
            CategoryId = existingLimit.Category.Id,
            Limit = existingLimit.Limit,
        };
    }

    public async Task DeleteCategoryLimitAsync(Guid userId, Guid limitId, CancellationToken ct)
    {
        var categoryLimit = await _db.Set<CategoryLimitEntity>()
            .FirstOrDefaultAsync(cl => cl.Id == limitId && cl.UserId == userId, ct);

        if (categoryLimit == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Limit not found");
        }

        _db.Set<CategoryLimitEntity>().Remove(categoryLimit);
        await _db.SaveChangesAsync();
    }
}
