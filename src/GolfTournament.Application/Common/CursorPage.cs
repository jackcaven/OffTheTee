namespace GolfTournament.Application.Common;

public record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
