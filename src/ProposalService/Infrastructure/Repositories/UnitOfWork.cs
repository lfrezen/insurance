using ProposalService.Domain.Ports;
using ProposalService.Infrastructure.Persistence;

namespace ProposalService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProposalDbContext _context;

    public UnitOfWork(ProposalDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
