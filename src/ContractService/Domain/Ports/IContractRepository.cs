using ContractService.Domain.Entities;

namespace ContractService.Domain.Ports;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Contract?> GetByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contract>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
}
