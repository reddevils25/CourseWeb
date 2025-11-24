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

        // POST: Admin/Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FullName, string Username, string Password,
     [Bind("DateOfBirth,Gender,Address,Phone,EnrollmentDate")] Student student)
        {
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

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Students/Edit/5
        // GET: Admin/Students/Edit/5
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


        // POST: Admin/Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    
                    var existingStudent = await _context.Students
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.StudentId == id);

                    if (existingStudent == null)
                        return NotFound();

          
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

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.StudentId == student.StudentId))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(student);
        }

        // GET: Admin/Students/Delete/5
        // GET: Admin/Students/Delete/5
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


        // POST: Admin/Students/Delete/5
        // POST: Admin/Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student != null)
            {
                // 1️⃣ Xóa Student trước
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // 2️⃣ Sau đó, nếu User tồn tại thì xóa luôn User (nếu bạn muốn)
                if (student.User != null)
                {
                    _context.Users.Remove(student.User);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}
