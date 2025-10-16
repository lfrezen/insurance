using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProposalService.Infrastructure.Persistence;

public class ProposalDbContextFactory : IDesignTimeDbContextFactory<ProposalDbContext>
{
    public ProposalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProposalDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Database=ProposalServiceDb;Username=postgres;Password=postgres;Port=5432");

        return new ProposalDbContext(optionsBuilder.Options);
    }
}
