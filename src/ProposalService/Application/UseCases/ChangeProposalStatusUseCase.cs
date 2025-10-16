using ProposalService.Application.Common;
using ProposalService.Application.DTOs;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Events;
using ProposalService.Domain.Exceptions;
using ProposalService.Domain.Ports;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Application.UseCases;

public class ChangeProposalStatusUseCase
{
    private readonly IProposalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public ChangeProposalStatusUseCase(
        IProposalRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ProposalResponse>> ExecuteAsync(
        Guid id,
        ChangeProposalStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var proposal = await _repository.GetByIdAsync(id, cancellationToken);

            if (proposal == null)
                return Result<ProposalResponse>.Failure("Proposal not found.");

            switch (request.Status)
            {
                case ProposalStatus.Aprovada:
                    proposal.Approve();
                    break;
                case ProposalStatus.Rejeitada:
                    proposal.Reject();
                    break;
                default:
                    return Result<ProposalResponse>.Failure("Invalid status transition.");
            }

            await _repository.UpdateAsync(proposal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.Status == ProposalStatus.Aprovada)
            {
                var approvedEvent = new ProposalApprovedEvent
                {
                    ProposalId = proposal.Id,
                    FullName = proposal.InsuredPerson.FullName,
                    Cpf = proposal.InsuredPerson.Cpf,
                    Email = proposal.InsuredPerson.Email,
                    CoverageType = proposal.CoverageType,
                    InsuredAmount = proposal.InsuredAmount,
                    ApprovedAt = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync(
                    approvedEvent,
                    "proposal.approved",
                    cancellationToken);
            }

            var response = MapToResponse(proposal);
            return Result<ProposalResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<ProposalResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProposalResponse>.Failure($"Error changing proposal status: {ex.Message}");
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
