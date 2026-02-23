namespace MoneyTracker.Core.Entities;

public class CategoryLimitEntity
{
    /// <summary>
    /// Id лимита
    /// </summary>
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Связанная с лимитом категория
    /// </summary>
    public required CategoryEntity Category { get; set; }

    /// <summary>
    /// Сумма лимита
    /// </summary>
    public decimal Limit { get; set; }
}