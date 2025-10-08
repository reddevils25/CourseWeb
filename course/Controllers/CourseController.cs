using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.Controllers
{
    public class CourseController : Controller
    {
        private readonly CourseContext _context;
        public CourseController(CourseContext context)
        {
            _context = context;
        }
        [Route("/course/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Courses == null)
            {
                return NotFound();
            }


            var course = await _context.Courses
     .Include(c => c.Category)
     .Include(c => c.Instructor)
         .ThenInclude(i => i.User)
     .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.courseReview = _context.CourseReviews
           .Include(r => r.User)
           .Where(r => r.CourseId == id && r.IsActive)
           .ToList();


            ViewBag.courseRelated = _context.Courses
     .Where(c => c.CourseId != id && c.CategoryId == course.CategoryId)
     .OrderByDescending(c => c.CourseId)
     .Take(5)
     .ToList();

            return View(course);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
