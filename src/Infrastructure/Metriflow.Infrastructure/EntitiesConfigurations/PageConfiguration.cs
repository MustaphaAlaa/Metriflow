using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Page> builder
    )
    {
        builder.HasIndex(page => page.Path).IsUnique();
    }
}
