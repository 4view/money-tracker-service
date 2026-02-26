namespace MoneyTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
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

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.UserName,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
        };

        await _userRepo.AddAsync(user, ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email, ct);

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            throw new ResponseException(ErrorType.Validation, "Неверный email или пароль!");

        // Прозрачная миграция SHA-256 → BCrypt при входе
        if (IsLegacySha256Hash(user.PasswordHash))
        {
            user.PasswordHash = HashPassword(dto.Password);
            await _userRepo.UpdateAsync(user, ct);
        }

        return BuildAuthResponse(user);
    }

    // ─── Приватные вспомогательные ───────────────────────────────────────────

    private static bool IsLegacySha256Hash(string hash) => !hash.StartsWith("$2");

    private bool VerifyPassword(string password, string hash)
    {
        if (IsLegacySha256Hash(hash))
            return HashSha256(password) == hash;

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>Только для проверки старых хешей при миграции.</summary>
    private static string HashSha256(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private AuthResponseDto BuildAuthResponse(UserEntity user)
    {
        return new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = GenerateJwtToken(user),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT ключ не задан. Добавьте 'Jwt:Key' в appsettings.json."
            );

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
