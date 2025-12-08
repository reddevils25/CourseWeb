using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using course.Models;
using course.Attributes;

namespace course.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class StudentsController : Controller
    {
        private readonly CourseContext _context;

        public StudentsController(CourseContext context)
        {
            _context = context;
        }

        // GET: Admin/Students
        public async Task<IActionResult> Index()
        {
            var courseContext = _context.Students.Include(s => s.User);
            return View(await courseContext.ToListAsync());
        }

        // GET: Admin/Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Admin/Students/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FullName, string Username, string Password,
     [Bind("DateOfBirth,Gender,Address,Phone,EnrollmentDate")] Student student)
        {
            try
            {
          
                if (_context.Users.Any(u => u.Email == Username))
                {
                    TempData["Error"] = "Email đã tồn tại!";
                    ModelState.AddModelError("Username", "Email này đã được dùng.");
                    return View(student); 
                }

                var newUser = new User
                {
                    FullName = FullName,
                    Email = Username,
                    PasswordHash = Password,
                    Role = "Student",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                student.UserId = newUser.UserId;

                var lastStudent = await _context.Students
                    .OrderByDescending(s => s.StudentId)
                    .FirstOrDefaultAsync();

                int nextId = (lastStudent != null) ? lastStudent.StudentId + 1 : 1;
                student.StudentCode = $"SV{DateTime.Now.Year}{nextId:D4}";

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm sinh viên: " + ex.Message;
                return View(student);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students
                .Include(s => s.User) 
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
                return NotFound();

          
            if (student.User == null)
                student.User = new User();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId)
                return NotFound();

            try
            {
                var existingStudent = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (existingStudent == null)
                    return NotFound();

                // Kiểm tra email trùng với user khác
                if (_context.Users.Any(u => u.Email == student.User.Email && u.UserId != student.UserId))
                {
                    TempData["Error"] = "Email đã được người khác sử dụng!";
                    ModelState.AddModelError("User.Email", "Email này đã tồn tại.");
                    return View(student);
                }

                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.Gender = student.Gender;
                existingStudent.Address = student.Address;
                existingStudent.Phone = student.Phone;
                existingStudent.EnrollmentDate = student.EnrollmentDate;
                existingStudent.StudentCode = student.StudentCode;

                if (existingStudent.User != null)
                {
                    existingStudent.User.FullName = student.User.FullName;
                    existingStudent.User.Email = student.User.Email;

                    if (!string.IsNullOrWhiteSpace(student.User.PasswordHash))
                    {
                        existingStudent.User.PasswordHash = student.User.PasswordHash;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                return View(student);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.StudentId == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student == null)
                {
                    TempData["Error"] = "Không tìm thấy sinh viên để xóa!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                if (student.User != null)
                {
                    _context.Users.Remove(student.User);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Xóa sinh viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}
