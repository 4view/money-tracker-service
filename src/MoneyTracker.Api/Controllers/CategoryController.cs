namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _repository;
    private readonly IErrorResponse _errorResponse;

    public CategoryController(ICategoryRepository repository, IErrorResponse errorResponse)
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
    public async Task<IActionResult> GetAllCategories(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var categories = await _repository.GetAllCategoryAsync(userId, ct);

            return Ok(categories);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _repository.GetCategoryByIdAsync(userId, id, ct);

            return Ok(category);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] CategoryDto dto, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _repository.AddCategoryAsync(userId, dto, ct);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                new { category.Id, category.Name }
            );
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] CategoryDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var updCategory = await _repository.UpdateCategoryAsync(userId, id, dto, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _repository.DeleteCategoryAsync(userId, id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}
