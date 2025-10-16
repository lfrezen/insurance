using ProposalService.Domain.ValueObjects;

namespace ProposalService.Application.DTOs;

public record ProposalResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CoverageType { get; init; } = string.Empty;
    public decimal InsuredAmount { get; init; }
    public ProposalStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
