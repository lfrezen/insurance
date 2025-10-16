using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProposalService.Domain.Entities;
using ProposalService.Domain.ValueObjects;

namespace ProposalService.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.CoverageType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.InsuredAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        builder.OwnsOne(p => p.InsuredPerson, ip =>
        {
            ip.Property(i => i.FullName)
                .HasColumnName("InsuredPersonFullName")
                .IsRequired()
                .HasMaxLength(200);

            ip.Property(i => i.Cpf)
                .HasColumnName("InsuredPersonCpf")
                .IsRequired()
                .HasMaxLength(11);

            ip.Property(i => i.Email)
                .HasColumnName("InsuredPersonEmail")
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.Status);
    }
}
