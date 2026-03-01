using FluentAssertions;
using GolfTournament.Application.Courses;
using GolfTournament.Domain.Enums;
using GolfTournament.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Tests.Courses;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    private static List<CourseHoleInput> ValidHoles(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new CourseHoleInput(i, 4, i, null))
            .ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_EmptyName_Fails(string name)
    {
        var command = new CreateCourseCommand(name, "Location", 18, null, null, ValidHoles(18));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_NameTooLong_Fails()
    {
        var command = new CreateCourseCommand(new string('A', 201), "Location", 18, null, null, ValidHoles(18));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(17)]
    [InlineData(36)]
    public async Task Validate_InvalidHoleCount_Fails(int holeCount)
    {
        var command = new CreateCourseCommand("Test Course", "Location", holeCount, null, null, ValidHoles(holeCount));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "HoleCount");
    }

    [Theory]
    [InlineData(9)]
    [InlineData(12)]
    [InlineData(18)]
    public async Task Validate_ValidHoleCount_Passes(int holeCount)
    {
        var command = new CreateCourseCommand("Test Course", "Location", holeCount, null, null, ValidHoles(holeCount));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_HoleCountMismatch_Fails()
    {
        // HoleCount says 18 but only 9 holes provided
        var command = new CreateCourseCommand("Test", "Location", 18, null, null, ValidHoles(9));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Holes");
    }

    [Fact]
    public async Task Validate_DuplicateStrokeIndex_Fails()
    {
        var holes = ValidHoles(18);
        holes[5] = holes[5] with { StrokeIndex = 1 }; // duplicate SI=1
        var command = new CreateCourseCommand("Test", "Location", 18, null, null, holes);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Stroke index"));
    }

    [Fact]
    public async Task Validate_DuplicateHoleNumber_Fails()
    {
        var holes = ValidHoles(18);
        holes[5] = holes[5] with { HoleNumber = 1 }; // duplicate hole 1
        var command = new CreateCourseCommand("Test", "Location", 18, null, null, holes);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task Validate_InvalidPar_Fails(int par)
    {
        var holes = ValidHoles(18);
        holes[0] = holes[0] with { Par = par };
        var command = new CreateCourseCommand("Test", "Location", 18, null, null, holes);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_StrokeIndexOutOfRange_Fails()
    {
        // 18-hole course, stroke index 19 is out of range
        var holes = ValidHoles(18);
        holes[0] = holes[0] with { HoleNumber = 19, StrokeIndex = 19 };
        var command = new CreateCourseCommand("Test", "Location", 18, null, null, holes);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ValidSlopeRating_Passes()
    {
        var command = new CreateCourseCommand("Test", "Location", 18, 113m, 70.1m, ValidHoles(18));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(54.9)]
    [InlineData(155.1)]
    public async Task Validate_SlopeRatingOutOfRange_Fails(double slope)
    {
        var command = new CreateCourseCommand("Test", "Location", 18, (decimal)slope, null, ValidHoles(18));
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_NineHoleCourse_StrokeIndexRange1to9_Passes()
    {
        var holes = Enumerable.Range(1, 9)
            .Select(i => new CourseHoleInput(i, 4, i, null))
            .ToList();
        var command = new CreateCourseCommand("9 Hole Course", "Location", 9, null, null, holes);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }
}

public class CreateCourseCommandHandlerTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static List<CourseHoleInput> ValidHoles(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new CourseHoleInput(i, i % 2 == 0 ? 4 : (i % 3 == 0 ? 5 : 3), i, null))
            .ToList();

    [Fact]
    public async Task Handle_ValidCommand_CreatesCourseWithHoles()
    {
        await using var context = CreateInMemoryContext();
        var handler = new CreateCourseCommandHandler(context);

        var command = new CreateCourseCommand(
            "Sunningdale", "Berkshire, England", 18, 130m, 72.4m, ValidHoles(18));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Sunningdale");
        result.HoleCount.Should().Be(18);
        result.CourseDataSource.Should().Be("Manual");
        result.Holes.Should().HaveCount(18);
        result.SlopeRating.Should().Be(130m);
        result.CourseRating.Should().Be(72.4m);

        // Verify persisted in DB
        var persisted = await context.Courses.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        context.CourseHoles.Count(h => h.CourseId == result.Id).Should().Be(18);
    }

    [Fact]
    public async Task Handle_ValidCommand_HolesOrderedByNumber()
    {
        await using var context = CreateInMemoryContext();
        var handler = new CreateCourseCommandHandler(context);

        // Provide holes in reverse order
        var holes = Enumerable.Range(1, 9).Reverse()
            .Select(i => new CourseHoleInput(i, 4, i, null))
            .ToList();

        var command = new CreateCourseCommand("Test", "Location", 9, null, null, holes);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Holes.Select(h => h.HoleNumber).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_CourseDataSource_SetToManual()
    {
        await using var context = CreateInMemoryContext();
        var handler = new CreateCourseCommandHandler(context);

        var command = new CreateCourseCommand("Test", "Location", 18, null, null, ValidHoles(18));
        var result = await handler.Handle(command, CancellationToken.None);

        result.CourseDataSource.Should().Be(CourseDataSource.Manual.ToString());
        result.ExternalCourseId.Should().BeNull();
    }
}
