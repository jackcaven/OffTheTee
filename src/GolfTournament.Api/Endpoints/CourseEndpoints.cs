using Asp.Versioning;
using GolfTournament.Api.Models;
using GolfTournament.Application.Common;
using GolfTournament.Application.Courses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GolfTournament.Api.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        var group = app.MapGroup("/api/v{version:apiVersion}/courses")
            .WithApiVersionSet(versionSet)
            .WithTags("Courses");

        // GET /api/v1/courses/search?name= — must be registered before {id} to avoid routing conflicts
        group.MapGet("/search", async ([FromQuery] string name, IMediator mediator) =>
        {
            var result = await mediator.Send(new SearchExternalCoursesQuery(name));
            return Results.Ok(ApiResponse<IReadOnlyList<ExternalCourseDto>>.Success(result));
        })
        .WithName("SearchExternalCourses")
        .WithSummary("Search for courses in the external provider")
        .Produces<ApiResponse<IReadOnlyList<ExternalCourseDto>>>()
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization();

        // POST /api/v1/courses/import
        group.MapPost("/import", async (ImportExternalCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/courses/{result.Id}", ApiResponse<CourseDto>.Success(result));
        })
        .WithName("ImportExternalCourse")
        .WithSummary("Import a course from the external provider by its external ID")
        .Produces<ApiResponse<CourseDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization();

        // GET /api/v1/courses
        group.MapGet("/", async ([FromQuery] string? cursor, [FromQuery] int limit, IMediator mediator) =>
        {
            var result = await mediator.Send(new ListCoursesQuery(cursor, limit <= 0 ? 20 : limit));
            return Results.Ok(ApiResponse<CursorPage<CourseDto>>.Success(result));
        })
        .WithName("ListCourses")
        .WithSummary("List all courses with cursor-based pagination")
        .Produces<ApiResponse<CursorPage<CourseDto>>>();

        // POST /api/v1/courses
        group.MapPost("/", async (CreateCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/courses/{result.Id}", ApiResponse<CourseDto>.Success(result));
        })
        .WithName("CreateCourse")
        .WithSummary("Create a new course with manual hole data entry")
        .Produces<ApiResponse<CourseDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization();

        // GET /api/v1/courses/{id}
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCourseQuery(id));
            return Results.Ok(ApiResponse<CourseDto>.Success(result));
        })
        .WithName("GetCourse")
        .WithSummary("Get a course by ID including all hole data")
        .Produces<ApiResponse<CourseDto>>()
        .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/v1/courses/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { Id = id });
            return Results.Ok(ApiResponse<CourseDto>.Success(result));
        })
        .WithName("UpdateCourse")
        .WithSummary("Update a course — replaces all hole data")
        .Produces<ApiResponse<CourseDto>>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization();

        return app;
    }
}
