using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly CourseContext _context;
        public InstructorsController(CourseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .OrderBy(i => i.InstructorId)
                .Take(10) 
                .ToListAsync();

            return View(instructors);
        }
        [Route("/Instructors/Profile/{id}")]
        public async Task<IActionResult> Profile(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User) 
                .Include(i => i.Courses) 
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null) return NotFound();

            var totalStudents = await _context.Enrollments
                .Where(e => instructor.Courses.Select(c => c.CourseId).Contains(e.CourseId))
                .CountAsync();

            ViewBag.TotalStudents = totalStudents;

            return View(instructor);
        }
    }
}
