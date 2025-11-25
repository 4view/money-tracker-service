namespace MoneyTracker.Core.DTO;

/// <summary>
/// Объект описывающий ошибку
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Сообщение описывающее ошибку
    /// </summary>
    public required string Message { get; set; }
}