using FluentValidation;

using ProposalService.Application.DTOs;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Application.Validators;

public class ChangeProposalStatusRequestValidator : AbstractValidator<ChangeProposalStatusRequest>
{
    public ChangeProposalStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido. Valores aceitos: EmAnalise, Aprovada, Rejeitada");
    }
}
