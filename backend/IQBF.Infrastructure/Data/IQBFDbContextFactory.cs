using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IQBF.Infrastructure.Data;

public class IQBFDbContextFactory : IDesignTimeDbContextFactory<IQBFDbContext>
{
    public IQBFDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("IQBF_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "La variable de entorno IQBF_CONNECTION_STRING no está configurada.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<IQBFDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new IQBFDbContext(optionsBuilder.Options);
    }
}
