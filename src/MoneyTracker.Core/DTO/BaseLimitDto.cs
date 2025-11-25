namespace MoneyTracker.Core.DTO;

public class BaseLimitDto : IEquatable<BaseLimitDto>
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public decimal Limit { get; set; }

    public bool Equals(BaseLimitDto? other)
    {
        if (other == null)
            return false;

        return this.Id == other.Id
            && this.CategoryId == other.CategoryId
            && this.Limit == other.Limit;
    }
}