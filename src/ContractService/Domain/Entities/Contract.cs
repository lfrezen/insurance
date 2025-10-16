using ContractService.Domain.Exceptions;

namespace ContractService.Domain.Entities;

public class Contract
{
    public Guid Id { get; private set; }
    public Guid ProposalId { get; private set; }
    public DateTime ContractedAt { get; private set; }

    private Contract()
    {
    }

    public static Contract Create(Guid proposalId)
    {
        if (proposalId == Guid.Empty)
            throw new DomainException("ProposalId cannot be empty.");

        return new Contract
        {
            Id = Guid.NewGuid(),
            ProposalId = proposalId,
            ContractedAt = DateTime.UtcNow
        };
    }
}
