using Metriflow.Domain.Entities;
using Metriflow.Domain.enums;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;


public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Page> builder
    )
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(page => page.Path).IsUnique();

        builder
            .Property(page => page.Path)
            .HasConversion<string>();

        builder.HasData(pages());
    }

    private List<Page> pages()
    {
        var pagesCount = (int)enPages.count;
        var pages = new List<Page>(pagesCount);

        for (int i = 1; i < pagesCount; i++)
            pages.Add(new Page
                {
                    Id = i,
                    Path = (enPages)i,
                }
            );
        return pages;
    }
}
