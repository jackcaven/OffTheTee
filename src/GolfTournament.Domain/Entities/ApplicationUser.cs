using GolfTournament.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace GolfTournament.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal? HandicapIndex { get; set; }
    public HandicapSource HandicapSource { get; set; } = HandicapSource.Manual;
    public string? HandicapProviderId { get; set; }
}
