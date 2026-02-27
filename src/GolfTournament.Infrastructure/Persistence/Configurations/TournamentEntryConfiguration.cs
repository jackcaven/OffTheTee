using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfTournament.Infrastructure.Persistence.Configurations;

public class TournamentEntryConfiguration : IEntityTypeConfiguration<TournamentEntry>
{
    public void Configure(EntityTypeBuilder<TournamentEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.TournamentId, e.PlayerId })
            .IsUnique();

        builder.HasOne(e => e.Player)
            .WithMany(p => p.TournamentEntries)
            .HasForeignKey(e => e.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Scores)
            .WithOne(s => s.TournamentEntry)
            .HasForeignKey(s => s.TournamentEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
