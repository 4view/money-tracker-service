namespace MoneyTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext context)
    {
        _db = context;
    }

    public async Task<List<CategoryDto>> GetAllCategoryAsync(CancellationToken ct)
    {
        var categoryList = await _db.Set<CategoryEntity>()
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync(ct);

        return categoryList;
    }

    public CategoryEntity GetCategoryByName(Guid id)
    {
        var category = _db.Set<CategoryEntity>().FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        return category;
    }

    public async Task<CategoryEntity> AddCategoryAsync(CategoryDto category, CancellationToken ct)
    {
        if (category.Name == null || string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ResponseException(ErrorType.Validation, $"Category name must be specefied");
        }

        if (
            await _db.Set<CategoryEntity>()
                .AnyAsync(c => c.Name.Trim().ToLower() == category.Name.Trim().ToLower(), ct)
        )
        {
            throw new ResponseException(
                ErrorType.Conflict,
                $"Category '{category.Name}' already exist"
            );
        }

        var newCategory = new CategoryEntity { Name = category.Name };

        await _db.Set<CategoryEntity>().AddAsync(newCategory, ct);
        await _db.SaveChangesAsync(ct);

        return newCategory;
    }

    public async Task<CategoryEntity> UpdateCategoryAsync(
        Guid id,
        CategoryDto dto,
        CancellationToken ct
    )
    {
        var category = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        if (
            await _db.Set<CategoryEntity>()
                .AnyAsync(c => c.Name.Trim().ToLower() == dto.Name.Trim().ToLower(), ct)
        )
        {
            throw new ResponseException(ErrorType.Conflict, $"Category '{dto.Name}' already exist");
        }

        category.Name = dto.Name;

        await _db.SaveChangesAsync(ct);

        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct)
    {
        var category = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category == null)
        {
            throw new ResponseException(ErrorType.NotFound, $"Category not found");
        }

        bool hasRelatedExpenses = await _db.Set<ExpenseEntity>()
            .AnyAsync(e => e.CategoryId == category.Id, ct);

        if (hasRelatedExpenses)
        {
            int expenseCount = await _db.Set<ExpenseEntity>()
                .CountAsync(e => e.CategoryId == category.Id, ct);

            throw new ResponseException(
                ErrorType.Validation,
                $"Cannot remove category '{category.Name}', it is used in {expenseCount} expenses"
            );
        }

        _db.Set<CategoryEntity>().Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
