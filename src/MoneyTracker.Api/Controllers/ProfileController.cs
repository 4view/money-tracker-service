namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : BaseController
{
    private readonly IUserService _userService;
    private readonly IErrorResponse _errorResponse;

    public ProfileController(IUserService userService, IErrorResponse errorResponse)
    {
        _userService = userService;
        _errorResponse = errorResponse;
    }

    /// <summary>Получить профиль текущего пользователя</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId, ct);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    /// <summary>Обновить имя пользователя</summary>
    [HttpPut("username")]
    public async Task<IActionResult> UpdateUserName(
        [FromBody] UpdateProfileDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.UpdateUserNameAsync(userId, dto, ct);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    /// <summary>Сменить пароль</summary>
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, dto, ct);
            return Ok(new { message = "Пароль успешно изменён" });
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }
}
