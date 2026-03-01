using FluentValidation;
using GolfTournament.Application.Common;
using GolfTournament.Domain.Entities;
using GolfTournament.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Application.Courses;

public record UpdateCourseCommand(
    Guid Id,
    string Name,
    string Location,
    int HoleCount,
    decimal? SlopeRating,
    decimal? CourseRating,
    List<CourseHoleInput> Holes
) : IRequest<CourseDto>;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    private static readonly int[] ValidHoleCounts = [9, 12, 18];

    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(300);

        RuleFor(x => x.HoleCount)
            .Must(n => ValidHoleCounts.Contains(n))
            .WithMessage("HoleCount must be 9, 12, or 18.");

        RuleFor(x => x.SlopeRating)
            .InclusiveBetween(55m, 155m)
            .When(x => x.SlopeRating.HasValue)
            .WithMessage("SlopeRating must be between 55 and 155.");

        RuleFor(x => x.CourseRating)
            .InclusiveBetween(60m, 80m)
            .When(x => x.CourseRating.HasValue)
            .WithMessage("CourseRating must be between 60 and 80.");

        RuleFor(x => x.Holes)
            .NotNull()
            .Must((cmd, holes) => holes.Count == cmd.HoleCount)
            .WithMessage(cmd => $"Exactly {cmd.HoleCount} holes are required.");

        RuleForEach(x => x.Holes).SetValidator(new CourseHoleInputValidator());

        RuleFor(x => x.Holes)
            .Must(holes => holes.Select(h => h.HoleNumber).Distinct().Count() == holes.Count)
            .WithMessage("Hole numbers must be unique.")
            .Must(holes => holes.Select(h => h.StrokeIndex).Distinct().Count() == holes.Count)
            .WithMessage("Stroke index values must be unique across all holes.")
            .Must((cmd, holes) => holes.All(h => h.HoleNumber >= 1 && h.HoleNumber <= cmd.HoleCount))
            .WithMessage(cmd => $"Hole numbers must be between 1 and {cmd.HoleCount}.")
            .Must((cmd, holes) => holes.All(h => h.StrokeIndex >= 1 && h.StrokeIndex <= cmd.HoleCount))
            .WithMessage(cmd => $"Stroke index values must be between 1 and {cmd.HoleCount}.");
    }
}

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.Holes)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course is null)
            throw new InvalidOperationException($"Course '{request.Id}' not found.");

        // Guard: cannot edit a course used by an active or completed tournament
        var hasActiveOrCompletedTournament = await _context.Tournaments
            .AnyAsync(t => t.CourseId == request.Id &&
                           (t.Status == TournamentStatus.Active || t.Status == TournamentStatus.Completed),
                      cancellationToken);

        if (hasActiveOrCompletedTournament)
            throw new InvalidOperationException("Cannot edit a course that is used by an active or completed tournament.");

        // Replace all holes
        foreach (var hole in course.Holes.ToList())
            _context.CourseHoles.Remove(hole);

        course.Name = request.Name;
        course.Location = request.Location;
        course.HoleCount = request.HoleCount;
        course.SlopeRating = request.SlopeRating;
        course.CourseRating = request.CourseRating;

        foreach (var h in request.Holes)
        {
            course.Holes.Add(new CourseHole
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                HoleNumber = h.HoleNumber,
                Par = h.Par,
                StrokeIndex = h.StrokeIndex,
                Yardage = h.Yardage
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return CourseDto.FromEntity(course);
    }
}
