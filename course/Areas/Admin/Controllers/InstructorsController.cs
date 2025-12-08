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
    public class InstructorsController : Controller
    {
        private readonly CourseContext _context;

        public InstructorsController(CourseContext context)
        {
            _context = context;
        }

        // GET: Admin/Instructors
        public async Task<IActionResult> Index()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User) 
                .ToListAsync();

            return View(instructors);
        }

        // GET: Admin/Instructors/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FullName, string Email, string Password, string MainSubject, string Experience, string Bio, string Website)
        {
            try
            {
                if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên, email và mật khẩu.");
                    return View();
                }


                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email này đã được sử dụng.");
                    return View();
                }


                var user = new User
                {
                    FullName = FullName,
                    Email = Email,
                    PasswordHash = Password,
                    Role = "Instructor",
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();


                var instructor = new Instructor
                {
                    UserId = user.UserId,
                    MainSubject = MainSubject,
                    Experience = Experience,
                    Bio = Bio,
                    Website = Website
                };

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm giảng viên thành công!";
                return RedirectToAction(nameof(Index));
             
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tạo giảng viên!";
                return RedirectToAction(nameof(Create));
            }
        }
        // GET: Admin/Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

          
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null)
                return NotFound();

           
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", instructor.UserId);

            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Instructor instructor)
        {
            if (id != instructor.InstructorId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", instructor.UserId);
                return View(instructor);
            }

            try
            {
                var existingInstructor = await _context.Instructors
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.InstructorId == id);

                if (existingInstructor == null)
                    return NotFound();

                existingInstructor.Bio = instructor.Bio;
                existingInstructor.Experience = instructor.Experience;
                existingInstructor.Website = instructor.Website;
                existingInstructor.MainSubject = instructor.MainSubject;

                if (existingInstructor.User != null)
                {
                    var userToUpdate = await _context.Users.FindAsync(existingInstructor.UserId);
                    if (userToUpdate != null)
                    {
                        userToUpdate.FullName = Request.Form["FullName"];
                        userToUpdate.Email = Request.Form["Email"];

                        var password = Request.Form["PasswordHash"];
                        if (!string.IsNullOrWhiteSpace(password))
                            userToUpdate.PasswordHash = password;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật giảng viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật giảng viên!";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }


        // GET: Admin/Instructors/Delete/5
        // GET: Admin/Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

           
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null)
                return NotFound();

            return View(instructor);
        }

        // POST: Admin/Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var instructor = await _context.Instructors
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.InstructorId == id);

                if (instructor == null)
                {
                    TempData["Error"] = "Không tìm thấy giảng viên cần xóa!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();

                if (instructor.User != null)
                {
                    _context.Users.Remove(instructor.User);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Xóa giảng viên thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa giảng viên!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.InstructorId == id);
        }
    }
}
