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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var role = HttpContext.Session.GetString("Role");

            if (role == "Instructor")
            {
                return await InstructorProfile();
            }
            else if (role == "Student")
            {
                return await StudentProfile();
            }

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

            // Lấy danh sách khóa học đã đăng ký
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

        // GET: Profile/InstructorProfile - Trang duy nhất cho instructor
        public async Task<IActionResult> InstructorProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Enrollments)
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(i => i.UserId == userId.Value);

            if (instructor == null)
            {
                return NotFound();
            }

            // Thống kê
            ViewBag.TotalCourses = instructor.Courses.Count;
            ViewBag.TotalStudents = instructor.Courses.Sum(c => c.Enrollments.Count);
            ViewBag.TotalRevenue = instructor.Courses
                .SelectMany(c => c.Enrollments)
                .Where(e => e.IsPaid)
                .Sum(e => e.Amount);
            ViewBag.TotalLessons = instructor.Courses.Sum(c => c.Lessons.Count);

            // Danh sách khóa học với lessons
            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Lessons)
                .Where(c => c.InstructorId == instructor.InstructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;

            return View("InstructorProfile", instructor);

        }

        // POST: Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string bio,
            string experience, string facebook, string linkedin, string website, string mainSubject)
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

            // Nếu là instructor, cập nhật thêm thông tin
            if (user.Role == "Instructor")
            {
                var instructor = await _context.Instructors
                    .FirstOrDefaultAsync(i => i.UserId == userId.Value);

                if (instructor != null)
                {
                    instructor.Bio = bio ?? "";
                    instructor.Experience = experience ?? "";
                    instructor.Facebook = facebook ?? "";
                    instructor.LinkedIn = linkedin ?? "";
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

            // Xóa lessons trước
            _context.Lessons.RemoveRange(course.Lessons);

            // Xóa enrollments
            _context.Enrollments.RemoveRange(course.Enrollments);

            // Xóa course
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa khóa học thành công!" });
        }

        // Helper: Hash Password
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}