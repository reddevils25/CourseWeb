using Microsoft.AspNetCore.Mvc;
using course.Models;
using System;

public class EnrollmentController : Controller
{
    private readonly CourseContext _context;

    public EnrollmentController(CourseContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Enroll(int courseId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        }

        var existing = _context.Enrollments
            .FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);

        if (existing != null)
        {
            return Json(new { success = false, message = "Bạn đã đăng ký khóa học này!" });
        }

        var enrollment = new Enrollment
        {
            UserId = userId.Value,
            CourseId = courseId,
            EnrollDate = DateTime.Now,
            Progress = 0,
            HasCertificate = false,
            IsPaid = false
        };

        _context.Enrollments.Add(enrollment);
        _context.SaveChanges();

        return Json(new { success = true, message = "Đăng ký thành công!" });
    }

}
