using Asp.Versioning;
using GolfTournament.Api.Models;
using GolfTournament.Application.Auth;
using MediatR;

namespace GolfTournament.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/auth")
            .WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(ApiResponse<AuthResultDto>.Success(result));
        })
        .WithName("Register")
        .WithSummary("Register a new user account")
        .Produces<ApiResponse<AuthResultDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(ApiResponse<AuthResultDto>.Success(result));
        })
        .WithName("Login")
        .WithSummary("Log in with email and password")
        .Produces<ApiResponse<AuthResultDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
