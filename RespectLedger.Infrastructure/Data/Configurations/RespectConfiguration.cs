using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Infrastructure.Data.Configurations;

public class RespectConfiguration : IEntityTypeConfiguration<Respect>
{
    public void Configure(EntityTypeBuilder<Respect> builder)
    {
        builder.ToTable("Respects");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SenderId)
            .IsRequired();

        builder.Property(r => r.ReceiverId)
            .IsRequired();

        builder.Property(r => r.SeasonId)
            .IsRequired();

        builder.Property(r => r.Amount)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(280);

        builder.Property(r => r.Tag)
            .HasMaxLength(50);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime2");

        // Index for cooldown checks
        builder.HasIndex(r => new { r.SenderId, r.ReceiverId });

        // Relationships
        builder.HasOne(r => r.Sender)
            .WithMany(u => u.SentRespects)
            .HasForeignKey(r => r.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Receiver)
            .WithMany(u => u.ReceivedRespects)
            .HasForeignKey(r => r.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Season)
            .WithMany()
            .HasForeignKey(r => r.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
