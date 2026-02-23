namespace MoneyTracker.Data.Configuration;

public class ExpenseConfiguration : IEntityTypeConfiguration<ExpenseEntity>
{
    public void Configure(EntityTypeBuilder<ExpenseEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TimeUnix).IsRequired();
        builder.Property(e => e.Sum).IsRequired();
        builder.Property(e => e.Description);
        builder
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
