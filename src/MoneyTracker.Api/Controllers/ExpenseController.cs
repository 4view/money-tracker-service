namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseController : Controller
{
    private readonly IExpenseRepository _repository;
    private readonly IErrorResponse _errorResponse;

    public ExpenseController(IExpenseRepository repository, IErrorResponse errorResponse)
    {
        _repository = repository;
        _errorResponse = errorResponse;
    }

    // Вспомогательный метод для получения ID текущего пользователя
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
    public async Task<IActionResult> GetExpenseByTime(
        long startDate,
        long endDate,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var expense = await _repository.GetExpenseByTimeAsync(userId, startDate, endDate, ct);

            if (!expense.Any())
            {
                return NoContent();
            }

            return Ok(expense);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPost("scan-qr")]
    public async Task<IActionResult> AddExpenseFromQr(
        [FromBody] ExpenseQrDto qrData,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();

            // Пытаемся определить категорию по описанию
            Guid? categoryId = null;

            if (!string.IsNullOrEmpty(qrData.CategoryName))
            {
                var category = await _repository.GetCategoryByNameAsync(
                    userId,
                    qrData.CategoryName,
                    ct
                );
                if (category != null)
                {
                    categoryId = category.Id;
                }
            }

            // Если категория не определена, используем категорию "Другое" или создаем её
            if (categoryId == null)
            {
                var defaultCategory = await _repository.GetOrCreateDefaultCategoryAsync(userId, ct);
                categoryId = defaultCategory.Id;
            }

            var expenseDto = new ExpenseDto
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId.Value,
                Time = qrData.Time,
                Description = qrData.Description,
                Sum = qrData.Sum,
            };

            var expense = await _repository.AddExpenseAsync(userId, expenseDto, ct);

            return Ok(
                new
                {
                    Success = true,
                    ExpenseId = expense.Id,
                    Message = "Расход успешно добавлен из QR-кода",
                }
            );
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseDto dto, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var expense = await _repository.AddExpenseAsync(userId, dto, ct);

            return CreatedAtAction(
                nameof(GetExpenseByTime),
                new { time = dto.Time },
                new
                {
                    ExpenseId = expense.Id,
                    CategoryId = expense.Category.Id,
                    Time = expense.TimeUnix,
                    expense.Sum,
                    expense.Description,
                }
            );
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(
        Guid id,
        [FromBody] ExpenseDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var updatedExpense = await _repository.UpdateExpenseAsync(userId, id, dto, ct);

            return Ok(updatedExpense);
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _repository.DeleteExpenseAsync(userId, id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }
}
