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
      .Include(c => c.Lessons)             
          .ThenInclude(l => l.Assignments)
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

            var previewVideo = course.Lessons
    .OrderBy(l => l.LessonId)
    .FirstOrDefault()?.VideoUrl;

            ViewBag.PreviewVideo = previewVideo;

            var reviewList = ViewBag.courseReview as List<CourseReview>;

            ViewBag.AvgRating = reviewList.Count > 0
                ? reviewList.Average(r => r.Rating)
                : 0;

            ViewBag.TotalReview = reviewList.Count;


            return View(course);

        }
       
        
        [Route("/Course/Learn/{id}")]
        public async Task<IActionResult> Learn(int id)
        {
            // Lấy course + lessons
            var course = await _context.Courses
                .Include(c => c.Instructor).ThenInclude(i => i.User)
                .Include(c => c.Lessons.OrderBy(l => l.LessonId))
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                TempData["msg"] = "Vui lòng đăng nhập để truy cập nội dung khóa học.";
                return RedirectToAction("Login", "Account");
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == id && e.UserId == userId.Value);

            if (enrollment == null)
            {
                TempData["msg"] = "Bạn chưa đăng ký khóa học này.";
                return RedirectToAction("Details", "Course", new { id = id });
            }

            var firstLesson = course.Lessons.OrderBy(l => l.LessonId).FirstOrDefault();
            var previewVideo = firstLesson?.VideoUrl;

            ViewBag.PreviewVideo = previewVideo;
            ViewBag.Enrollment = enrollment;

            return View(course);
        }
        public async Task<IActionResult> Index(string keyword, string sortOrder)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor).ThenInclude(i => i.User)
                .Include(c => c.Lessons)
                .Include(c => c.CourseReviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Title.Contains(keyword));
            }

            var courses = await query.ToListAsync();

 
            ViewBag.Categories = await _context.CourseCategories.ToListAsync();

            return View(courses);
        }





    }
}
