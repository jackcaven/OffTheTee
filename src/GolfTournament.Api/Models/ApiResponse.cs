namespace GolfTournament.Api.Models;

public record ApiResponse<T>(T? Data, string? Error)
{
    public static ApiResponse<T> Success(T data) => new(data, null);
    public static ApiResponse<T> Failure(string error) => new(default, error);
}

public record ApiResponse(object? Data, string? Error)
{
    public static ApiResponse Success(object? data = null) => new(data, null);
    public static ApiResponse Failure(string error) => new(null, error);
}
