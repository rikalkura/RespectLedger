using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Infrastructure.Data.Configurations;

public class SeasonResultConfiguration : IEntityTypeConfiguration<SeasonResult>
{
    public void Configure(EntityTypeBuilder<SeasonResult> builder)
    {
        builder.ToTable("SeasonResults");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.SeasonId)
            .IsRequired();

        builder.Property(sr => sr.UserId)
            .IsRequired();

        builder.Property(sr => sr.RankPosition)
            .IsRequired();

        builder.Property(sr => sr.TotalScore)
            .IsRequired();

        builder.Property(sr => sr.RewardSummary)
            .HasMaxLength(255);

        builder.Property(sr => sr.RecordedAt)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(sr => sr.Season)
            .WithMany()
            .HasForeignKey(sr => sr.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
