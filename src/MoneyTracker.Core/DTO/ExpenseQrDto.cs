namespace MoneyTracker.Core.DTO;

/// <summary>
/// DTO для данных из QR-кода чека
/// </summary>
public class ExpenseQrDto
{
    /// <summary>
    /// Время покупки (timestamp)
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// Сумма покупки
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Описание/наименование покупки
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Название категории (если удастся определить)
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Дополнительные данные из QR-кода
    /// </summary>
    public Dictionary<string, string>? AdditionalData { get; set; }
}
