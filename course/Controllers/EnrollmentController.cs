using Microsoft.AspNetCore.Mvc;
using course.Models;
using Microsoft.EntityFrameworkCore;

public class EnrollmentController : Controller
{
    private readonly CourseContext _context;

    public EnrollmentController(CourseContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null)
        {
            return Json(new { success = false, message = "Khóa học không tồn tại!" });
        }

        bool existed = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (existed)
        {
            return Json(new { success = false, message = "Bạn đã đăng ký khóa học này!" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (course.Price == 0)
            {
                var enrollment = new Enrollment
                {
                    UserId = userId.Value,
                    CourseId = courseId,
                    EnrollDate = DateTime.Now,
                    Progress = 0,
                    HasCertificate = false,
                    IsPaid = true,
                    Amount = 0,
                    PaymentDate = DateTime.Now
                };

                _context.Enrollments.Add(enrollment);


                course.EnrollCount = (course.EnrollCount ?? 0) + 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Đăng ký khóa học thành công!"
                });
            }

            await transaction.CommitAsync();

            return Json(new
            {
                success = true,
                redirect = Url.Action("Checkout", "Payment", new { courseId })
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return Json(new
            {
                success = false,
                message = "Có lỗi xảy ra, vui lòng thử lại!"
            });
        }
    }

    public IActionResult MyCourses()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var myCourses = _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                    .ThenInclude(i => i.User)
            .Where(e => e.UserId == userId && e.IsPaid == true)
            .ToList();

        return View(myCourses);
    }
}
