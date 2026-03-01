using FluentValidation;
using GolfTournament.Domain.Interfaces;
using MediatR;

namespace GolfTournament.Application.Courses;

public record SearchExternalCoursesQuery(string Name) : IRequest<IReadOnlyList<ExternalCourseDto>>;

public class SearchExternalCoursesQueryValidator : AbstractValidator<SearchExternalCoursesQuery>
{
    public SearchExternalCoursesQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Search name is required.")
            .MinimumLength(2).WithMessage("Search name must be at least 2 characters.");
    }
}

public class SearchExternalCoursesQueryHandler : IRequestHandler<SearchExternalCoursesQuery, IReadOnlyList<ExternalCourseDto>>
{
    private readonly ICourseDataProvider _provider;

    public SearchExternalCoursesQueryHandler(ICourseDataProvider provider)
    {
        _provider = provider;
    }

    public async Task<IReadOnlyList<ExternalCourseDto>> Handle(
        SearchExternalCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _provider.SearchCourseAsync(request.Name, cancellationToken);

        return results.Select(r => new ExternalCourseDto(
            r.ExternalId,
            r.Name,
            r.Location,
            r.HoleCount,
            r.SlopeRating,
            r.CourseRating,
            r.Holes.Select(h => new CourseHoleDto(h.HoleNumber, h.Par, h.StrokeIndex, h.Yardage)).ToList()
        )).ToList();
    }
}
