public class UserEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Для подтверждения email
    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiry { get; set; }

    // Для восстановления пароля
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
}
