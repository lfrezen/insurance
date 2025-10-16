namespace ProposalService.Domain.Events;

public record ProposalApprovedEvent
{
    public Guid ProposalId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CoverageType { get; init; } = string.Empty;
    public decimal InsuredAmount { get; init; }
    public DateTime ApprovedAt { get; init; }
}
