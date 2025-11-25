namespace MoneyTracker.Core.Interfaces;

public interface IErrorResponse
{
    public IActionResult CreateErrorResponse(Exception ex);
}