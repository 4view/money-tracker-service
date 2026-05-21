namespace MoneyTracker.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailConfirmationAsync(
        string toEmail,
        string userName,
        string confirmationLink,
        CancellationToken ct
    )
    {
        var subject = "Подтверждение email — MoneyTracker";

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 24px; background: #f5f7fa;">
                <div style="background: white; border-radius: 12px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                    <h2 style="color: #2196F3; margin-bottom: 8px;">MoneyTracker</h2>
                    <h3 style="color: #333; margin-bottom: 16px;">Подтвердите ваш email</h3>
                    <p style="color: #666; margin-bottom: 24px;">
                        Привет, <strong>{userName}</strong>! Для завершения регистрации нажмите на кнопку ниже.
                    </p>
                    <a href="{confirmationLink}"
                       style="display: inline-block; padding: 12px 28px; background: #2196F3; color: white;
                              text-decoration: none; border-radius: 8px; font-weight: bold;">
                        Подтвердить email
                    </a>
                    <p style="color: #999; font-size: 13px; margin-top: 24px;">
                        Ссылка действительна 24 часа. Если вы не регистрировались — просто проигнорируйте это письмо.
                    </p>
                </div>
            </div>
            """;

        await SendAsync(toEmail, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken ct
    )
    {
        var subject = "Восстановление пароля — MoneyTracker";

        var body = $"""
            <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; padding: 24px; background: #f5f7fa;">
                <div style="background: white; border-radius: 12px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                    <h2 style="color: #2196F3; margin-bottom: 8px;">MoneyTracker</h2>
                    <h3 style="color: #333; margin-bottom: 16px;">Сброс пароля</h3>
                    <p style="color: #666; margin-bottom: 24px;">
                        Привет, <strong>{userName}</strong>! Для установки нового пароля нажмите на кнопку ниже.
                    </p>
                    <a href="{resetLink}"
                       style="display: inline-block; padding: 12px 28px; background: #2196F3; color: white;
                              text-decoration: none; border-radius: 8px; font-weight: bold;">
                        Сбросить пароль
                    </a>
                    <p style="color: #999; font-size: 13px; margin-top: 24px;">
                        Ссылка действительна 1 час. Если вы не запрашивали сброс — просто проигнорируйте это письмо.
                    </p>
                </div>
            </div>
            """;

        await SendAsync(toEmail, subject, body, ct);
    }

    // ─── Отправка через Gmail SMTP ───────────────────────────────────────────

    private async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct
    )
    {
        var smtpHost =
            _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException("Email:SmtpHost не задан");
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var fromAddress =
            _configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Email:FromAddress не задан");
        var fromName = _configuration["Email:FromName"] ?? "MoneyTracker";
        var password =
            _configuration["Email:Password"]
            ?? throw new InvalidOperationException("Email:Password не задан");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect, ct);
        await client.AuthenticateAsync(fromAddress, password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
