namespace MoneyTracker.Core.DTO;

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResendConfirmationDto
{
    public string Email { get; set; } = string.Empty;
}
