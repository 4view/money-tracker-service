namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IErrorResponse _errorResponse;

    public AuthController(IUserRepository userRepository, IErrorResponse errorResponse)
    {
        _userRepository = userRepository;
        _errorResponse = errorResponse;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _userRepository.RegisterAsync(dto, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _userRepository.LoginAsync(dto, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}
