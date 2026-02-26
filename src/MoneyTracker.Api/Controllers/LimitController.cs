namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LimitController : BaseController
{
    private readonly ILimitService _limitService;
    private readonly IErrorResponse _errorResponse;

    public LimitController(ILimitService limitService, IErrorResponse errorResponse)
    {
        _limitService = limitService;
        _errorResponse = errorResponse;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLimits(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var limits = await _limitService.GetAllAsync(userId, ct);

            return Ok(limits);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
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
            var result = await _limitService.GetWithCalculationAsync(
                userId,
                limitId,
                categoryId,
                startDate,
                endDate,
                ct
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
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
            var created = await _limitService.AddAsync(userId, dto, ct);

            return CreatedAtAction(
                nameof(GetCategoryLimit),
                new { limitId = created.Id, categoryId = created.CategoryId },
                created
            );
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
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
            await _limitService.UpdateAsync(userId, limitId, dto, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpDelete("{limitId}")]
    public async Task<IActionResult> DeleteCategoryLimit(Guid limitId, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _limitService.DeleteAsync(userId, limitId, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }
}
