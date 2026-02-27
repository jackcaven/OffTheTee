using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfTournament.Infrastructure.Persistence.Configurations;

public class CourseHoleConfiguration : IEntityTypeConfiguration<CourseHole>
{
    public void Configure(EntityTypeBuilder<CourseHole> builder)
    {
        builder.HasKey(h => h.Id);

        builder.HasIndex(h => new { h.CourseId, h.HoleNumber })
            .IsUnique();

        builder.Property(h => h.Par).IsRequired();
        builder.Property(h => h.StrokeIndex).IsRequired();
    }
}
