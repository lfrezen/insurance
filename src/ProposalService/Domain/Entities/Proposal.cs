using ProposalService.Domain.Exceptions;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Domain.Entities;

public class Proposal
{
    public Guid Id { get; private set; }
    public InsuredPerson InsuredPerson { get; private set; } = null!;
    public string CoverageType { get; private set; } = null!;
    public decimal InsuredAmount { get; private set; }
    public ProposalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Proposal()
    {
    }

    public static Proposal Create(InsuredPerson insuredPerson, string coverageType, decimal insuredAmount)
    {
        if (insuredPerson == null)
            throw new ArgumentNullException(nameof(insuredPerson));

        if (string.IsNullOrWhiteSpace(coverageType))
            throw new DomainException("Coverage type cannot be empty.");

        if (insuredAmount <= 0)
            throw new DomainException("Insured amount must be greater than zero.");

        return new Proposal
        {
            Id = Guid.NewGuid(),
            InsuredPerson = insuredPerson,
            CoverageType = coverageType,
            InsuredAmount = insuredAmount,
            Status = ProposalStatus.EmAnalise,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        if (Status != ProposalStatus.EmAnalise)
            throw new DomainException($"Cannot approve a proposal with status '{Status}'.");

        Status = ProposalStatus.Aprovada;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != ProposalStatus.EmAnalise)
            throw new DomainException($"Cannot reject a proposal with status '{Status}'.");

        Status = ProposalStatus.Rejeitada;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeContracted()
    {
        return Status == ProposalStatus.Aprovada;
    }
}
