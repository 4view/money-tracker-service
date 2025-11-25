namespace MoneyTracker.Core.DTO;

public class ReturnedLimitDto : IEquatable<ReturnedLimitDto>
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public decimal Limit { get; set; }

    public decimal Remaining { get; set; }

    public bool Equals(ReturnedLimitDto? other)
    {
        if (other == null)
            return false;

        return this.Id == other.Id
            && this.CategoryId == other.CategoryId
            && this.Limit == other.Limit
            && this.Remaining == other.Remaining;
    }
}