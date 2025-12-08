using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.Controllers
{
    public class EventsController : Controller
    {
        private readonly CourseContext _context;
        public EventsController(CourseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var query = _context.Events
                .Include(e => e.Instructor)
                    .ThenInclude(i => i.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.Title.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm) ||
                    e.Location.Contains(searchTerm) ||
                    e.Instructor.User.FullName.Contains(searchTerm)
                );
            }

            var events = await query.ToListAsync();
            return View(events);
        }



    }
}
