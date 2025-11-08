using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder
    )
    {
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
