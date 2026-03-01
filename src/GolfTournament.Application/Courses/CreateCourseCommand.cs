using FluentValidation;
using GolfTournament.Application.Common;
using GolfTournament.Domain.Entities;
using GolfTournament.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Application.Courses;

public record CreateCourseCommand(
    string Name,
    string Location,
    int HoleCount,
    decimal? SlopeRating,
    decimal? CourseRating,
    List<CourseHoleInput> Holes
) : IRequest<CourseDto>;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    private static readonly int[] ValidHoleCounts = [9, 12, 18];

    public CreateCourseCommandValidator()
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

public class CourseHoleInputValidator : AbstractValidator<CourseHoleInput>
{
    private static readonly int[] ValidPars = [3, 4, 5];

    public CourseHoleInputValidator()
    {
        RuleFor(x => x.Par)
            .Must(p => ValidPars.Contains(p))
            .WithMessage("Par must be 3, 4, or 5.");
    }
}

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCourseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Location = request.Location,
            HoleCount = request.HoleCount,
            SlopeRating = request.SlopeRating,
            CourseRating = request.CourseRating,
            CourseDataSource = CourseDataSource.Manual,
            Holes = request.Holes.Select(h => new CourseHole
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
