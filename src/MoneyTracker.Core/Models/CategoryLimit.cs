namespace MoneyTracker.Core.Models;

public class CategoryLimit
{
    /// <summary>
    /// Id лимита
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Связанная с лимитом категория
    /// </summary>
    public required Category Category { get; set; }

    /// <summary>
    /// Сумма лимита
    /// </summary>
    public decimal Limit { get; set; }
}