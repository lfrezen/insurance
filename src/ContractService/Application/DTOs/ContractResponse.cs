namespace ContractService.Application.DTOs;

public record ContractResponse
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public DateTime ContractedAt { get; init; }
}
