using ContractService.Application.Common;
using ContractService.Application.DTOs;
using ContractService.Domain.Entities;
using ContractService.Domain.Exceptions;
using ContractService.Domain.Ports;

namespace ContractService.Application.UseCases;

public class CreateContractUseCase
{
    private readonly IContractRepository _contractRepository;
    private readonly IProposalClient _proposalClient;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContractUseCase(
        IContractRepository contractRepository,
        IProposalClient proposalClient,
        IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _proposalClient = proposalClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContractResponse>> ExecuteAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingContract = await _contractRepository.GetByProposalIdAsync(
                request.ProposalId,
                cancellationToken);

            if (existingContract != null)
                return Result<ContractResponse>.Failure("This proposal has already been contracted.");

            var proposal = await _proposalClient.GetProposalByIdAsync(
                request.ProposalId,
                cancellationToken);

            if (proposal == null)
                return Result<ContractResponse>.Failure("Proposal not found.");

            if (proposal.Status != "Aprovada")
                return Result<ContractResponse>.Failure($"Cannot contract a proposal with status '{proposal.Status}'. Only 'Aprovada' proposals can be contracted.");

            var contract = Contract.Create(request.ProposalId);

            await _contractRepository.AddAsync(contract, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(contract);
            return Result<ContractResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<ContractResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ContractResponse>.Failure($"Error creating contract: {ex.Message}");
        }
    }

    private static ContractResponse MapToResponse(Contract contract)
    {
        return new ContractResponse
        {
            Id = contract.Id,
            ProposalId = contract.ProposalId,
            ContractedAt = contract.ContractedAt
        };
    }
}
