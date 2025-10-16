using FluentAssertions;

using ContractService.Domain.Entities;

namespace ContractService.Tests.Domain;

public class ContractTests
{
    [Fact]
    public void Create_ShouldCreateContractWithProposalId()
    {
        var proposalId = Guid.NewGuid();

        var contract = Contract.Create(proposalId);

        contract.Should().NotBeNull();
        contract.Id.Should().NotBeEmpty();
        contract.ProposalId.Should().Be(proposalId);
        contract.ContractedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
