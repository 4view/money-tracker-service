namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ExpenseEntity> _logger;

    public ExpenseController(ApplicationDbContext context, ILogger<ExpenseEntity> logger)
    {
        _logger = logger;
        _db = context;
    }

    [HttpGet]
    public IActionResult GetAllExpenses()
    {
        var expenses = _db.Set<ExpenseEntity>()
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryName = e.Category.Name,
                Time = DateTimeOffset.FromUnixTimeSeconds(e.TimeUnix),
                Sum = e.Sum,
                Description = e.Description
            });

        return Ok(expenses);
    }

    [HttpGet("{time}")]
    public async Task<IActionResult> GetExpenseByTime(DateTimeOffset time)
    {
        long timeUnix = time.ToUnixTimeSeconds();

        var expense = await _db.Set<ExpenseEntity>()
            .Where(e => e.TimeUnix == timeUnix)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryName = e.Category.Name,
                Time = DateTimeOffset.FromUnixTimeSeconds(e.TimeUnix),
                Sum = e.Sum,
                Description = e.Description
            })
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return BadRequest(new
            {
                Error = "TimeMatchConflict",
                Message = $"Не было найдено трат по времени {time}"
            });
        }

        return Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseDto dto)
    {
        var timeUnix = dto.Time.ToUnixTimeSeconds();

        var timeConflict = await _db.Set<ExpenseEntity>().AnyAsync(e => e.TimeUnix == timeUnix);

        var expenseCategory = await _db.Set<CategoryEntity>()
            .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == dto.CategoryName.Trim().ToLower());       

        if (timeConflict)
        {
            return BadRequest(new
            {
                Error = "TimeConflict",
                Message = $"Трата с таким временем '{dto.Time}' уже существует"
            });
        }

        if (expenseCategory == null)
            return BadRequest(new
            {
                Error = "NoSuchCategory",
                Message = $"Категории '{dto.CategoryName}' не существует"
            });

        if (dto.Sum <= 0)
            return BadRequest(new
            {
                Error = "SumConflict",
                Message = $"Некорректно указана сумма '{dto.Sum}' в затратах"
            });


        var expense = new ExpenseEntity
        {
            Id = Guid.NewGuid(),
            CategoryId = expenseCategory.Id,
            TimeUnix = timeUnix,
            Sum = dto.Sum,
            Description = dto.Description
        };

        try
        {
            _db.Set<ExpenseEntity>().Add(expense);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetExpenseByTime),
                new { time = dto.Time },
                new
                {
                    expense.Id,
                    expense.Category.Name,
                    Time = DateTimeOffset.FromUnixTimeSeconds(expense.TimeUnix),
                    expense.Sum,
                    expense.Description
                }
            );
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка при создании траты");
            return StatusCode(500, "Ошибка при сохранении в базу данных");
        }
    }
}