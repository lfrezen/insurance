using ProposalService.Domain.Entities;

namespace ProposalService.Domain.Ports;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Proposal>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default);
    Task UpdateAsync(Proposal proposal, CancellationToken cancellationToken = default);
}
