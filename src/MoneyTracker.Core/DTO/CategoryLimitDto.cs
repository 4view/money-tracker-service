namespace MoneyTracker.Core.DTO;

public class CategoryLimitDto
{
    /// <summary>
    /// Id лимита
    /// </summary>
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    /// <summary>
    /// Лимит категории
    /// </summary>
    public decimal Limit { get; set; }

    /// <summary>
    /// Сумма высчитывающая, сколько осталось потратить средств в категории
    /// </summary>
    public decimal Remaining { get; set; } 
}