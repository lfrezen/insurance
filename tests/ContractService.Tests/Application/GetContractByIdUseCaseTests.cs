using FluentAssertions;

using Moq;

using ContractService.Application.UseCases;
using ContractService.Domain.Entities;
using ContractService.Domain.Ports;

namespace ContractService.Tests.Application;

public class GetContractByIdUseCaseTests
{
    private readonly Mock<IContractRepository> _mockRepository;
    private readonly GetContractByIdUseCase _useCase;

    public GetContractByIdUseCaseTests()
    {
        _mockRepository = new Mock<IContractRepository>();
        _useCase = new GetContractByIdUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnContract_WhenContractExists()
    {
        var contractId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var contract = Contract.Create(proposalId);

        _mockRepository.Setup(x => x.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var result = await _useCase.ExecuteAsync(contractId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(contract.Id);
        result.Value.ProposalId.Should().Be(proposalId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenContractDoesNotExist()
    {
        var contractId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        var result = await _useCase.ExecuteAsync(contractId, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Contract not found.");
    }
}
