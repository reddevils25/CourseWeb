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
    public class AssignmentsController : Controller
    {
        private readonly CourseContext _context;
        public AssignmentsController(CourseContext context)
        {
            _context = context;
        }

        // GET: Admin/Assignments
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                .ToListAsync();

            return View(assignments);
        }

        // GET: Admin/Assignments/Create
        public IActionResult Create()
        {
            // Dropdown bài học
            ViewBag.Lessons = _context.Lessons
                .Select(l => new SelectListItem
                {
                    Value = l.LessonId.ToString(),
                    Text = l.Title
                })
                .ToList();

            return View();
        }

        // POST: Admin/Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
              
                ViewBag.Lessons = _context.Lessons
                    .Select(l => new SelectListItem
                    {
                        Value = l.LessonId.ToString(),
                        Text = l.Title
                    })
                    .ToList();

                return View(assignment);
            }

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Assignments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            ViewBag.Lessons = _context.Lessons
                .Select(l => new SelectListItem
                {
                    Value = l.LessonId.ToString(),
                    Text = l.Title
                })
                .ToList();

            return View(assignment);
        }

        // POST: Admin/Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assignment assignment)
        {
            if (id != assignment.AssignmentId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Lessons = _context.Lessons
                    .Select(l => new SelectListItem
                    {
                        Value = l.LessonId.ToString(),
                        Text = l.Title
                    })
                    .ToList();

                return View(assignment);
            }

            try
            {
                _context.Assignments.Update(assignment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Assignments.Any(a => a.AssignmentId == assignment.AssignmentId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Assignments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // POST: Admin/Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
