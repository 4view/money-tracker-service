namespace MoneyTracker.Core;

public class ResponseException : Exception
{
    public ErrorType ErrorType { get; set; }

    public ResponseException(ErrorType errorType, string message)
        : base(message)
    {
        ErrorType = errorType;
    }
}

public enum ErrorType
{
    Validation,
    Conflict,
    NotFound,
}