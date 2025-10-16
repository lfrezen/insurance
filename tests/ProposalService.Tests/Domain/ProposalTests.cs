using FluentAssertions;

using ProposalService.Domain.Entities;
using ProposalService.Domain.Exceptions;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Tests.Domain;

public class ProposalTests
{
    [Fact]
    public void Create_ShouldCreateProposalWithEmAnaliseStatus()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");

        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        proposal.Should().NotBeNull();
        proposal.Id.Should().NotBeEmpty();
        proposal.InsuredPerson.Should().Be(insuredPerson);
        proposal.CoverageType.Should().Be("Vida");
        proposal.InsuredAmount.Should().Be(100000m);
        proposal.Status.Should().Be(ProposalStatus.EmAnalise);
        proposal.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Approve_ShouldChangeStatusToAprovada_WhenStatusIsEmAnalise()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        proposal.Approve();

        proposal.Status.Should().Be(ProposalStatus.Aprovada);
    }

    [Fact]
    public void Approve_ShouldThrowDomainException_WhenStatusIsNotEmAnalise()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);
        proposal.Approve();

        var act = () => proposal.Approve();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot approve a proposal with status 'Aprovada'.");
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejeitada_WhenStatusIsEmAnalise()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        proposal.Reject();

        proposal.Status.Should().Be(ProposalStatus.Rejeitada);
    }

    [Fact]
    public void Reject_ShouldThrowDomainException_WhenStatusIsNotEmAnalise()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);
        proposal.Reject();

        var act = () => proposal.Reject();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot reject a proposal with status 'Rejeitada'.");
    }

    [Fact]
    public void CanBeContracted_ShouldReturnTrue_WhenStatusIsAprovada()
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);
        proposal.Approve();

        var result = proposal.CanBeContracted();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(ProposalStatus.EmAnalise)]
    [InlineData(ProposalStatus.Rejeitada)]
    public void CanBeContracted_ShouldReturnFalse_WhenStatusIsNotAprovada(ProposalStatus status)
    {
        var insuredPerson = new InsuredPerson("John Doe", "07577961094", "john@email.com");
        var proposal = Proposal.Create(insuredPerson, "Vida", 100000m);

        if (status == ProposalStatus.Rejeitada)
            proposal.Reject();

        var result = proposal.CanBeContracted();

        result.Should().BeFalse();
    }
}
