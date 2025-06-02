namespace MoneyTracker.Core.Models;

/// <summary>
/// Затраты
/// </summary>
public class Expense
{
    /// <summary>
    /// Время покупки
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Сумма покупки
    /// </summary>
    public Decimal Sum { get; set; }

    /// <summary>
    /// Описание о затратах
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Категория покупки
    /// </summary>
    public Category Category { get; set; }    
}