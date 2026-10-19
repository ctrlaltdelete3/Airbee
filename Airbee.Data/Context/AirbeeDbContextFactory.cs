using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Airbee.Data.Context;

// Used only by "dotnet ef" to create migrations (no database connection needed for that).
// For "database update" pass the real connection string with --connection.
public class AirbeeDbContextFactory : IDesignTimeDbContextFactory<AirbeeDbContext>
{
    public AirbeeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AirbeeDbContext>();
        options.UseNpgsql("Host=localhost;Port=5432;Database=airbee");

        return new AirbeeDbContext(options.Options);
    }
}
