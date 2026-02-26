namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LimitController : Controller
{
    private readonly ICategoryLimitRepository _repository;
    private readonly IErrorResponse _errorResponse;

    public LimitController(ICategoryLimitRepository repository, IErrorResponse errorResponse)
    {
        _repository = repository;
        _errorResponse = errorResponse;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        }
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLimits(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var limitsList = await _repository.GetAllLimitsAsync(userId, ct);
            return Ok(limitsList);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpGet("{limitId}/calculate")]
    public async Task<IActionResult> GetCategoryLimit(
        Guid limitId,
        Guid categoryId,
        long startDate,
        long endDate,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var categoryLimit = await _repository.GetCategoryLimitAsync(
                userId,
                limitId,
                categoryId,
                startDate,
                endDate,
                ct
            );

            return Ok(categoryLimit);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCategoryLimit(
        [FromBody] BaseLimitDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var categoryLimit = await _repository.AddCategoryLimitAsync(userId, dto, ct);

            return CreatedAtAction(
                nameof(GetCategoryLimit),
                new { limitId = categoryLimit.Id, categoryId = categoryLimit.CategoryId },
                categoryLimit
            );
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPut("{limitId}")]
    public async Task<IActionResult> UpdateCategoryLimit(
        Guid limitId,
        [FromBody] BaseLimitDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var updatedLimit = await _repository.UpdateCategoryLimitAsync(userId, limitId, dto, ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpDelete("{limitId}")]
    public async Task<IActionResult> DeleteCategoryLimit(Guid limitId, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _repository.DeleteCategoryLimitAsync(userId, limitId, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}
