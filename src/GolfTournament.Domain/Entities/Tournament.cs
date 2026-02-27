using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class Tournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OrganiserId { get; set; } = string.Empty;
    public TournamentFormat Format { get; set; }
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid CourseId { get; set; }
    public int? MaxPlayers { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    public ApplicationUser Organiser { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public ICollection<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();
}
