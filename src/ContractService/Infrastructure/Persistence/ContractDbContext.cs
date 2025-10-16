using ContractService.Domain.Entities;
using ContractService.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;

namespace ContractService.Infrastructure.Persistence;

public class ContractDbContext : DbContext
{
    public ContractDbContext(DbContextOptions<ContractDbContext> options) : base(options)
    {
    }

    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ContractConfiguration());
    }
}
