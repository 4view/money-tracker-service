namespace MoneyTracker.Data.Configuration;

public class CategoryLimitConfiguration : IEntityTypeConfiguration<CategoryLimitEntity>
{
    public void Configure(EntityTypeBuilder<CategoryLimitEntity> builder)
    {
        builder.HasKey(cl => cl.Id);
        builder.Property(cl => cl.Limit).IsRequired();

        builder
            .HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
