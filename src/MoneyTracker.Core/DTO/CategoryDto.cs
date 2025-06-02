namespace MoneyTracker.Core.DTO;

/// <summary>
/// DTO для использования данных между слоями
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}