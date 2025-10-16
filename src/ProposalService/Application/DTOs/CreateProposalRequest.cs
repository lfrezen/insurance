namespace ProposalService.Application.DTOs;

public record CreateProposalRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CoverageType { get; init; } = string.Empty;
    public decimal InsuredAmount { get; init; }
}
