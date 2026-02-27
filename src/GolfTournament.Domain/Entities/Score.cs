using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class Score
{
    public Guid Id { get; set; }
    public Guid TournamentEntryId { get; set; }
    public Guid RoundId { get; set; }
    public int HoleNumber { get; set; }
    public int GrossStrokes { get; set; }
    public int? NetStrokes { get; set; }
    public int? StablefordPoints { get; set; }
    public bool? GIR { get; set; }
    public int? Putts { get; set; }
    public ScoreStatus Status { get; set; } = ScoreStatus.Draft;
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? PartnerVerifiedAt { get; set; }

    public TournamentEntry TournamentEntry { get; set; } = null!;
    public Round Round { get; set; } = null!;
}
