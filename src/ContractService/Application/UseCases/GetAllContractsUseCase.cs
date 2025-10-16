using ContractService.Application.Common;
using ContractService.Application.DTOs;
using ContractService.Domain.Entities;
using ContractService.Domain.Ports;

namespace ContractService.Application.UseCases;

public class GetAllContractsUseCase
{
    private readonly IContractRepository _repository;

    public GetAllContractsUseCase(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ContractResponse>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contracts = await _repository.GetAllAsync(cancellationToken);
            var responses = contracts.Select(MapToResponse);

            return Result<IEnumerable<ContractResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ContractResponse>>.Failure($"Error retrieving contracts: {ex.Message}");
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
