using Microsoft.AspNetCore.Identity.Data;

namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IUserService _userService;
    private readonly IErrorResponse _errorResponse;

    public AuthController(IUserService userService, IErrorResponse errorResponse)
    {
        _userService = userService;
        _errorResponse = errorResponse;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _userService.RegisterAsync(dto, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _userService.LoginAsync(dto, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, CancellationToken ct)
    {
        try
        {
            await _userService.ConfirmEmailAsync(token, ct);
            return Ok(new { message = "Email успешно подтвержден" });
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendConfirmationDto dto,
        CancellationToken ct
    )
    {
        try
        {
            await _userService.SendEmailConfirmationAsync(dto.Email, ct);
            return Ok(
                new { message = "Если такой email зарегестрирован, письмо будет отправлено" }
            );
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto,
        CancellationToken ct
    )
    {
        try
        {
            await _userService.ForgotPasswordAsync(dto.Email, ct);
            return Ok(
                new { message = "Если такой email зарегестрирован, письмо будет отправлено" }
            );
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto,
        CancellationToken ct
    )
    {
        try
        {
            await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword, ct);
            return Ok(new { message = "Пароль успешно изменен" });
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }
}
