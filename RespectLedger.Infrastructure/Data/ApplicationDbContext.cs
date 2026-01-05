using Microsoft.EntityFrameworkCore;
using RespectLedger.Domain.Entities;

namespace RespectLedger.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Respect> Respects => Set<Respect>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<SeasonResult> SeasonResults => Set<SeasonResult>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
