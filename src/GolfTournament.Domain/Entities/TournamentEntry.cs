using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class TournamentEntry
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid PlayerId { get; set; }
    public int PlayingHandicap { get; set; }
    public HandicapCalculationMode HandicapCalculationMode { get; set; } = HandicapCalculationMode.Auto;
    public Guid? FlightId { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public ICollection<Score> Scores { get; set; } = new List<Score>();
}
