using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RespectLedger.Domain.Entities;
using RespectLedger.Domain.Enums;

namespace RespectLedger.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        if (!context.Users.Any())
        {
            // Create admin user
            User adminUser = new("Admin", "admin@respectledger.com", BCrypt.Net.BCrypt.HashPassword("Admin@123"));
            adminUser.Approve();
            adminUser.PromoteToAdmin();
            context.Users.Add(adminUser);

            // Create initial season
            DateTime now = DateTime.UtcNow;
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            Season currentSeason = new($"Season {now:MMMM yyyy}", startOfMonth, endOfMonth);
            context.Seasons.Add(currentSeason);

            // Create sample achievements
            List<Achievement> achievements = new()
            {
                new("First Respect", AchievementCriteriaType.TotalRespects, 1, "Received your first respect"),
                new("Respected", AchievementCriteriaType.TotalRespects, 10, "Received 10 respects"),
                new("Highly Respected", AchievementCriteriaType.TotalRespects, 50, "Received 50 respects"),
                new("Legend", AchievementCriteriaType.TotalRespects, 100, "Received 100 respects")
            };
            context.Achievements.AddRange(achievements);

            await context.SaveChangesAsync();
        }
    }
}
