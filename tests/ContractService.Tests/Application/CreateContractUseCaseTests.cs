using FluentAssertions;

using Moq;

using ContractService.Application.DTOs;
using ContractService.Application.UseCases;
using ContractService.Domain.Entities;
using ContractService.Domain.Ports;

namespace ContractService.Tests.Application;

public class CreateContractUseCaseTests
{
    private readonly Mock<IContractRepository> _mockContractRepository;
    private readonly Mock<IProposalClient> _mockProposalClient;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateContractUseCase _useCase;

    public CreateContractUseCaseTests()
    {
        _mockContractRepository = new Mock<IContractRepository>();
        _mockProposalClient = new Mock<IProposalClient>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _useCase = new CreateContractUseCase(
            _mockContractRepository.Object,
            _mockProposalClient.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateContract_WhenProposalIsApproved()
    {
        var proposalId = Guid.NewGuid();
        var request = new CreateContractRequest { ProposalId = proposalId };
        var proposalDto = new ProposalDto { Id = proposalId, Status = "Aprovada" };

        _mockContractRepository.Setup(x => x.GetByProposalIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);
        _mockProposalClient.Setup(x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposalDto);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProposalId.Should().Be(proposalId);

        _mockContractRepository.Verify(x => x.AddAsync(
            It.Is<Contract>(c => c.ProposalId == proposalId),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenProposalAlreadyContracted()
    {
        var proposalId = Guid.NewGuid();
        var request = new CreateContractRequest { ProposalId = proposalId };
        var existingContract = Contract.Create(proposalId);

        _mockContractRepository.Setup(x => x.GetByProposalIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingContract);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("This proposal has already been contracted.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenProposalNotFound()
    {
        var proposalId = Guid.NewGuid();
        var request = new CreateContractRequest { ProposalId = proposalId };

        _mockContractRepository.Setup(x => x.GetByProposalIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);
        _mockProposalClient.Setup(x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProposalDto?)null);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Proposal not found.");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenProposalNotApproved()
    {
        var proposalId = Guid.NewGuid();
        var request = new CreateContractRequest { ProposalId = proposalId };
        var proposalDto = new ProposalDto { Id = proposalId, Status = "EmAnalise" };

        _mockContractRepository.Setup(x => x.GetByProposalIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);
        _mockProposalClient.Setup(x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposalDto);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Cannot contract a proposal with status 'EmAnalise'. Only 'Aprovada' proposals can be contracted.");
    }
}
