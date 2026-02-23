namespace MoneyTracker.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public UserRepository(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _db = dbContext;
        _configuration = configuration;
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Set<UserEntity>().FindAsync([userId], ct);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct)
    {
        return !await _db.Set<UserEntity>().AnyAsync(u => u.Email == email, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await _db.Set<UserEntity>().FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new ResponseException(ErrorType.Validation, "Неверный email или пароль!");
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(
            _configuration["Jwt:Key"] ?? "your-secret-key-here-min-32-characters-long!"
        );

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

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        if (!await IsEmailUniqueAsync(dto.Email, ct))
        {
            throw new ResponseException(
                ErrorType.Conflict,
                "Пользователя с таким Email уже существует"
            );
        }

        var userNameExist = await _db.Set<UserEntity>().AnyAsync(u => u.UserName == dto.UserName);

        if (userNameExist)
        {
            throw new ResponseException(
                ErrorType.Conflict,
                $"Пользователь с таким '{dto.UserName}' уже существует"
            );
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.UserName,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
        };

        await _db.Set<UserEntity>().AddAsync(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
    }
}
