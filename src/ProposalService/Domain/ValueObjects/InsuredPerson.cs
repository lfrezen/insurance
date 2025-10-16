namespace ProposalService.Domain.ValueObjects;

public record InsuredPerson
{
    public string FullName { get; init; }
    public string Cpf { get; init; }
    public string Email { get; init; }

    public InsuredPerson(string fullName, string cpf, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF cannot be empty.", nameof(cpf));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        FullName = fullName;
        Cpf = cpf;
        Email = email;
    }
}
