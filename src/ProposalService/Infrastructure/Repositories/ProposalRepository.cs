using Microsoft.EntityFrameworkCore;

using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;
using ProposalService.Infrastructure.Persistence;

namespace ProposalService.Infrastructure.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly ProposalDbContext _context;

    public ProposalRepository(ProposalDbContext context)
    {
        _context = context;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Proposal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Proposals
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        await _context.Proposals.AddAsync(proposal, cancellationToken);
    }

    public Task UpdateAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        _context.Proposals.Update(proposal);
        return Task.CompletedTask;
    }
}
