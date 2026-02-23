namespace MoneyTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryRepository(
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

    public async Task<List<CategoryDto>> GetAllCategoryAsync(Guid userId, CancellationToken ct)
    {
        var categoryList = await _db.Set<CategoryEntity>()
            .Where(c => c.UserId == userId)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync(ct);

        return categoryList;
    }

    public async Task<CategoryEntity> GetCategoryByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Категория не найдена");
        }

        return category;
    }

    public async Task<CategoryEntity> AddCategoryAsync(
        Guid userId,
        CategoryDto category,
        CancellationToken ct
    )
    {
        if (category.Name == null || string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ResponseException(ErrorType.Validation, $"Category name must be specified");
        }

        if (
            await _db.Set<CategoryEntity>()
                .AnyAsync(
                    c =>
                        c.UserId == userId
                        && c.Name.Trim().ToLower() == category.Name.Trim().ToLower(),
                    ct
                )
        )
        {
            throw new ResponseException(
                ErrorType.Conflict,
                $"Category '{category.Name}' already exist"
            );
        }

        var newCategory = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = category.Name,
            UserId = userId,
        };

        await _db.Set<CategoryEntity>().AddAsync(newCategory, ct);
        await _db.SaveChangesAsync(ct);

        return newCategory;
    }

    public async Task<CategoryEntity> UpdateCategoryAsync(
        Guid userId,
        Guid id,
        CategoryDto dto,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        if (
            await _db.Set<CategoryEntity>()
                .AnyAsync(
                    c =>
                        c.UserId == userId
                        && c.Id != id
                        && c.Name.Trim().ToLower() == dto.Name.Trim().ToLower(),
                    ct
                )
        )
        {
            throw new ResponseException(ErrorType.Conflict, $"Category '{dto.Name}' already exist");
        }

        category.Name = dto.Name;

        await _db.SaveChangesAsync(ct);

        return category;
    }

    public async Task DeleteCategoryAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var category = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        bool hasRelatedExpenses = await _db.Set<ExpenseEntity>()
            .AnyAsync(e => e.CategoryId == category.Id && e.UserId == userId, ct);

        if (hasRelatedExpenses)
        {
            int expenseCount = await _db.Set<ExpenseEntity>()
                .CountAsync(e => e.CategoryId == category.Id && e.UserId == userId, ct);

            throw new ResponseException(
                ErrorType.Validation,
                $"Cannot remove category '{category.Name}', it is used in {expenseCount} expenses"
            );
        }

        _db.Set<CategoryEntity>().Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
