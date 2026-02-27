using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfTournament.Infrastructure.Persistence.Configurations;

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.TournamentEntryId, s.RoundId, s.HoleNumber })
            .IsUnique();

        builder.HasOne(s => s.Round)
            .WithMany(r => r.Scores)
            .HasForeignKey(s => s.RoundId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
