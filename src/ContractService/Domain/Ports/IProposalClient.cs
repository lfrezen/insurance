namespace ContractService.Domain.Ports;

public interface IProposalClient
{
    Task<ProposalDto?> GetProposalByIdAsync(Guid proposalId, CancellationToken cancellationToken = default);
}

public record ProposalDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
}
