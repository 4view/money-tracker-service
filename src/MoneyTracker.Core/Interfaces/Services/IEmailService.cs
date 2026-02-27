namespace MoneyTracker.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(
        string toEmail,
        string userName,
        string confirmationLink,
        CancellationToken cancellationToken
    );

    Task SendPasswordResetAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken
    );
}
