namespace GolfTournament.Application.Auth;

public record AuthResultDto(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string DisplayName
);
