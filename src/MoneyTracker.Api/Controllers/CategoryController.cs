namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _repository;

    private readonly IErrorResponse _errorResponse;

    public CategoryController(ICategoryRepository repository, IErrorResponse errorResponse)
    {
        _repository = repository;
        _errorResponse = errorResponse;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(CancellationToken ct)
    {
        try
        {
            var categories = await _repository.GetAllCategoryAsync(ct);

            return Ok(categories);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetCategoryByName(Guid id)
    {
        try
        {
            var category = _repository.GetCategoryByName(id);

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
            var category = await _repository.AddCategoryAsync(dto, ct);

            return CreatedAtAction(
                nameof(GetCategoryByName),
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
    public async Task<IActionResult> UpdateCategory(Guid id, CategoryDto dto, CancellationToken ct)
    {
        try
        {
            var updCategory = await _repository.UpdateCategoryAsync(id, dto, ct);

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
            await _repository.DeleteCategoryAsync(id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}