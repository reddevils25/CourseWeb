using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.ViewComponents
{
    public class InstructorsViewComponent : ViewComponent
    {
        private readonly CourseContext _context;

        public InstructorsViewComponent(CourseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int top = 5)
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .OrderByDescending(i => _context.Courses
                    .Where(c => c.InstructorId == i.InstructorId)
                    .Join(_context.Enrollments,
                          c => c.CourseId,
                          e => e.CourseId,
                          (c, e) => e)
                    .Count())
                .Take(top)
                .ToListAsync();

            return View(instructors);
        }
    }
}
