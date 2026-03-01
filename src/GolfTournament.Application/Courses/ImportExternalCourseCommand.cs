using FluentValidation;
using GolfTournament.Application.Common;
using GolfTournament.Domain.Entities;
using GolfTournament.Domain.Enums;
using GolfTournament.Domain.Interfaces;
using MediatR;

namespace GolfTournament.Application.Courses;

public record ImportExternalCourseCommand(string ExternalId) : IRequest<CourseDto>;

public class ImportExternalCourseCommandValidator : AbstractValidator<ImportExternalCourseCommand>
{
    public ImportExternalCourseCommandValidator()
    {
        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("ExternalId is required.");
    }
}

public class ImportExternalCourseCommandHandler : IRequestHandler<ImportExternalCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseDataProvider _provider;

    public ImportExternalCourseCommandHandler(IApplicationDbContext context, ICourseDataProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<CourseDto> Handle(ImportExternalCourseCommand request, CancellationToken cancellationToken)
    {
        var imported = await _provider.ImportCourseAsync(request.ExternalId, cancellationToken);

        if (imported is null)
            throw new InvalidOperationException($"Course '{request.ExternalId}' not found in the external provider.");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = imported.Name,
            Location = imported.Location,
            HoleCount = imported.HoleCount,
            SlopeRating = imported.SlopeRating,
            CourseRating = imported.CourseRating,
            CourseDataSource = CourseDataSource.API,
            ExternalCourseId = imported.ExternalId,
            Holes = imported.Holes.Select(h => new CourseHole
            {
                Id = Guid.NewGuid(),
                HoleNumber = h.HoleNumber,
                Par = h.Par,
                StrokeIndex = h.StrokeIndex,
                Yardage = h.Yardage
            }).ToList()
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return CourseDto.FromEntity(course);
    }
}
