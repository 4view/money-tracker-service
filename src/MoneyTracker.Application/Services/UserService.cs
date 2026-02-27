using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MoneyTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public UserService(
        IUserRepository userRepo,
        IEmailService emailService,
        IConfiguration configuration
    )
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ResponseException(ErrorType.Validation, "Email не может быть пустым");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            throw new ResponseException(
                ErrorType.Validation,
                "Пароль должен содержать минимум 6 символов"
            );

        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ResponseException(
                ErrorType.Validation,
                "Имя пользователя не может быть пустым"
            );

        if (await _userRepo.EmailExistsAsync(dto.Email, ct))
            throw new ResponseException(
                ErrorType.Conflict,
                "Пользователь с таким Email уже существует"
            );

        if (await _userRepo.UsernameExistsAsync(dto.UserName, ct))
            throw new ResponseException(
                ErrorType.Conflict,
                $"Пользователь с именем '{dto.UserName}' уже существует"
            );

        var confirmationToken = GenerateSecureToken();

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.UserName,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false,
            EmailConfirmationToken = confirmationToken,
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
        };

        await _userRepo.AddAsync(user, ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5500";
        var confirmationLink = $"{frontendUrl}/confirm-email.html?token={confirmationToken}";
        await _emailService.SendEmailConfirmationAsync(
            user.Email,
            user.UserName,
            confirmationLink,
            ct
        );

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email, ct);

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            throw new ResponseException(ErrorType.Validation, "Неверный email или пароль!");

        if (!user.IsEmailConfirmed)
            throw new ResponseException(
                ErrorType.Validation,
                "Email не подтверждён. Проверьте почту."
            );

        if (IsLegacySha256Hash(user.PasswordHash))
        {
            user.PasswordHash = HashPassword(dto.Password);
            await _userRepo.UpdateAsync(user, ct);
        }

        return BuildAuthResponse(user);
    }

    public async Task SendEmailConfirmationAsync(string email, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(email, ct);
        if (user == null || user.IsEmailConfirmed)
            return;

        user.EmailConfirmationToken = GenerateSecureToken();
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await _userRepo.UpdateAsync(user, ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5500";
        var confirmationLink =
            $"{frontendUrl}/confirm-email.html?token={user.EmailConfirmationToken}";
        await _emailService.SendEmailConfirmationAsync(
            user.Email,
            user.UserName,
            confirmationLink,
            ct
        );
    }

    public async Task ConfirmEmailAsync(string token, CancellationToken ct)
    {
        var user =
            await _userRepo.GetByEmailConfirmationTokenAsync(token, ct)
            ?? throw new ResponseException(
                ErrorType.NotFound,
                "Неверная или устаревшая ссылка подтверждения"
            );

        if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            throw new ResponseException(
                ErrorType.Validation,
                "Ссылка подтверждения устарела. Запросите новую."
            );

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        await _userRepo.UpdateAsync(user, ct);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(email, ct);
        if (user == null)
            return;

        user.PasswordResetToken = GenerateSecureToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepo.UpdateAsync(user, ct);

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5500";
        var resetLink = $"{frontendUrl}/reset-password.html?token={user.PasswordResetToken}";
        await _emailService.SendPasswordResetAsync(user.Email, user.UserName, resetLink, ct);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new ResponseException(
                ErrorType.Validation,
                "Пароль должен содержать минимум 6 символов"
            );

        var user =
            await _userRepo.GetByPasswordResetTokenAsync(token, ct)
            ?? throw new ResponseException(
                ErrorType.NotFound,
                "Неверная или устаревшая ссылка сброса пароля"
            );

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new ResponseException(
                ErrorType.Validation,
                "Ссылка сброса устарела. Запросите новую."
            );

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user, ct);
    }

    private static string GenerateSecureToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();

    private static bool IsLegacySha256Hash(string hash) => !hash.StartsWith("$2");

    private bool VerifyPassword(string password, string hash)
    {
        if (IsLegacySha256Hash(hash))
            return HashSha256(password) == hash;
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private static string HashSha256(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private AuthResponseDto BuildAuthResponse(UserEntity user) =>
        new()
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = GenerateJwtToken(user),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

    private string GenerateJwtToken(UserEntity user)
    {
        var jwtKey =
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT ключ не задан.");

        if (jwtKey.Length < 32)
            throw new InvalidOperationException("JWT ключ слишком короткий. Минимум 32 символа.");

        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                ]
            ),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
