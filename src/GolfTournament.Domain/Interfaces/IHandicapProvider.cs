namespace GolfTournament.Domain.Interfaces;

public interface IHandicapProvider
{
    Task<decimal?> GetHandicapIndexAsync(string handicapId, CancellationToken cancellationToken = default);
}
