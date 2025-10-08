using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.ViewComponents
{
    public class BlogViewComponent : ViewComponent
    {
        private readonly CourseContext _context;

        public BlogViewComponent(CourseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.Blogs
                .Include(b => b.User)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(items);
        }
    }
}
