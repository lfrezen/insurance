using Microsoft.AspNetCore.Mvc;

using ProposalService.Application.DTOs;
using ProposalService.Application.UseCases;

namespace ProposalService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : ControllerBase
{
    private readonly CreateProposalUseCase _createProposalUseCase;
    private readonly GetProposalByIdUseCase _getProposalByIdUseCase;
    private readonly GetAllProposalsUseCase _getAllProposalsUseCase;
    private readonly ChangeProposalStatusUseCase _changeProposalStatusUseCase;

    public ProposalsController(
        CreateProposalUseCase createProposalUseCase,
        GetProposalByIdUseCase getProposalByIdUseCase,
        GetAllProposalsUseCase getAllProposalsUseCase,
        ChangeProposalStatusUseCase changeProposalStatusUseCase)
    {
        _createProposalUseCase = createProposalUseCase;
        _getProposalByIdUseCase = getProposalByIdUseCase;
        _getAllProposalsUseCase = getAllProposalsUseCase;
        _changeProposalStatusUseCase = changeProposalStatusUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProposal(
        [FromBody] CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createProposalUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetProposalById),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProposalById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getProposalByIdUseCase.ExecuteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProposalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProposals(CancellationToken cancellationToken)
    {
        var result = await _getAllProposalsUseCase.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProposalStatus(
        Guid id,
        [FromBody] ChangeProposalStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _changeProposalStatusUseCase.ExecuteAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error!.Contains("not found"))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
