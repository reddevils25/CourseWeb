﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using course.Models;

namespace course.Areas.Admin.Controllers
{
    [Area("Admin")]
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
                .Include(i => i.User) // load thêm dữ liệu User
                .ToListAsync();

            return View(instructors);
        }
        // GET: Admin/Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.InstructorId == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Admin/Instructors/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/Instructors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FullName, string Email, string Password, string MainSubject, string Experience, string Bio, string Facebook, string LinkedIn, string Website)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên, email và mật khẩu.");
                return View();
            }

            // 🔹 Kiểm tra trùng email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email này đã được sử dụng.");
                return View();
            }

            // 🔹 Tạo tài khoản người dùng (không mã hóa mật khẩu)
            var user = new User
            {
                FullName = FullName,
                Email = Email,
                PasswordHash = Password, // ❗ Lưu plain text (không mã hóa)
                Role = "Instructor",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 🔹 Tạo giảng viên liên kết với User vừa tạo
            var instructor = new Instructor
            {
                UserId = user.UserId,
                MainSubject = MainSubject,
                Experience = Experience,
                Bio = Bio,
                Facebook = Facebook,
                LinkedIn = LinkedIn,
                Website = Website
            };

            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Include User để tránh null reference
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null)
                return NotFound();

            // Nếu bạn muốn dropdown chọn User (nếu có)
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", instructor.UserId);

            return View(instructor);
        }

        // POST: Admin/Instructors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Instructor instructor)
        {
            if (id != instructor.InstructorId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingInstructor = await _context.Instructors
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.InstructorId == id);

                if (existingInstructor == null)
                    return NotFound();

                // Cập nhật Instructor
                existingInstructor.Bio = instructor.Bio;
                existingInstructor.Experience = instructor.Experience;
                existingInstructor.Facebook = instructor.Facebook;
                existingInstructor.LinkedIn = instructor.LinkedIn;
                existingInstructor.Website = instructor.Website;
                existingInstructor.MainSubject = instructor.MainSubject;

                // Cập nhật User từ DB, không bind trực tiếp từ form
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
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", instructor.UserId);
            return View(instructor);
        }

        // GET: Admin/Instructors/Delete/5
        // GET: Admin/Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            // Lấy Instructor kèm User
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
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor != null)
            {
                // 1️⃣ Xóa Instructor trước
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();

                // 2️⃣ Xóa User nếu muốn (chỉ khi chắc chắn User này không dùng bảng khác)
                if (instructor.User != null)
                {
                    _context.Users.Remove(instructor.User);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.InstructorId == id);
        }
    }
}
