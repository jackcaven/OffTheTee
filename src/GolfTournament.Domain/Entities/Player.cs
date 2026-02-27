using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal? HandicapIndex { get; set; }
    public HandicapSource HandicapSource { get; set; } = HandicapSource.Manual;
    public string? HandicapProviderId { get; set; }

    public ApplicationUser? User { get; set; }
    public ICollection<TournamentEntry> TournamentEntries { get; set; } = new List<TournamentEntry>();
}
