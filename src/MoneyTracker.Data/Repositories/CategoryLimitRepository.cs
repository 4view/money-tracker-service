namespace MoneyTracker.Data.Repositories;

public class CategoryLimitRepository : ICategoryLimitRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryLimitRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryLimitEntity>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Set<CategoryLimitEntity>()
            .Include(cl => cl.Category)
            .Where(cl => cl.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<CategoryLimitEntity?> GetByIdAsync(
        Guid userId,
        Guid limitId,
        CancellationToken ct
    )
    {
        return await _db.Set<CategoryLimitEntity>()
            .Include(cl => cl.Category)
            .FirstOrDefaultAsync(cl => cl.Id == limitId && cl.UserId == userId, ct);
    }

    public async Task<bool> ExistsForCategoryAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken ct
    )
    {
        return await _db.Set<CategoryLimitEntity>()
            .AnyAsync(cl => cl.UserId == userId && cl.Category.Id == categoryId, ct);
    }

    public async Task<CategoryLimitEntity> AddAsync(CategoryLimitEntity limit, CancellationToken ct)
    {
        await _db.Set<CategoryLimitEntity>().AddAsync(limit, ct);
        await _db.SaveChangesAsync(ct);
        return limit;
    }

    public async Task UpdateAsync(CategoryLimitEntity limit, CancellationToken ct)
    {
        _db.Set<CategoryLimitEntity>().Update(limit);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(CategoryLimitEntity limit, CancellationToken ct)
    {
        _db.Set<CategoryLimitEntity>().Remove(limit);
        await _db.SaveChangesAsync(ct);
    }
}
