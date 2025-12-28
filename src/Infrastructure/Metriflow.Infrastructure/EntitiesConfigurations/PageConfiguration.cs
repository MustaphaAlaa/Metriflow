using Metriflow.Domain.Entities;
using Metriflow.Domain.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Page> builder
    )
    {
        builder.HasIndex(page => page.Path).IsUnique();
        builder
            .Property(page => page.Path)
            .HasConversion(
                new ValueConverter<enPages, string>(
                    v => v.ToString(),
                    v => (enPages)Enum.Parse(typeof(enPages), v)
                )
            );

        builder.HasData(pages());
    }

    private List<string> pages()
    {
        var pagesCount = (int)enPages.count;
        var pages = new List<string>(pagesCount);

        for (int i = 1; i < pagesCount; i++)
            pages.Add(((enPages)i).ToString());
        return pages;
    }
}
