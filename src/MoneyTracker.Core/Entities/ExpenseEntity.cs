namespace MoneyTracker.Core.Entities;

public class ExpenseEntity
{
    /// <summary>
    /// Id покупки
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Id категории покупки
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Ссылка на объект категории
    /// </summary>
    public CategoryEntity Category { get; set; } = null!;
    
    /// <summary>
    /// Время покупки
    /// </summary>
    public long TimeUnix { get; set; }

    /// <summary>
    /// Описание о затратах
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Сумма покупки 
    /// </summary>
    public Decimal Sum { get; set; }
}