namespace MoneyTracker.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasIndex(u => u.UserName).IsUnique();
    }
}
