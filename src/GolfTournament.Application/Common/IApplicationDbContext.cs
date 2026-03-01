using GolfTournament.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    DbSet<CourseHole> CourseHoles { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<Round> Rounds { get; }
    DbSet<Player> Players { get; }
    DbSet<TournamentEntry> TournamentEntries { get; }
    DbSet<Score> Scores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
