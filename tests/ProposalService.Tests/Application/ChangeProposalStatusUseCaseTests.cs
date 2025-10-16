using FluentAssertions;

using Moq;

using ProposalService.Application.DTOs;
using ProposalService.Application.UseCases;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Tests.Application;

public class ChangeProposalStatusUseCaseTests
{
    private readonly Mock<IProposalRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ChangeProposalStatusUseCase _useCase;

    public ChangeProposalStatusUseCaseTests()
    {
        _mockRepository = new Mock<IProposalRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _useCase = new ChangeProposalStatusUseCase(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApproveProposal_WhenStatusIsAprovada()
    {
        var proposalId = Guid.NewGuid();
        var insuredPerson = new InsuredPerson("John Doe", "12345678900", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        _mockRepository.Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new ChangeProposalStatusRequest { Status = ProposalStatus.Aprovada };

        var result = await _useCase.ExecuteAsync(proposalId, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ProposalStatus.Aprovada);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectProposal_WhenStatusIsRejeitada()
    {
        var proposalId = Guid.NewGuid();
        var insuredPerson = new InsuredPerson("John Doe", "12345678900", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        _mockRepository.Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new ChangeProposalStatusRequest { Status = ProposalStatus.Rejeitada };

        var result = await _useCase.ExecuteAsync(proposalId, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ProposalStatus.Rejeitada);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenProposalNotFound()
    {
        var proposalId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proposal?)null);

        var request = new ChangeProposalStatusRequest { Status = ProposalStatus.Aprovada };

        var result = await _useCase.ExecuteAsync(proposalId, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Proposal not found.");
    }
}
