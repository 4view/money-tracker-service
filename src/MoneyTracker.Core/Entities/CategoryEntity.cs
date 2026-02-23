namespace MoneyTracker.Core.Entities;

/// <summary>
/// Сущность категории
/// </summary>
public class CategoryEntity
{
    /// <summary>
    /// Id категории
    /// </summary>
    public Guid Id { get; init; }

    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Название категории
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
