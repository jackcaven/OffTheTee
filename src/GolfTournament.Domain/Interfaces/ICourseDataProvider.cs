namespace GolfTournament.Domain.Interfaces;

public record CourseImportDto(
    string ExternalId,
    string Name,
    string Location,
    int HoleCount,
    decimal? SlopeRating,
    decimal? CourseRating,
    IReadOnlyList<CourseHoleImportDto> Holes
);

public record CourseHoleImportDto(
    int HoleNumber,
    int Par,
    int StrokeIndex,
    int? Yardage
);

public interface ICourseDataProvider
{
    Task<IReadOnlyList<CourseImportDto>> SearchCourseAsync(string name, CancellationToken cancellationToken = default);
    Task<CourseImportDto?> ImportCourseAsync(string externalId, CancellationToken cancellationToken = default);
}
