using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Models;
using System.Security.Cryptography;
using System.Text;

namespace course.Controllers
{
    public class ProfileController : Controller
    {
        private readonly CourseContext _context;

        public ProfileController(CourseContext context)
        {
            _context = context;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            Console.WriteLine("=== PROFILE INDEX CALLED ===");

            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"UserId from session: {userId}");

            if (!userId.HasValue)
            {
                Console.WriteLine("No UserId - Redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            var role = HttpContext.Session.GetString("Role");
            Console.WriteLine($"Role: {role}");

            if (role == "Instructor")
            {
                Console.WriteLine("Going to InstructorProfile");
                return await InstructorProfile();
            }
            else if (role == "Student")
            {
                Console.WriteLine("Going to StudentProfile");
                return await StudentProfile();
            }

            Console.WriteLine("No valid role - Redirecting to Home");
            return RedirectToAction("Index", "Home");
        }

        // GET: Profile/StudentProfile
        public async Task<IActionResult> StudentProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Where(e => e.UserId == userId.Value)
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewBag.TotalCourses = enrollments.Count;
            ViewBag.CompletedCourses = enrollments.Count(e => e.Progress >= 100);
            ViewBag.InProgressCourses = enrollments.Count(e => e.Progress < 100);

            return View("StudentProfile", user);
        }

        public async Task<IActionResult> InstructorProfile()
        {
            Console.WriteLine("=== INSTRUCTOR PROFILE CALLED ===");

            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"UserId: {userId}");

            if (!userId.HasValue)
            {
                Console.WriteLine("No UserId - Redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("Fetching instructor from database...");

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Enrollments)
                        .ThenInclude(e => e.User)
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            if (instructor == null)
            {
                Console.WriteLine($"ERROR: No instructor found for UserId {userId}");
                return NotFound($"Không tìm thấy giảng viên với UserId: {userId}");
            }

            Console.WriteLine($"Instructor found: {instructor.User.FullName}");
            Console.WriteLine($"Courses count: {instructor.Courses.Count}");

            ViewBag.TotalCourses = instructor.Courses.Count;
            ViewBag.TotalStudents = instructor.Courses.Sum(c => c.Enrollments.Count);
            ViewBag.TotalRevenue = instructor.Courses
                .SelectMany(c => c.Enrollments)
                .Where(e => e.IsPaid)
                .Sum(e => e.Amount);
            ViewBag.TotalLessons = instructor.Courses.Sum(c => c.Lessons.Count);

            Console.WriteLine($"ViewBag - TotalCourses: {ViewBag.TotalCourses}");
            Console.WriteLine($"ViewBag - TotalStudents: {ViewBag.TotalStudents}");
            Console.WriteLine($"ViewBag - TotalRevenue: {ViewBag.TotalRevenue}");
            Console.WriteLine($"ViewBag - TotalLessons: {ViewBag.TotalLessons}");

            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.User)
                .Include(c => c.Lessons)
                .Where(c => c.InstructorId == instructor.InstructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            Console.WriteLine($"Courses loaded: {courses.Count}");

            Console.WriteLine("Returning view...");
            return View("InstructorProfile", instructor);
        }
        // POST: Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string bio,
            string experience, string website, string mainSubject)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            user.FullName = fullName;
            user.Email = email;

            if (user.Role == "Instructor")
            {
                var instructor = await _context.Instructors
                    .FirstOrDefaultAsync(i => i.UserId == userId.Value);

                if (instructor != null)
                {
                    instructor.Bio = bio ?? "";
                    instructor.Experience = experience ?? "";
                    instructor.Website = website ?? "";
                    instructor.MainSubject = mainSubject ?? "";
                }
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);

            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới không khớp" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }

            var currentPasswordHash = HashPassword(currentPassword);
            if (user.PasswordHash != currentPasswordHash)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng" });
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        // POST: Profile/RemoveStudent - XÓA HỌC VIÊN KHỎI KHÓA HỌC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int enrollmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            if (instructor == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đăng ký khóa học" });
            }

            if (enrollment.Course.InstructorId != instructor.InstructorId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa học viên này" });
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã xóa học viên {enrollment.User.FullName} khỏi khóa học {enrollment.Course.Title}"
            });
        }

        // POST: Profile/DeleteLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null || lesson.Course.InstructorId != instructor.InstructorId)
            {
                return Json(new { success = false, message = "Không tìm thấy bài giảng hoặc không có quyền xóa" });
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa bài giảng thành công!" });
        }

        // POST: Profile/DeleteCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == instructor.InstructorId);

            if (course == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khóa học hoặc không có quyền xóa" });
            }

            _context.Lessons.RemoveRange(course.Lessons);
            _context.Enrollments.RemoveRange(course.Enrollments);
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa khóa học thành công!" });
        }
        [HttpGet]
        public async Task<IActionResult> ViewAssignments(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            if (instructor == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                     .ThenInclude(st => st.User)
                .Where(a => a.Lesson.CourseId == courseId)
                .OrderByDescending(a => a.AssignmentId)
                .ToListAsync();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == instructor.InstructorId);

            if (course == null)
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            ViewBag.Course = course;
            return View("ViewAssignments", assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(int submissionId, decimal score, string feedback)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            if (instructor == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            var submission = await _context.Submissions
       .Include(s => s.Student)
           .ThenInclude(st => st.User)  
       .Include(s => s.Assignment)
           .ThenInclude(a => a.Lesson)
               .ThenInclude(l => l.Course)
       .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài nộp" });
            }

            if (submission.Assignment.Lesson.Course.InstructorId != instructor.InstructorId)
            {
                return Json(new { success = false, message = "Không có quyền chấm điểm" });
            }

            submission.Score = (double)score;
            submission.Feedback = feedback;
            submission.GradedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã chấm điểm {score}/100 cho {submission.Student.User.FullName}"
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}