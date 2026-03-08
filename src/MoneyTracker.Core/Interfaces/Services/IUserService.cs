namespace MoneyTracker.Core.Interfaces.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);

    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);

    Task SendEmailConfirmationAsync(string email, CancellationToken ct);

    Task ConfirmEmailAsync(string token, CancellationToken ct);

    Task ForgotPasswordAsync(string email, CancellationToken ct);

    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct);
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct);

    Task<ProfileDto> UpdateUserNameAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct);

    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken ct);
}
