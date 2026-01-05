using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Infrastructure.Data.Configurations;

public class RespectLikeConfiguration : IEntityTypeConfiguration<RespectLike>
{
    public void Configure(EntityTypeBuilder<RespectLike> builder)
    {
        builder.ToTable("RespectLikes");

        builder.HasKey(rl => rl.Id);

        builder.Property(rl => rl.RespectId)
            .IsRequired();

        builder.Property(rl => rl.UserId)
            .IsRequired();

        // Unique constraint - one like per user per respect
        builder.HasIndex(rl => new { rl.RespectId, rl.UserId })
            .IsUnique();

        // Relationships
        builder.HasOne(rl => rl.Respect)
            .WithMany()
            .HasForeignKey(rl => rl.RespectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rl => rl.User)
            .WithMany()
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
