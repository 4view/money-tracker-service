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
}
