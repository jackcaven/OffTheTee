using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfTournament.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.InviteCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(t => t.InviteCode)
            .IsUnique();

        builder.HasOne(t => t.Organiser)
            .WithMany()
            .HasForeignKey(t => t.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Course)
            .WithMany(c => c.Tournaments)
            .HasForeignKey(t => t.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Rounds)
            .WithOne(r => r.Tournament)
            .HasForeignKey(r => r.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Entries)
            .WithOne(e => e.Tournament)
            .HasForeignKey(e => e.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
