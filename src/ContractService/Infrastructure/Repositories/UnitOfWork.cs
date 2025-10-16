using ContractService.Domain.Ports;
using ContractService.Infrastructure.Persistence;

namespace ContractService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ContractDbContext _context;

    public UnitOfWork(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
