using Microsoft.AspNetCore.Mvc;

using ContractService.Application.DTOs;
using ContractService.Application.UseCases;

namespace ContractService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly CreateContractUseCase _createContractUseCase;
    private readonly GetContractByIdUseCase _getContractByIdUseCase;
    private readonly GetAllContractsUseCase _getAllContractsUseCase;

    public ContractsController(
        CreateContractUseCase createContractUseCase,
        GetContractByIdUseCase getContractByIdUseCase,
        GetAllContractsUseCase getAllContractsUseCase)
    {
        _createContractUseCase = createContractUseCase;
        _getContractByIdUseCase = getContractByIdUseCase;
        _getAllContractsUseCase = getAllContractsUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContract(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createContractUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetContractById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContractById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getContractByIdUseCase.ExecuteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContractResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllContracts(CancellationToken cancellationToken)
    {
        var result = await _getAllContractsUseCase.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
