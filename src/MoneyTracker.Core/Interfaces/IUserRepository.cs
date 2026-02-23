namespace MoneyTracker.Core.Interfaces;

public interface IUserRepository
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
    Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct);
}
