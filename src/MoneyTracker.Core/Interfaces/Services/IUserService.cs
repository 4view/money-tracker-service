namespace MoneyTracker.Core.Interfaces.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);

    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
}
