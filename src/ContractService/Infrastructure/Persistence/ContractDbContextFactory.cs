using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContractService.Infrastructure.Persistence;

public class ContractDbContextFactory : IDesignTimeDbContextFactory<ContractDbContext>
{
    public ContractDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContractDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Database=ContractServiceDb;Username=postgres;Password=postgres;Port=5432");

        return new ContractDbContext(optionsBuilder.Options);
    }
}
