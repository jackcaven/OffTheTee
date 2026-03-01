using GolfTournament.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GolfTournament.Application.Courses;

public record ListCoursesQuery(string? Cursor = null, int Limit = 20) : IRequest<CursorPage<CourseDto>>;

public class ListCoursesQueryHandler : IRequestHandler<ListCoursesQuery, CursorPage<CourseDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCoursesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CursorPage<CourseDto>> Handle(ListCoursesQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 100);
        var cursorName = DecodeCursor(request.Cursor);

        var query = _context.Courses
            .Include(c => c.Holes)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id);

        if (cursorName is not null)
            query = query.Where(c => c.Name.CompareTo(cursorName) > 0 ||
                                     (c.Name == cursorName)).OrderBy(c => c.Name).ThenBy(c => c.Id);

        // Fetch one extra to determine if there's a next page
        var courses = await query.Take(limit + 1).ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (courses.Count > limit)
        {
            courses.RemoveAt(courses.Count - 1);
            nextCursor = EncodeCursor(courses[^1].Name);
        }

        return new CursorPage<CourseDto>(
            courses.Select(CourseDto.FromEntity).ToList(),
            nextCursor);
    }

    private static string? DecodeCursor(string? cursor)
    {
        if (cursor is null) return null;
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        }
        catch
        {
            return null;
        }
    }

    private static string EncodeCursor(string name) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
}
