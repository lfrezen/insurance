using ProposalService.Application.Common;
using ProposalService.Application.DTOs;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;

namespace ProposalService.Application.UseCases;

public class GetProposalByIdUseCase
{
    private readonly IProposalRepository _repository;

    public GetProposalByIdUseCase(IProposalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProposalResponse>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var proposal = await _repository.GetByIdAsync(id, cancellationToken);

            if (proposal == null)
                return Result<ProposalResponse>.Failure("Proposal not found.");

            var response = MapToResponse(proposal);
            return Result<ProposalResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<ProposalResponse>.Failure($"Error retrieving proposal: {ex.Message}");
        }
    }

    private static ProposalResponse MapToResponse(Proposal proposal)
    {
        return new ProposalResponse
        {
            Id = proposal.Id,
            FullName = proposal.InsuredPerson.FullName,
            Cpf = proposal.InsuredPerson.Cpf,
            Email = proposal.InsuredPerson.Email,
            CoverageType = proposal.CoverageType,
            InsuredAmount = proposal.InsuredAmount,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            UpdatedAt = proposal.UpdatedAt
        };
    }
}
