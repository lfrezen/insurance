using FluentAssertions;

using Moq;

using ProposalService.Application.UseCases;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Tests.Application;

public class GetProposalByIdUseCaseTests
{
    private readonly Mock<IProposalRepository> _mockRepository;
    private readonly GetProposalByIdUseCase _useCase;

    public GetProposalByIdUseCaseTests()
    {
        _mockRepository = new Mock<IProposalRepository>();
        _useCase = new GetProposalByIdUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnProposal_WhenProposalExists()
    {
        var proposalId = Guid.NewGuid();
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        _mockRepository.Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);

        var result = await _useCase.ExecuteAsync(proposalId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(proposal.Id);
        result.Value.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenProposalDoesNotExist()
    {
        var proposalId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proposal?)null);

        var result = await _useCase.ExecuteAsync(proposalId, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Proposal not found.");
    }
}
