using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.ViewComponents
{
    public class CourseViewComponent : ViewComponent
    {
        private readonly CourseContext _context;

        public CourseViewComponent(CourseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)           
                    .ThenInclude(i => i.User)         
                .Where(c => c.IsFeatured == true || c.IsNew == true)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }
    }
}
