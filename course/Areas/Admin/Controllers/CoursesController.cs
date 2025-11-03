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
            ViewData["InstructorId"] = new SelectList(
                _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new {
                        InstructorId = i.InstructorId,
                        FullName = i.User.FullName
                    })
                    .ToList(),
                "InstructorId",
                "FullName"
            );

            ViewData["CategoryId"] = new SelectList(_context.CourseCategories, "CategoryId", "Name");

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
           
            if (string.IsNullOrEmpty(course.Alias) && !string.IsNullOrEmpty(course.Title))
            {
                course.Alias = course.Title.ToLower().Replace(" ", "-");
            }

           
            ModelState.Remove("Instructor");
            ModelState.Remove("Category");
            ModelState.Remove("CourseReviews");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Lessons");

            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;
                course.EnrollCount = 0;

                _context.Add(course);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

           
            ViewData["InstructorId"] = new SelectList(
                _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new {
                        InstructorId = i.InstructorId,
                        FullName = i.User.FullName
                    })
                    .ToList(),
                "InstructorId",
                "FullName",
                course.InstructorId
            );

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

            ViewData["InstructorId"] = new SelectList(
                _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new {
                        InstructorId = i.InstructorId,
                        FullName = i.User.FullName
                    })
                    .ToList(),
                "InstructorId",
                "FullName",
                course.InstructorId
            );

            ViewData["CategoryId"] = new SelectList(_context.CourseCategories.ToList(), "CategoryId", "Name", course.CategoryId);

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

            ModelState.Remove("Instructor");
            ModelState.Remove("Category");
            ModelState.Remove("CourseReviews");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Lessons");

            if (!ModelState.IsValid)
            {
                ViewData["InstructorId"] = new SelectList(
                    _context.Instructors
                        .Include(i => i.User)
                        .Select(i => new {
                            InstructorId = i.InstructorId,
                            FullName = i.User.FullName
                        })
                        .ToList(),
                    "InstructorId",
                    "FullName",
                    course.InstructorId
                );

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
                // Lấy course cũ để giữ lại CreatedAt và EnrollCount
                var existingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == id);
                if (existingCourse != null)
                {
                    course.CreatedAt = existingCourse.CreatedAt;
                    course.EnrollCount = existingCourse.EnrollCount;
                }

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
