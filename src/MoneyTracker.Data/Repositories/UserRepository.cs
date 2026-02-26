namespace MoneyTracker.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().FindAsync([userId], ct);
    }

    public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().AnyAsync(u => u.Email == email, ct);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().AnyAsync(u => u.UserName == username, ct);
    }

    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct)
    {
        await _db.Set<UserEntity>().AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(UserEntity user, CancellationToken ct)
    {
        _db.Set<UserEntity>().Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
