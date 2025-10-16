using ContractService.Application.Common;
using ContractService.Application.DTOs;
using ContractService.Domain.Entities;
using ContractService.Domain.Ports;

namespace ContractService.Application.UseCases;

public class GetContractByIdUseCase
{
    private readonly IContractRepository _repository;

    public GetContractByIdUseCase(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ContractResponse>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _repository.GetByIdAsync(id, cancellationToken);

            if (contract == null)
                return Result<ContractResponse>.Failure("Contract not found.");

            var response = MapToResponse(contract);
            return Result<ContractResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<ContractResponse>.Failure($"Error retrieving contract: {ex.Message}");
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
