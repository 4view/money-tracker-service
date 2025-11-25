namespace MoneyTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : Controller
{
    private readonly IExpenseRepository _repository;

    private readonly IErrorResponse _errorResponse;

    public ExpenseController(IExpenseRepository repository, IErrorResponse errorResponse)
    {
        _repository = repository;
        _errorResponse = errorResponse;
    }

    [HttpGet]
    public async Task<IActionResult> GetExpenseByTime(long startDate, long endDate, CancellationToken ct)
    {
        try
        {
            var expense = await _repository.GetExpenseByTimeAsync(startDate, endDate, ct);

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

    [HttpPost]
    public async Task<IActionResult> AddExpense([FromBody] ExpenseDto dto, CancellationToken ct)
    {        
        try
        {
            var expense = await _repository.AddExpenseAsync(dto, ct);

            return CreatedAtAction(
                nameof(GetExpenseByTime),
                new { time = dto.Time },
                new
                {
                    ExpenseId = expense.Id,
                    CategoryId = expense.Category.Id,
                    Time = expense.TimeUnix,
                    expense.Sum,
                    expense.Description
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
    public async Task<IActionResult> UpdateEaxpense(Guid id, ExpenseDto dto, CancellationToken ct)
    {
        try
        {
            var updatedExpense = await _repository.UpdateExpenseAsync(id, dto, ct);

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
            await _repository.DeleteExpenseAsync(id, ct);

            return NoContent();
        }
        catch (Exception ex)
        {
            var error = _errorResponse.CreateErrorResponse(ex);
            return error;
        }
    }    
}