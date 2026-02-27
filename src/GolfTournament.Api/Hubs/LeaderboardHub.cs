using Microsoft.AspNetCore.SignalR;

namespace GolfTournament.Api.Hubs;

/// <summary>
/// SignalR hub for live leaderboard updates.
/// Clients subscribe to tournament-specific groups to receive real-time score updates.
/// Full implementation is in build step #9 (Leaderboard).
/// </summary>
public class LeaderboardHub : Hub
{
    public async Task SubscribeToTournament(string tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament:{tournamentId}");
    }

    public async Task UnsubscribeFromTournament(string tournamentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament:{tournamentId}");
    }
}
