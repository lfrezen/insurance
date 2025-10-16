namespace ContractService.Application.DTOs;

public record CreateContractRequest
{
    public Guid ProposalId { get; init; }
}
