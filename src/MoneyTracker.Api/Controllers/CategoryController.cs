namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;
    private readonly IErrorResponse _errorResponse;

    public CategoryController(ICategoryService categoryService, IErrorResponse errorResponse)
    {
        _categoryService = categoryService;
        _errorResponse = errorResponse;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var categories = await _categoryService.GetAllAsync(userId, ct);

            return Ok(categories);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.GetByIdAsync(userId, id, ct);

            return Ok(category);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] CategoryDto dto, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var created = await _categoryService.AddAsync(userId, dto, ct);

            return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
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
            await _categoryService.UpdateAsync(userId, id, dto, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _categoryService.DeleteAsync(userId, id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }
}
