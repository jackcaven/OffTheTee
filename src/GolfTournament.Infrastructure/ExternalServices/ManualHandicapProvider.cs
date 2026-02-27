using GolfTournament.Domain.Interfaces;

namespace GolfTournament.Infrastructure.ExternalServices;

/// <summary>
/// Manual handicap provider — always returns null, falling back to manual entry.
/// Replace with WHSHandicapProvider when API credentials are available.
/// </summary>
public class ManualHandicapProvider : IHandicapProvider
{
    public Task<decimal?> GetHandicapIndexAsync(string handicapId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<decimal?>(null);
    }
}
