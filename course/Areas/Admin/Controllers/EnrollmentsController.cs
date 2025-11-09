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
    public class EnrollmentsController : Controller
    {
        private readonly CourseContext _context;

        public EnrollmentsController(CourseContext context)
        {
            _context = context;
        }

        // GET: Admin/Enrollments
        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .ToListAsync();

            return View(enrollments);
        }

 
        // GET: Admin/Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", enrollment.UserId);

            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnrollmentId,UserId,CourseId,EnrollDate,Progress,HasCertificate,Amount,IsPaid,PaymentDate")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId) return NotFound();
            if (enrollment.EnrollDate == default(DateTime))
            {
                var oldEnrollment = await _context.Enrollments.AsNoTracking().FirstOrDefaultAsync(e => e.EnrollmentId == id);
                if (oldEnrollment != null)
                    enrollment.EnrollDate = oldEnrollment.EnrollDate;
                else
                    enrollment.EnrollDate = DateTime.Now;
            } 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Enrollments.Any(e => e.EnrollmentId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }


            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "FullName", enrollment.UserId);
            return View(enrollment);
        }
        
        // GET: Admin/Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.EnrollmentId == id);
            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        // POST: Admin/Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            enrollment.IsPaid = true;
            enrollment.PaymentDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentId == id);
        }
    }
}
