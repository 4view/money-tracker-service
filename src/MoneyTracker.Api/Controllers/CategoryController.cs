namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
    {
        _db = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllCategories()
    {
        var categories = _db.Set<CategoryEntity>()
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });

        if (!categories.Any())
        {
            return NotFound();
        }

        return Ok(categories);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetCategoryByName(string name)
    {
        var category = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(c => c.Name == name);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] CategoryDto dto)
    {
        if (await _db.Set<CategoryEntity>().AnyAsync(c => c.Name == dto.Name))
        {
            return BadRequest(new
            {
                Error = "DuplicateCategory",
                Message = $"Категория '{dto.Name}' уже существует"
            });
        }

        var category = new CategoryEntity
        {
            Name = dto.Name
        };

        _db.Set<CategoryEntity>().Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategoryByName),
            new { name = category.Name },
            new { category.Name }
        );
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteCategory(string name)
    {
        var category = await _db.Set<CategoryEntity>().FirstOrDefaultAsync(c => c.Name == name);

        if (category == null)
        {
            return BadRequest( new
            {
                Error = "NoSuchCategory",
                Message = $"Категории '{name}' не существует"
            });
        }

        bool hasRelatedExpenses = await _db.Set<ExpenseEntity>()
            .AnyAsync(e => e.CategoryId == category.Id);

        if (hasRelatedExpenses)
        {
            int expenseCount = await _db.Set<ExpenseEntity>()
                .CountAsync(e => e.CategoryId == category.Id);

            return BadRequest(new
            {
                Error = "CategoryInUse",
                Message = $"Невозможно удалить категорию '{category.Name}'",
                Detailt = $"Категория используется в {expenseCount} расходах"
            });
        }

        try
        {
            _db.Set<CategoryEntity>().Remove(category);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, 
                "Ошибка при удалении категории ID: {CategoryId}", 
                category.Id);
                
            return StatusCode(500, "Ошибка базы данных при удалении категории");
        }
    }
}