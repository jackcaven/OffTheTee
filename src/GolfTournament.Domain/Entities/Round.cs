using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class Round
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public int RoundNumber { get; set; }
    public DateOnly Date { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Pending;

    public Tournament Tournament { get; set; } = null!;
    public ICollection<Score> Scores { get; set; } = new List<Score>();
}
