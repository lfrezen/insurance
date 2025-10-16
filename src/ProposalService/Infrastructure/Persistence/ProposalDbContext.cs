using Microsoft.EntityFrameworkCore;

using ProposalService.Domain.Entities;
using ProposalService.Infrastructure.Persistence.Configurations;

namespace ProposalService.Infrastructure.Persistence;

public class ProposalDbContext : DbContext
{
    public ProposalDbContext(DbContextOptions<ProposalDbContext> options) : base(options)
    {
    }

    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProposalConfiguration());
    }
}
