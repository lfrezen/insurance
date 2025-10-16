using FluentValidation;

using ProposalService.Application.DTOs;

namespace ProposalService.Application.Validators;

public class CreateProposalRequestValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MinimumLength(3).WithMessage("Nome completo deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(BeValidCpf).WithMessage("CPF inválido");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(100).WithMessage("Email deve ter no máximo 100 caracteres");

        RuleFor(x => x.CoverageType)
            .NotEmpty().WithMessage("Tipo de cobertura é obrigatório")
            .Must(BeValidCoverageType).WithMessage("Tipo de cobertura inválido. Valores aceitos: Vida, Auto, Residencial, Empresarial");

        RuleFor(x => x.InsuredAmount)
            .GreaterThan(0).WithMessage("Valor segurado deve ser maior que zero")
            .LessThanOrEqualTo(10_000_000).WithMessage("Valor segurado não pode exceder R$ 10.000.000,00");
    }

    private static bool BeValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

        if (cpf.Length != 11)
            return false;

        if (!cpf.All(char.IsDigit))
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var digits = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        var sum = 0;
        for (int i = 0; i < 9; i++)
            sum += digits[i] * (10 - i);

        var firstDigit = sum % 11;
        firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;

        if (digits[9] != firstDigit)
            return false;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += digits[i] * (11 - i);

        var secondDigit = sum % 11;
        secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;

        return digits[10] == secondDigit;
    }

    private static bool BeValidCoverageType(string coverageType)
    {
        var validTypes = new[] { "Vida", "Auto", "Residencial", "Empresarial" };
        return validTypes.Contains(coverageType, StringComparer.OrdinalIgnoreCase);
    }
}
