using GolfTournament.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GolfTournament.Application.Courses;

public record GetCourseQuery(Guid Id) : IRequest<CourseDto>;

public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, CourseDto>
{
    private readonly IApplicationDbContext _context;

    public GetCourseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseDto> Handle(GetCourseQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.Holes)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (course is null)
            throw new InvalidOperationException($"Course '{request.Id}' not found.");

        return CourseDto.FromEntity(course);
    }
}
