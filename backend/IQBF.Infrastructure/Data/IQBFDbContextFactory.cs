using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IQBF.Infrastructure.Data;

public class IQBFDbContextFactory : IDesignTimeDbContextFactory<IQBFDbContext>
{
    public IQBFDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IQBFDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=IQBFControlDB_DEV;Trusted_Connection=True;TrustServerCertificate=True;");

        return new IQBFDbContext(optionsBuilder.Options);
    }
}
