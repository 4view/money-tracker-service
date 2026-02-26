namespace MoneyTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryEntity>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Set<CategoryEntity>().Where(c => c.UserId == userId).ToListAsync(ct);
    }

    public async Task<CategoryEntity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        return await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
    }

    public async Task<CategoryEntity?> GetByNameAsync(
        Guid userId,
        string name,
        CancellationToken ct
    )
    {
        return await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<bool> ExistsWithNameAsync(
        Guid userId,
        string name,
        Guid? excludeId,
        CancellationToken ct
    )
    {
        return await _db.Set<CategoryEntity>()
            .AnyAsync(
                c =>
                    c.UserId == userId
                    && c.Name.Trim().ToLower() == name.Trim().ToLower()
                    && (excludeId == null || c.Id != excludeId),
                ct
            );
    }

    public async Task<bool> HasExpensesAsync(Guid userId, Guid categoryId, CancellationToken ct)
    {
        return await _db.Set<ExpenseEntity>()
            .AnyAsync(e => e.CategoryId == categoryId && e.UserId == userId, ct);
    }

    public async Task<int> CountExpensesAsync(Guid userId, Guid categoryId, CancellationToken ct)
    {
        return await _db.Set<ExpenseEntity>()
            .CountAsync(e => e.CategoryId == categoryId && e.UserId == userId, ct);
    }

    public async Task<CategoryEntity> AddAsync(CategoryEntity category, CancellationToken ct)
    {
        await _db.Set<CategoryEntity>().AddAsync(category, ct);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(CategoryEntity category, CancellationToken ct)
    {
        _db.Set<CategoryEntity>().Update(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(CategoryEntity category, CancellationToken ct)
    {
        _db.Set<CategoryEntity>().Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
