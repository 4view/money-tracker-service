namespace MoneyTracker.Core.DTO;

/// <summary>
/// DTO для использования данных между слоями
/// </summary>
public class ExpenseDto
{
    public Guid Id { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTimeOffset Time { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Sum { get; set; }
}