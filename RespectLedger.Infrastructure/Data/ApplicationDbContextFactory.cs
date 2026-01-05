using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RespectLedger.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer("Server=localhost;Database=RespectLedgerDb;Trusted_Connection=true;TrustServerCertificate=true;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
