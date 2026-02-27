using GolfTournament.Domain.Enums;

namespace GolfTournament.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int HoleCount { get; set; } = 18;
    public decimal? SlopeRating { get; set; }
    public decimal? CourseRating { get; set; }
    public CourseDataSource CourseDataSource { get; set; } = CourseDataSource.Manual;
    public string? ExternalCourseId { get; set; }

    public ICollection<CourseHole> Holes { get; set; } = new List<CourseHole>();
    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}
