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

    /// <summary>
    /// Название категории 
    /// </summary>
    public string Name { get; set; } = string.Empty;
}