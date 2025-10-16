using ProposalService.Application.Common;
using ProposalService.Application.DTOs;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Application.UseCases;

public class CreateProposalUseCase
{
    private readonly IProposalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProposalUseCase(IProposalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProposalResponse>> ExecuteAsync(
        CreateProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var insuredPerson = new InsuredPerson(
                request.FullName,
                request.Cpf,
                request.Email
            );

            var proposal = Proposal.Create(
                insuredPerson,
                request.CoverageType,
                request.InsuredAmount
            );

            await _repository.AddAsync(proposal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(proposal);
            return Result<ProposalResponse>.Success(response);
        }
        catch (ArgumentException ex)
        {
            return Result<ProposalResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProposalResponse>.Failure($"Error creating proposal: {ex.Message}");
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
