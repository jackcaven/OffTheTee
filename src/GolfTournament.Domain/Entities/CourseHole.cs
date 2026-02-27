namespace GolfTournament.Domain.Entities;

public class CourseHole
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int StrokeIndex { get; set; }
    public int? Yardage { get; set; }

    public Course Course { get; set; } = null!;
}
