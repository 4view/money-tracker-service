namespace MoneyTracker.Core.DTO;

/// <summary>
/// DTO для использования данных между слоями
/// </summary>
public class ExpenseDto : IEquatable<ExpenseDto>
{
    public Guid Id { get; set; }
    
    public Guid CategoryId { get; set; }

    public long Time { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Sum { get; set; }

    public bool Equals(ExpenseDto? other)
    {
        if (other == null)
            return false;

        return this.Id == other.Id
            && this.CategoryId == other.CategoryId
            && this.Time == other.Time
            && this.Description == other.Description
            && this.Sum == other.Sum;
    }
}