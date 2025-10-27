using System;
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
    public class CoursesController : Controller
    {
        private readonly CourseContext _context;

        public CoursesController(CourseContext context)
        {
            _context = context;
        }

        // GET: Admin/Courses
        public async Task<IActionResult> Index()
        {
            // Load Instructor để hiển thị tên giảng viên
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                .Include(c => c.Category) // nếu muốn hiển thị danh mục
                .ToListAsync();

            return View(courses);
        }

        // GET: Admin/Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Admin/Courses/Create
        public IActionResult Create()
        {
            // Load danh sách Instructor để chọn
            ViewData["InstructorId"] = new SelectList(_context.Instructors
                .Include(i => i.User)
                .ToList(), "InstructorId", "User.FullName");

            // Load danh sách Category
            ViewData["CategoryId"] = new SelectList(_context.CourseCategories, "CategoryId", "Name");

            // Dropdown Level tiếng Việt
            ViewData["LevelList"] = new List<SelectListItem>
    {
        new SelectListItem { Value = "Beginner", Text = "Cơ bản" },
        new SelectListItem { Value = "Intermediate", Text = "Trung cấp" },
        new SelectListItem { Value = "Advanced", Text = "Nâng cao" }
    };

            return View();
        }

        // POST: Admin/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InstructorId,Title,Description,Price,Thumbnail,Level,IsFeatured,IsNew,HasCertificate,CategoryId,Alias,Rating")] Course course)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                Console.WriteLine("ModelState errors: " + errors);
            }
            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;
                course.Rating = course.Rating ?? 0;
                course.EnrollCount = 0;

                _context.Add(course);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, load lại dropdown
            ViewData["InstructorId"] = new SelectList(_context.Instructors
                .Include(i => i.User)
                .ToList(), "InstructorId", "User.FullName", course.InstructorId);
            ViewData["CategoryId"] = new SelectList(_context.CourseCategories, "CategoryId", "Name", course.CategoryId);
            ViewData["LevelList"] = new List<SelectListItem>
    {
        new SelectListItem { Value = "Beginner", Text = "Cơ bản" },
        new SelectListItem { Value = "Intermediate", Text = "Trung cấp" },
        new SelectListItem { Value = "Advanced", Text = "Nâng cao" }
    };

            return View(course);
        }

        // GET: Admin/Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // Dropdown giảng viên
            ViewData["InstructorId"] = new SelectList(_context.Instructors.Include(i => i.User).ToList(), "InstructorId", "User.FullName", course.InstructorId);

            // Dropdown danh mục
            ViewData["CategoryId"] = new SelectList(_context.CourseCategories.ToList(), "CategoryId", "Name", course.CategoryId);

            // Dropdown Level tiếng Việt
            ViewData["LevelList"] = new List<SelectListItem>
    {
        new SelectListItem { Value = "Beginner", Text = "Cơ bản" },
        new SelectListItem { Value = "Intermediate", Text = "Trung cấp" },
        new SelectListItem { Value = "Advanced", Text = "Nâng cao" }
    };

            return View(course);
        }


        // POST: Admin/Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,InstructorId,Title,Description,Price,Thumbnail,Level,IsFeatured,IsNew,HasCertificate,Rating,CategoryId,Alias")] Course course)
        {
            if (id != course.CourseId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["InstructorId"] = new SelectList(_context.Instructors.Include(i => i.User).ToList(), "InstructorId", "User.FullName", course.InstructorId);
                ViewData["CategoryId"] = new SelectList(_context.CourseCategories.ToList(), "CategoryId", "Name", course.CategoryId);
                ViewData["LevelList"] = new List<SelectListItem>
        {
            new SelectListItem { Value = "Beginner", Text = "Cơ bản" },
            new SelectListItem { Value = "Intermediate", Text = "Trung cấp" },
            new SelectListItem { Value = "Advanced", Text = "Nâng cao" }
        };
                return View(course);
            }

            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(e => e.CourseId == course.CourseId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Admin/Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
