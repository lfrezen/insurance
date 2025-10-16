using FluentAssertions;

using Moq;

using ProposalService.Application.DTOs;
using ProposalService.Application.UseCases;
using ProposalService.Domain.Entities;
using ProposalService.Domain.Ports;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Tests.Application;

public class CreateProposalUseCaseTests
{
    private readonly Mock<IProposalRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateProposalUseCase _useCase;

    public CreateProposalUseCaseTests()
    {
        _mockRepository = new Mock<IProposalRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _useCase = new CreateProposalUseCase(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateProposal_WhenRequestIsValid()
    {
        var request = new CreateProposalRequest
        {
            FullName = "John Doe",
            Cpf = "12345678900",
            Email = "john@email.com",
            CoverageType = "Vida",
            InsuredAmount = 100000m
        };

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().Be(request.FullName);
        result.Value.Cpf.Should().Be(request.Cpf);
        result.Value.Email.Should().Be(request.Email);
        result.Value.Status.Should().Be(ProposalStatus.EmAnalise);

        _mockRepository.Verify(x => x.AddAsync(
            It.Is<Proposal>(p => p.InsuredPerson.FullName == request.FullName),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
