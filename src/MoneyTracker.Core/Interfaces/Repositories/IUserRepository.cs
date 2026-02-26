namespace MoneyTracker.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid userId, CancellationToken ct);

    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct);

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);

    Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct);

    Task UpdateAsync(UserEntity user, CancellationToken ct);
}
