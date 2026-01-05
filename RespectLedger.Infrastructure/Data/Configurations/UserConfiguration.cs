using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RespectLedger.Domain.Entities;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nickname)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasMaxLength(500);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserStatus.Pending);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserRole.User);

        builder.Property(u => u.CurrentMana)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(u => u.LastManaReset)
            .HasColumnType("datetime2");

        builder.Property(u => u.TotalXp)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.CurrentLevel)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(u => u.CurrentClass)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Novice");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Nickname)
            .IsUnique();

        // Relationships
        builder.HasMany(u => u.SentRespects)
            .WithOne(r => r.Sender)
            .HasForeignKey(r => r.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReceivedRespects)
            .WithOne(r => r.Receiver)
            .HasForeignKey(r => r.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
