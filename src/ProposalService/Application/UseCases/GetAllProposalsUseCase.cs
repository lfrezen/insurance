using ProposalService.Application.Common;
using ProposalService.Application.DTOs;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;

namespace ProposalService.Application.UseCases;

public class GetAllProposalsUseCase
{
    private readonly IProposalRepository _repository;

    public GetAllProposalsUseCase(IProposalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ProposalResponse>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var proposals = await _repository.GetAllAsync(cancellationToken);
            var responses = proposals.Select(MapToResponse);

            return Result<IEnumerable<ProposalResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ProposalResponse>>.Failure($"Error retrieving proposals: {ex.Message}");
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
