using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.Controllers
{
    public class PaymentController : Controller
    {
        private readonly CourseContext _context;

        public PaymentController(CourseContext context)
        {
            _context = context;
        }

        // HIỆN TRANG THANH TOÁN + QR
        public IActionResult Checkout(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                bool existed = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

                if (existed)
                    return RedirectToAction("MyCourses", "Enrollment");

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);

                if (course == null)
                    return NotFound();

                var enrollment = new Enrollment
                {
                    UserId = userId.Value,
                    CourseId = courseId,
                    EnrollDate = DateTime.Now,
                    Progress = 0,
                    HasCertificate = false,
                    Amount = course.Price,
                    IsPaid = true,
                    PaymentDate = DateTime.Now
                };

                _context.Enrollments.Add(enrollment);

                // 🔥 DÒNG QUAN TRỌNG NHẤT
                course.EnrollCount = (course.EnrollCount ?? 0) + 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("MyCourses", "Enrollment");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
