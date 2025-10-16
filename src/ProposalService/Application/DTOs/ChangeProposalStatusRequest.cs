using ProposalService.Domain.ValueObjects;

namespace ProposalService.Application.DTOs;

public record ChangeProposalStatusRequest
{
    public ProposalStatus NewStatus { get; init; }
}
