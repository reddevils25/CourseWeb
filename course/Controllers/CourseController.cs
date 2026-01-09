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
            var userId = HttpContext.Session.GetInt32("UserId");

            bool isEnrolled = false;

            if (userId.HasValue)
            {
                isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.CourseId == id && e.UserId == userId.Value);
            }

            ViewBag.IsEnrolled = isEnrolled;

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
        [HttpGet]
        public async Task<IActionResult> ViewMyGrades(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                .Include(s => s.Student)
                    .ThenInclude(st => st.User) 
                .Where(s =>
                    s.Student.UserId == userId.Value && 
                    s.Assignment.Lesson.CourseId == courseId
                )
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            ViewBag.Course = course;
            return View("MyGrades", submissions);
        }


        [HttpGet]
        public async Task<IActionResult> SubmitAssignment(int assignmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId.Value);

            ViewBag.ExistingSubmission = existingSubmission;
            return View("SubmitAssignment", assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, string answerText)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Lấy Student từ UserId
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (student == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin học viên" });
            }

            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài tập" });
            }

            if (assignment.Deadline.HasValue && DateTime.Now > assignment.Deadline.Value)
            {
                return Json(new { success = false, message = "Đã quá hạn nộp bài" });
            }

            // Kiểm tra submission hiện có - QUAN TRỌNG: dùng student.StudentId
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s =>
                    s.AssignmentId == assignmentId &&
                    s.StudentId == student.StudentId  // ĐÃ SỬA: dùng student.StudentId thay vì userId
                );

            if (existingSubmission != null)
            {
                existingSubmission.AnswerText = answerText;
                existingSubmission.SubmittedAt = DateTime.Now;
                existingSubmission.Score = null;
                existingSubmission.Feedback = null;
                existingSubmission.GradedAt = null;
            }
            else
            {
                var submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.StudentId,  // ĐÃ SỬA: dùng student.StudentId
                    AnswerText = answerText,
                    SubmittedAt = DateTime.Now
                };
                _context.Submissions.Add(submission);
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Nộp bài thành công!",
                redirectUrl = $"/Course/Learn/{assignment.Lesson.CourseId}"
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddReview(int CourseId, int Rating, string Comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var course = await _context.Courses
                .Where(c => c.CourseId == CourseId)
                .Select(c => new { c.CourseId, c.Alias })
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            if (!userId.HasValue)
            {
                TempData["msg"] = "Vui lòng đăng nhập để đánh giá.";
                return RedirectToAction("Details", "Course", new
                {
                    alias = course.Alias,
                    id = course.CourseId
                });
            }

            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == CourseId && e.UserId == userId.Value);

            if (!enrolled)
            {
                TempData["msg"] = "Bạn cần đăng ký khóa học để đánh giá.";
                return RedirectToAction("Details", "Course", new
                {
                    alias = course.Alias,
                    id = course.CourseId
                });
            }

            var existed = await _context.CourseReviews
                .AnyAsync(r => r.CourseId == CourseId && r.UserId == userId.Value);

            if (existed)
            {
                TempData["msg"] = "Bạn đã đánh giá khóa học này rồi.";
                return RedirectToAction("Details", "Course", new
                {
                    alias = course.Alias,
                    id = course.CourseId
                });
            }

            var review = new CourseReview
            {
                CourseId = CourseId,
                UserId = userId.Value,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.CourseReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["msg"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", "Course", new
            {
                alias = course.Alias,
                id = course.CourseId
            });
        }


    }
}
