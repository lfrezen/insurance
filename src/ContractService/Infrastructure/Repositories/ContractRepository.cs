using ContractService.Domain.Entities;
using ContractService.Domain.Ports;
using ContractService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractService.Infrastructure.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ContractDbContext _context;

    public ContractRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contract?> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.ProposalId == proposalId, cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .OrderByDescending(c => c.ContractedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _context.Contracts.AddAsync(contract, cancellationToken);
    }
}
