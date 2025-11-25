namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LimitController : Controller
{
    private readonly ICategoryLimitRepository _repository;

    private readonly IErrorResponse _errorResponse;

    public LimitController(ICategoryLimitRepository repository, IErrorResponse errorResponse)
    {
        _repository = repository;
        _errorResponse = errorResponse;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLimits(CancellationToken ct)
    {
        try
        {
            var limitsList = await _repository.GetAllLimitsAsync(ct);
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
        int startDate,
        int endDate,
        CancellationToken ct
    )
    {
        try
        {
            var categoryLimit = await _repository.GetCategoryLimitAsync(
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
    public async Task<IActionResult> AddCategoryLimit(BaseLimitDto dto, CancellationToken ct)
    {
        try
        {
            var categoryLimit = await _repository.AddCategoryLimitAsync(dto, ct);

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
        BaseLimitDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var updatedLimit = await _repository.UpdateCategoryLimitAsync(limitId, dto, ct);
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
            await _repository.DeleteCategoryLimitAsync(limitId, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}
