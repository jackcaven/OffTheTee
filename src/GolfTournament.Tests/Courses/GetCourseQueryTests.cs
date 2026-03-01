using FluentAssertions;
using GolfTournament.Application.Courses;
using GolfTournament.Domain.Entities;
using GolfTournament.Domain.Enums;
using GolfTournament.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Tests.Courses;

public class GetCourseQueryHandlerTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Course CreateCourse(int holeCount = 18)
    {
        var courseId = Guid.NewGuid();
        return new Course
        {
            Id = courseId,
            Name = "Augusta National",
            Location = "Georgia, USA",
            HoleCount = holeCount,
            SlopeRating = 148m,
            CourseRating = 76.2m,
            CourseDataSource = CourseDataSource.Manual,
            Holes = Enumerable.Range(1, holeCount)
                .Select(i => new CourseHole
                {
                    Id = Guid.NewGuid(),
                    CourseId = courseId,
                    HoleNumber = i,
                    Par = 4,
                    StrokeIndex = i
                }).ToList()
        };
    }

    [Fact]
    public async Task Handle_CourseNotFound_Throws()
    {
        await using var context = CreateInMemoryContext();
        var handler = new GetCourseQueryHandler(context);

        var act = () => handler.Handle(new GetCourseQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_CourseExists_ReturnsCourseDto()
    {
        await using var context = CreateInMemoryContext();
        var course = CreateCourse(18);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var handler = new GetCourseQueryHandler(context);
        var result = await handler.Handle(new GetCourseQuery(course.Id), CancellationToken.None);

        result.Id.Should().Be(course.Id);
        result.Name.Should().Be("Augusta National");
        result.Location.Should().Be("Georgia, USA");
        result.HoleCount.Should().Be(18);
        result.SlopeRating.Should().Be(148m);
        result.CourseRating.Should().Be(76.2m);
        result.CourseDataSource.Should().Be("Manual");
        result.Holes.Should().HaveCount(18);
    }

    [Fact]
    public async Task Handle_CourseExists_HolesOrderedByNumber()
    {
        await using var context = CreateInMemoryContext();
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Name = "Test",
            Location = "Somewhere",
            HoleCount = 9,
            CourseDataSource = CourseDataSource.Manual,
            // Add holes in reverse order to verify sorting
            Holes = Enumerable.Range(1, 9).Reverse()
                .Select(i => new CourseHole
                {
                    Id = Guid.NewGuid(),
                    CourseId = courseId,
                    HoleNumber = i,
                    Par = 4,
                    StrokeIndex = i
                }).ToList()
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var handler = new GetCourseQueryHandler(context);
        var result = await handler.Handle(new GetCourseQuery(courseId), CancellationToken.None);

        result.Holes.Select(h => h.HoleNumber).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_CourseExists_NullRatings_ReturnsNulls()
    {
        await using var context = CreateInMemoryContext();
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Name = "Village Course",
            Location = "Nowhere",
            HoleCount = 9,
            SlopeRating = null,
            CourseRating = null,
            CourseDataSource = CourseDataSource.Manual,
            Holes = Enumerable.Range(1, 9)
                .Select(i => new CourseHole
                {
                    Id = Guid.NewGuid(),
                    CourseId = courseId,
                    HoleNumber = i,
                    Par = 4,
                    StrokeIndex = i
                }).ToList()
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var handler = new GetCourseQueryHandler(context);
        var result = await handler.Handle(new GetCourseQuery(courseId), CancellationToken.None);

        result.SlopeRating.Should().BeNull();
        result.CourseRating.Should().BeNull();
        result.ExternalCourseId.Should().BeNull();
    }
}
