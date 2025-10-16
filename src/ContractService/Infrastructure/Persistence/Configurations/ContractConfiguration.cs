using ContractService.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractService.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.ProposalId)
            .IsRequired();

        builder.Property(c => c.ContractedAt)
            .IsRequired();

        builder.HasIndex(c => c.ProposalId)
            .IsUnique();

        builder.HasIndex(c => c.ContractedAt);
    }
}
