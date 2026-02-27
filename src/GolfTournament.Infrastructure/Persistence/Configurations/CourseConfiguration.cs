using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfTournament.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Location)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(c => c.SlopeRating)
            .HasPrecision(5, 1);

        builder.Property(c => c.CourseRating)
            .HasPrecision(5, 1);

        builder.HasMany(c => c.Holes)
            .WithOne(h => h.Course)
            .HasForeignKey(h => h.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
