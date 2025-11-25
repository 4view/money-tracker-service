namespace MoneyTracker.Data.Responses;

public class ErrorResponse : IErrorResponse
{
    public IActionResult CreateErrorResponse(Exception ex)
    {
        var errorData = new ErrorDto()
        {
            Code = "500",
            Message = ex.Message
        };

        if (ex is ResponseException responseException)
        {
            if (responseException.ErrorType == ErrorType.Validation)
            {
                errorData.Code = "400";
                return new ObjectResult(errorData) { StatusCode = 400 };
            }
            else if (responseException.ErrorType == ErrorType.NotFound)
            {
                errorData.Code = "404";
                return new ObjectResult(errorData) { StatusCode = 404 };
            }
            else if (responseException.ErrorType == ErrorType.Conflict)
            {
                errorData.Code = "409";
                return new ObjectResult(errorData) { StatusCode = 409 };
            }
        }
        return new ObjectResult(errorData) { StatusCode = 500 };
    }
}