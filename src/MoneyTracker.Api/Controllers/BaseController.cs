namespace MoneyTracker.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        return userId;
    }
}
