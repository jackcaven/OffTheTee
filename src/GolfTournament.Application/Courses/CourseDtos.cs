using GolfTournament.Domain.Entities;

namespace GolfTournament.Application.Courses;

public record CourseHoleInput(int HoleNumber, int Par, int StrokeIndex, int? Yardage);

public record CourseHoleDto(int HoleNumber, int Par, int StrokeIndex, int? Yardage)
{
    public static CourseHoleDto FromEntity(CourseHole hole) =>
        new(hole.HoleNumber, hole.Par, hole.StrokeIndex, hole.Yardage);
}

public record CourseDto(
    Guid Id,
    string Name,
    string Location,
    int HoleCount,
    decimal? SlopeRating,
    decimal? CourseRating,
    string CourseDataSource,
    string? ExternalCourseId,
    IReadOnlyList<CourseHoleDto> Holes)
{
    public static CourseDto FromEntity(Course course) => new(
        course.Id,
        course.Name,
        course.Location,
        course.HoleCount,
        course.SlopeRating,
        course.CourseRating,
        course.CourseDataSource.ToString(),
        course.ExternalCourseId,
        course.Holes.OrderBy(h => h.HoleNumber).Select(CourseHoleDto.FromEntity).ToList());
}

public record ExternalCourseDto(
    string ExternalId,
    string Name,
    string Location,
    int HoleCount,
    decimal? SlopeRating,
    decimal? CourseRating,
    IReadOnlyList<CourseHoleDto> Holes);
