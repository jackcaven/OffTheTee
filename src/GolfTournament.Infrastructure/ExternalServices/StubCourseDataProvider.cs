using GolfTournament.Domain.Interfaces;

namespace GolfTournament.Infrastructure.ExternalServices;

/// <summary>
/// Stub course data provider — returns empty results, falling back to manual entry.
/// Replace with a real implementation (e.g. GolfApiCourseDataProvider) when an API key is available.
/// </summary>
public class StubCourseDataProvider : ICourseDataProvider
{
    public Task<IReadOnlyList<CourseImportDto>> SearchCourseAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CourseImportDto>>(Array.Empty<CourseImportDto>());
    }

    public Task<CourseImportDto?> ImportCourseAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CourseImportDto?>(null);
    }
}
