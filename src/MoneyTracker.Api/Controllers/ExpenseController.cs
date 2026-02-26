namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseController : BaseController
{
    private readonly IExpenseService _expenseService;
    private readonly IErrorResponse _errorResponse;

    public ExpenseController(IExpenseService expenseService, IErrorResponse errorResponse)
    {
        _expenseService = expenseService;
        _errorResponse = errorResponse;
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
            var expenses = await _expenseService.GetByPeriodAsync(userId, startDate, endDate, ct);

            return expenses.Any() ? Ok(expenses) : NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseDto dto, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _expenseService.AddAsync(userId, dto, ct);

            return CreatedAtAction(nameof(GetExpenseByTime), new { time = dto.Time }, result);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpPost("scan-qr")]
    public async Task<IActionResult> AddExpenseFromQr(
        [FromBody] ExpenseQrDto dto,
        CancellationToken ct
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _expenseService.AddFromQrAsync(userId, dto, ct);

            return Ok(
                new
                {
                    Success = true,
                    ExpenseId = result.Id,
                    Message = "Расход успешно добавлен из QR-кода",
                }
            );
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
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
            var result = await _expenseService.UpdateAsync(userId, id, dto, ct);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _expenseService.DeleteAsync(userId, id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            return _errorResponse.CreateErrorResponse(ex);
        }
    }
}
